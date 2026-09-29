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

                Dim currentVer As String = Managers.VersionManager.ReadRegistryVersion()
                Dim serverVer As String = Managers.VersionManager.ReadLatestVersion()

                If String.IsNullOrEmpty(serverVer) Then Return False

                Dim needsUpdate As Boolean = False
                If String.IsNullOrEmpty(currentVer) Then
                    needsUpdate = True
                ElseIf Not String.Equals(currentVer, serverVer, StringComparison.OrdinalIgnoreCase) Then
                    needsUpdate = True
                End If

                If Not needsUpdate Then Return False

                Dim watchFolder As String = Config.AppSettings.AutoWatchFolderPath
                Dim stopLogFolder As String = Config.AppSettings.AutoStopLogFolderPath

                If String.IsNullOrEmpty(watchFolder) OrElse String.IsNullOrEmpty(stopLogFolder) Then
                    Return False
                End If

                If Not File.Exists(watchFolder) AndAlso Not Directory.Exists(watchFolder) Then Return False
                If Not File.Exists(stopLogFolder) AndAlso Not Directory.Exists(stopLogFolder) Then Return False

                Dim watchPath As String = FindLatestFile(watchFolder)
                Dim stopLogPath As String = FindLatestFile(stopLogFolder)

                If String.IsNullOrEmpty(watchPath) OrElse String.IsNullOrEmpty(stopLogPath) Then
                    Return False
                End If

                Dim endOfTestTime As DateTime = ReadEndOfTestTime(watchPath)
                Dim stopTime As DateTime = ReadLatestStopTime(stopLogPath)

                If endOfTestTime = DateTime.MinValue OrElse stopTime = DateTime.MinValue Then
                    Return False
                End If

                Return (endOfTestTime < stopTime)
            Catch ex As Exception
                Managers.LogManager.Error("Auto mode: CheckAutoCondition error: " & ex.Message, ex)
                Return False
            End Try
        End Function

        Public Shared Function FindLatestFile(folderOrFilePath As String) As String
            Try
                If File.Exists(folderOrFilePath) Then
                    Return folderOrFilePath
                End If

                If Not Directory.Exists(folderOrFilePath) Then
                    Return Nothing
                End If

                Dim latestFile As String = Nothing
                Dim latestTime As DateTime = DateTime.MinValue

                For Each f As String In Directory.GetFiles(folderOrFilePath)
                    Try
                        Dim writeTime As DateTime = File.GetLastWriteTime(f)
                        If writeTime > latestTime Then
                            latestTime = writeTime
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

        Public Shared Function ReadEndOfTestTime(filePath As String) As DateTime
            Try
                Dim lines() As String = SafeReadAllLines(filePath)

                For i As Integer = lines.Length - 1 To 0 Step -1
                    Dim line As String = lines(i)
                    If line.IndexOf("ENDOFTEST", StringComparison.OrdinalIgnoreCase) < 0 Then Continue For

                    Dim m As Match = Regex.Match(line, "ENDOFTEST\s*:\s*(\d{1,2})/(\d{1,2})/(\d{4})\s+(\d{1,2}):(\d{2}):(\d{2})", RegexOptions.IgnoreCase)
                    If m.Success Then
                        Dim mo As Integer = Integer.Parse(m.Groups(1).Value)
                        Dim dy As Integer = Integer.Parse(m.Groups(2).Value)
                        Dim yr As Integer = Integer.Parse(m.Groups(3).Value)
                        Dim hr As Integer = Integer.Parse(m.Groups(4).Value)
                        Dim mi As Integer = Integer.Parse(m.Groups(5).Value)
                        Dim sc As Integer = Integer.Parse(m.Groups(6).Value)
                        If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                            Return New DateTime(yr, mo, dy, hr, mi, sc)
                        End If
                    End If
                Next

                Return DateTime.MinValue
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: ReadEndOfTestTime error: " & ex.Message)
                Return DateTime.MinValue
            End Try
        End Function

        Public Shared Function ReadLatestStopTime(filePath As String) As DateTime
            Try
                Dim lines() As String = SafeReadAllLines(filePath)

                For i As Integer = lines.Length - 1 To 0 Step -1
                    Dim line As String = lines(i)
                    If line.IndexOf("STOP", StringComparison.OrdinalIgnoreCase) < 0 Then Continue For

                    Dim m As Match = Regex.Match(line, "(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})\s+STOP", RegexOptions.IgnoreCase)
                    If m.Success Then
                        Dim yr As Integer = Integer.Parse(m.Groups(1).Value)
                        Dim mo As Integer = Integer.Parse(m.Groups(2).Value)
                        Dim dy As Integer = Integer.Parse(m.Groups(3).Value)
                        Dim hr As Integer = Integer.Parse(m.Groups(4).Value)
                        Dim mi As Integer = Integer.Parse(m.Groups(5).Value)
                        Dim sc As Integer = Integer.Parse(m.Groups(6).Value)
                        If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                            Return New DateTime(yr, mo, dy, hr, mi, sc)
                        End If
                    End If
                Next

                Return DateTime.MinValue
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: ReadLatestStopTime error: " & ex.Message)
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
