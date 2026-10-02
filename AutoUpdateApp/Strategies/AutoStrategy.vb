Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text.RegularExpressions

Namespace Strategies

    Public Class AutoStrategy
        Implements IUpdateStrategy

        Public Const CheckIntervalSeconds As Integer = 2

        Private _invokeControl As System.Windows.Forms.Control

        Public Sub New(Optional invokeControl As System.Windows.Forms.Control = Nothing)
            _invokeControl = invokeControl
        End Sub

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            ' In AUTO mode, periodic checking of the 30-minute stop condition is handled
            ' independently by MainForm via AutoModeTimer every CheckIntervalSeconds.
            Return UpdateResult.NoAction
        End Function

        Public Shared Function CheckAutoCondition(computerName As String) As Boolean
            Try
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester Is Nothing OrElse Not String.Equals(tester.Mode, "AUTO", StringComparison.OrdinalIgnoreCase) Then
                    Return False
                End If

                Dim watchFolder As String = Config.AppSettings.AutoWatchFolderPath
                Dim stopLogFolder As String = Config.AppSettings.AutoStopLogFolderPath

                If String.IsNullOrEmpty(watchFolder) OrElse String.IsNullOrEmpty(stopLogFolder) Then
                    Return False
                End If

                If Not File.Exists(watchFolder) AndAlso Not Directory.Exists(watchFolder) Then Return False
                If Not File.Exists(stopLogFolder) AndAlso Not Directory.Exists(stopLogFolder) Then Return False

                Dim watchPath As String = FindLatestFile(watchFolder, computerName)
                Dim stopLogPath As String = FindLatestFile(stopLogFolder, computerName)

                If String.IsNullOrEmpty(watchPath) OrElse String.IsNullOrEmpty(stopLogPath) Then
                    Return False
                End If

                Dim endOfTestTime As DateTime = ReadEndOfTestTime(watchPath)
                Dim stopTime As DateTime = ReadLatestStopTime(stopLogPath)

                If endOfTestTime = DateTime.MinValue OrElse stopTime = DateTime.MinValue Then
                    Return False
                End If

                ' Prevent using an old endOfTestTime from multiple days ago against a recent stop
                If stopTime > endOfTestTime AndAlso (stopTime - endOfTestTime).TotalHours > 24 Then
                    Managers.LogManager.Info("Auto Condition: False (gap > 24h, skipped)")
                    Return False
                End If

                Dim result As Boolean = (endOfTestTime < stopTime)
                Managers.LogManager.Info(String.Format("Auto Condition: {0} (EndOfTest={1}, Stop={2})",
                    If(result, "True", "False"),
                    endOfTestTime.ToString("MM/dd HH:mm:ss"),
                    stopTime.ToString("MM/dd HH:mm:ss")))
                Return result
            Catch ex As Exception
                Managers.LogManager.Error("Auto mode: CheckAutoCondition error: " & ex.Message, ex)
                Return False
            End Try
        End Function

        Public Shared Function FindLatestFile(folderOrFilePath As String, Optional computerName As String = Nothing) As String
            Try
                If File.Exists(folderOrFilePath) Then
                    Return folderOrFilePath
                End If

                If Not Directory.Exists(folderOrFilePath) Then
                    Return Nothing
                End If

                Dim allFiles As String() = Directory.GetFiles(folderOrFilePath)
                If allFiles.Length = 0 Then Return Nothing

                Dim candidateFiles As New System.Collections.Generic.List(Of String)()
                If Not String.IsNullOrEmpty(computerName) Then
                    For Each f As String In allFiles
                        If Path.GetFileName(f).IndexOf(computerName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                            candidateFiles.Add(f)
                        End If
                    Next
                End If

                If candidateFiles.Count = 0 Then
                    ' No files found matching this computer's name — do NOT fall back to
                    ' other machines' files, as that would trigger restarts based on wrong data.
                    Return Nothing
                End If

                Dim latestFile As String = Nothing
                Dim latestTimestamp As DateTime = DateTime.MinValue

                For Each f As String In candidateFiles
                    Try
                        Dim fileTs As DateTime = GetFileTimestamp(f)
                        If fileTs > latestTimestamp Then
                            latestTimestamp = fileTs
                            latestFile = f
                        End If
                    Catch
                    End Try
                Next

                Return latestFile
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: FindLatestFile error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Shared Function GetFileTimestamp(filePath As String) As DateTime
            Try
                Dim fileName As String = Path.GetFileNameWithoutExtension(filePath)

                ' 1. Check for 14-digit datetime: YYYYMMDD_HHMMSS or YYYYMMDDHHMMSS
                ' e.g. OperaterLog_20260928153708, OperaterLog_20260928_153708
                Dim m14 As Match = Regex.Match(fileName, "(?<!\d)(\d{4})(\d{2})(\d{2})_?(\d{2})(\d{2})(\d{2})(?!\d)")
                If m14.Success Then
                    Dim yr As Integer = Integer.Parse(m14.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(m14.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(m14.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(m14.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(m14.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(m14.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                ' 2. Check for 8-digit date: YYYYMMDD or YYYY-MM-DD or YYYY_MM_DD
                ' e.g. ATPM_20260928_RSX5000-373, Log_2026-09-28
                Dim m8 As Match = Regex.Match(fileName, "(?<!\d)(\d{4})[-_]?(\d{2})[-_]?(\d{2})(?!\d)")
                If m8.Success Then
                    Dim yr As Integer = Integer.Parse(m8.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(m8.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(m8.Groups(3).Value)
                    If IsValidDateTime(yr, mo, dy, 0, 0, 0) Then
                        Dim lwt As DateTime = File.GetLastWriteTime(filePath)
                        Return New DateTime(yr, mo, dy, lwt.Hour, lwt.Minute, lwt.Second)
                    End If
                End If

                ' 3. Check for dd-MM-yyyy or dd_MM_yyyy
                Dim mDMY As Match = Regex.Match(fileName, "(?<!\d)(\d{2})[-_](\d{2})[-_](\d{4})(?!\d)")
                If mDMY.Success Then
                    Dim dy As Integer = Integer.Parse(mDMY.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mDMY.Groups(2).Value)
                    Dim yr As Integer = Integer.Parse(mDMY.Groups(3).Value)
                    If IsValidDateTime(yr, mo, dy, 0, 0, 0) Then
                        Dim lwt As DateTime = File.GetLastWriteTime(filePath)
                        Return New DateTime(yr, mo, dy, lwt.Hour, lwt.Minute, lwt.Second)
                    End If
                End If

                ' 4. Fallback to File.GetLastWriteTime
                Return File.GetLastWriteTime(filePath)
            Catch
                Try
                    Return File.GetLastWriteTime(filePath)
                Catch
                    Return DateTime.MinValue
                End Try
            End Try
        End Function

        Public Shared Function ReadEndOfTestTime(filePath As String) As DateTime
            Try
                Dim lines() As String = SafeReadAllLines(filePath)
                Dim latestTime As DateTime = DateTime.MinValue

                For Each line As String In lines
                    If line.IndexOf("ENDOFTEST", StringComparison.OrdinalIgnoreCase) < 0 Then Continue For

                    Dim parsed As DateTime = ParseEndOfTestLine(line)
                    If parsed > latestTime Then
                        latestTime = parsed
                    End If
                Next

                Return latestTime
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: ReadEndOfTestTime error: " & ex.Message)
                Return DateTime.MinValue
            End Try
        End Function

        Private Shared Function ParseEndOfTestLine(line As String) As DateTime
            Try
                ' 1. yyyy-MM-dd HH:mm:ss or yyyy/MM/dd HH:mm:ss
                Dim mYMD As Match = Regex.Match(line, "ENDOFTEST\s*:\s*(\d{4})[-/.](\d{1,2})[-/.](\d{1,2})\s+(\d{1,2}):(\d{2}):(\d{2})", RegexOptions.IgnoreCase)
                If mYMD.Success Then
                    Dim yr As Integer = Integer.Parse(mYMD.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mYMD.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(mYMD.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mYMD.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mYMD.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mYMD.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                ' 2. MM/dd/yyyy HH:mm:ss or dd/MM/yyyy HH:mm:ss
                Dim mMDY As Match = Regex.Match(line, "ENDOFTEST\s*:\s*(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})\s+(\d{1,2}):(\d{2}):(\d{2})", RegexOptions.IgnoreCase)
                If mMDY.Success Then
                    Dim g1 As Integer = Integer.Parse(mMDY.Groups(1).Value)
                    Dim g2 As Integer = Integer.Parse(mMDY.Groups(2).Value)
                    Dim yr As Integer = Integer.Parse(mMDY.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mMDY.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mMDY.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mMDY.Groups(6).Value)

                    If IsValidDateTime(yr, g1, g2, hr, mi, sc) Then
                        Return New DateTime(yr, g1, g2, hr, mi, sc)
                    ElseIf IsValidDateTime(yr, g2, g1, hr, mi, sc) Then
                        Return New DateTime(yr, g2, g1, hr, mi, sc)
                    End If
                End If

                ' 3. Compact 14-digit: yyyyMMdd_HHmmss or yyyyMMddHHmmss
                Dim mCompact As Match = Regex.Match(line, "ENDOFTEST\s*:\s*(\d{4})(\d{2})(\d{2})_?(\d{2})(\d{2})(\d{2})", RegexOptions.IgnoreCase)
                If mCompact.Success Then
                    Dim yr As Integer = Integer.Parse(mCompact.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mCompact.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(mCompact.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mCompact.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mCompact.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mCompact.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                Return DateTime.MinValue
            Catch
                Return DateTime.MinValue
            End Try
        End Function

        Public Shared Function ReadLatestStopTime(filePath As String) As DateTime
            Try
                Dim lines() As String = SafeReadAllLines(filePath)
                Dim latestTime As DateTime = DateTime.MinValue

                For Each line As String In lines
                    If line.IndexOf("STOP", StringComparison.OrdinalIgnoreCase) < 0 Then Continue For

                    Dim parsed As DateTime = ParseStopLine(line)
                    If parsed > latestTime Then
                        latestTime = parsed
                    End If
                Next

                Return latestTime
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: ReadLatestStopTime error: " & ex.Message)
                Return DateTime.MinValue
            End Try
        End Function

        Private Shared Function ParseStopLine(line As String) As DateTime
            Try
                ' 1. Compact format: yyyyMMdd_HHmmss STOP or yyyyMMddHHmmss STOP
                Dim mCompact As Match = Regex.Match(line, "(?<!\d)(\d{4})(\d{2})(\d{2})_?(\d{2})(\d{2})(\d{2})\s+STOP", RegexOptions.IgnoreCase)
                If mCompact.Success Then
                    Dim yr As Integer = Integer.Parse(mCompact.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mCompact.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(mCompact.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mCompact.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mCompact.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mCompact.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                ' 2. Delimited format: yyyy-MM-dd HH:mm:ss STOP or yyyy/MM/dd HH:mm:ss STOP
                Dim mYMD As Match = Regex.Match(line, "(?<!\d)(\d{4})[-/.](\d{1,2})[-/.](\d{1,2})[\s_T]+(\d{1,2}):(\d{2}):(\d{2})\s+STOP", RegexOptions.IgnoreCase)
                If mYMD.Success Then
                    Dim yr As Integer = Integer.Parse(mYMD.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mYMD.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(mYMD.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mYMD.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mYMD.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mYMD.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                ' 3. STOP followed by timestamp: STOP 20260928_153708 or STOP : 20260928_153708
                Dim mStopFirst As Match = Regex.Match(line, "STOP\s*[:\s_-]\s*(\d{4})(\d{2})(\d{2})_?(\d{2})(\d{2})(\d{2})", RegexOptions.IgnoreCase)
                If mStopFirst.Success Then
                    Dim yr As Integer = Integer.Parse(mStopFirst.Groups(1).Value)
                    Dim mo As Integer = Integer.Parse(mStopFirst.Groups(2).Value)
                    Dim dy As Integer = Integer.Parse(mStopFirst.Groups(3).Value)
                    Dim hr As Integer = Integer.Parse(mStopFirst.Groups(4).Value)
                    Dim mi As Integer = Integer.Parse(mStopFirst.Groups(5).Value)
                    Dim sc As Integer = Integer.Parse(mStopFirst.Groups(6).Value)
                    If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                        Return New DateTime(yr, mo, dy, hr, mi, sc)
                    End If
                End If

                Return DateTime.MinValue
            Catch
                Return DateTime.MinValue
            End Try
        End Function

        Private Shared Function IsValidTime(hr As Integer, mi As Integer, sc As Integer) As Boolean
            Return (hr >= 0 AndAlso hr <= 23 AndAlso mi >= 0 AndAlso mi <= 59 AndAlso sc >= 0 AndAlso sc <= 59)
        End Function

        Private Shared Function IsValidDateTime(yr As Integer, mo As Integer, dy As Integer, hr As Integer, mi As Integer, sc As Integer) As Boolean
            If yr < 2000 OrElse yr > 2100 Then Return False
            If mo < 1 OrElse mo > 12 Then Return False
            If dy < 1 OrElse dy > 31 Then Return False
            Try
                If dy > DateTime.DaysInMonth(yr, mo) Then Return False
            Catch
                Return False
            End Try
            Return IsValidTime(hr, mi, sc)
        End Function

        Private Shared Function SafeReadAllLines(filePath As String) As String()
            Try
                Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using reader As New StreamReader(fs)
                        Dim lines As New System.Collections.Generic.List(Of String)
                        Dim line As String = reader.ReadLine()
                        While line IsNot Nothing
                            lines.Add(line)
                            line = reader.ReadLine()
                        End While
                        Return lines.ToArray()
                    End Using
                End Using
            Catch ex As Exception
                Return New String() {}
            End Try
        End Function

    End Class

End Namespace
