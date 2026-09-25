Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text.RegularExpressions

Namespace Strategies

    Public Class AutoStrategy
        Implements IUpdateStrategy

        Private Const CheckIntervalMs As Integer = 30000

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            If context Is Nothing OrElse context.Tester Is Nothing Then
                Managers.LogManager.[Error]("AutoStrategy: Invalid or null UpdateContext/Tester.")
                Return UpdateResult.[Error]
            End If

            If Not context.NeedsUpdate Then
                Managers.LogManager.Info("Auto mode: No version mismatch. No action.")
                Return UpdateResult.NoAction
            End If

            Managers.LogManager.Info("Auto mode: Version mismatch detected. Current=" & _
                                    If(context.CurrentVersion, "N/A") & " Latest=" & If(context.LatestVersion, "N/A"))

            Dim watchFolder As String = Config.AppSettings.AutoWatchFolderPath
            Dim stopLogFolder As String = Config.AppSettings.AutoStopLogFolderPath

            If String.IsNullOrEmpty(watchFolder) OrElse String.IsNullOrEmpty(stopLogFolder) Then
                Managers.LogManager.Info("Auto mode: Folders not configured. Falling back to flag mode.")
                Return ExecuteFlagMode(context)
            End If

            Dim watchPath As String = FindLatestFile(watchFolder)
            If String.IsNullOrEmpty(watchPath) Then
                Managers.LogManager.Warn("Auto mode: No files in watch folder: " & watchFolder)
                Return UpdateResult.NoAction
            End If

            Dim stopLogPath As String = FindLatestFile(stopLogFolder)
            If String.IsNullOrEmpty(stopLogPath) Then
                Managers.LogManager.Warn("Auto mode: No files in stop log folder: " & stopLogFolder)
                Return UpdateResult.NoAction
            End If

            Dim endOfTestTime As DateTime = ReadEndOfTestTime(watchPath)
            Dim stopTime As DateTime = ReadLatestStopTime(stopLogPath)

            Managers.LogManager.Info("Auto mode: ENDOFTEST=" & endOfTestTime.ToString("yyyy-MM-dd HH:mm:ss") & _
                                    ", STOP=" & stopTime.ToString("yyyy-MM-dd HH:mm:ss"))

            If endOfTestTime >= stopTime Then
                Managers.LogManager.Info("Auto mode: ENDOFTEST >= STOP. Machine still active. No action.")
                Return UpdateResult.NoAction
            End If

            Managers.LogManager.Info("Auto mode: ENDOFTEST < STOP. Machine stopped. Starting " & _
                                    Config.AppSettings.AutoWaitMinutes.ToString() & "-minute wait...")

            If WaitAndMonitor(watchFolder, stopLogFolder, Config.AppSettings.AutoWaitMinutes) Then
                Managers.LogManager.Info("Auto mode: Wait complete. Machine still stopped. Setting flag and requesting restart.")
                Try
                    Managers.UpdateFlagManager.SetFlag(context.Tester.ComputerName, True)
                Catch ex As Exception
                    Managers.LogManager.Warn("Auto mode: Failed to set update flag: " & ex.Message)
                End Try
                Return UpdateResult.RestartRequired
            Else
                Managers.LogManager.Info("Auto mode: Machine became active during wait. Cancelled.")
                Return UpdateResult.NoAction
            End If
        End Function

        Private Function ExecuteFlagMode(context As Models.UpdateContext) As UpdateResult
            Dim computerName As String = context.Tester.ComputerName
            Managers.LogManager.Info("Auto mode (flag): Setting update flag for " & computerName)
            Try
                Managers.UpdateFlagManager.SetFlag(computerName, True)
                Return UpdateResult.RestartRequired
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to set update flag for " & computerName, ex)
                Return UpdateResult.[Error]
            End Try
        End Function

        Private Shared Function FindLatestFile(folderOrFilePath As String) As String
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

        Private Shared Function ReadEndOfTestTime(filePath As String) As DateTime
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

        Private Shared Function ReadLatestStopTime(filePath As String) As DateTime
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

        Private Shared Function WaitAndMonitor(watchFolder As String, stopLogFolder As String, waitMinutes As Integer) As Boolean
            Dim deadline As DateTime = DateTime.Now.AddMinutes(waitMinutes)

            Do While DateTime.Now < deadline
                System.Threading.Thread.Sleep(CheckIntervalMs)

                Dim wp As String = FindLatestFile(watchFolder)
                Dim sp As String = FindLatestFile(stopLogFolder)
                If String.IsNullOrEmpty(wp) OrElse String.IsNullOrEmpty(sp) Then Continue Do

                Dim currentEndOfTest As DateTime = ReadEndOfTestTime(wp)
                Dim currentStopTime As DateTime = ReadLatestStopTime(sp)

                If currentEndOfTest >= currentStopTime Then
                    Managers.LogManager.Info("Auto mode: Monitor -- machine became active. ENDOFTEST=" & _
                                            currentEndOfTest.ToString("HH:mm:ss") & " >= STOP=" & _
                                            currentStopTime.ToString("HH:mm:ss"))
                    Return False
                End If
            Loop

            Dim fwp As String = FindLatestFile(watchFolder)
            Dim fsp As String = FindLatestFile(stopLogFolder)
            If String.IsNullOrEmpty(fwp) OrElse String.IsNullOrEmpty(fsp) Then Return False
            Return (ReadEndOfTestTime(fwp) < ReadLatestStopTime(fsp))
        End Function

    End Class

End Namespace
