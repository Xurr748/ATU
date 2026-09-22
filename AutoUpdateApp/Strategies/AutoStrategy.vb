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

            Dim watchPath As String = Config.AppSettings.AutoWatchFilePath
            Dim stopLogPath As String = Config.AppSettings.AutoStopLogPath

            If String.IsNullOrEmpty(watchPath) OrElse String.IsNullOrEmpty(stopLogPath) Then
                Managers.LogManager.Info("Auto mode: AutoWatchFilePath or AutoStopLogPath not configured. Falling back to flag mode.")
                Return ExecuteFlagMode(context)
            End If

            If Not File.Exists(watchPath) Then
                Managers.LogManager.Warn("Auto mode: Watch file not found: " & watchPath)
                Return UpdateResult.NoAction
            End If

            If Not File.Exists(stopLogPath) Then
                Managers.LogManager.Warn("Auto mode: Stop log file not found: " & stopLogPath)
                Return UpdateResult.NoAction
            End If

            Dim lastEditTime As DateTime = ReadLastEditTime(watchPath)
            Dim stopTime As DateTime = ReadLatestStopTime(stopLogPath)

            Managers.LogManager.Info("Auto mode: WatchFile lastEdit=" & lastEditTime.ToString("yyyy-MM-dd HH:mm:ss") & _
                                    ", StopLog stopTime=" & stopTime.ToString("yyyy-MM-dd HH:mm:ss"))

            If lastEditTime <= stopTime Then
                Managers.LogManager.Info("Auto mode: lastEdit <= stopTime. Machine is stopped. No action.")
                Return UpdateResult.NoAction
            End If

            Managers.LogManager.Info("Auto mode: lastEdit > stopTime. Machine may be active. Starting " & _
                                    Config.AppSettings.AutoWaitMinutes.ToString() & "-minute monitoring...")

            If WaitAndMonitor(watchPath, stopLogPath, Config.AppSettings.AutoWaitMinutes) Then
                Managers.LogManager.Info("Auto mode: Monitoring period elapsed. lastEdit still > stopTime. Requesting restart.")
                Return UpdateResult.RestartRequired
            Else
                Managers.LogManager.Info("Auto mode: During monitoring, stopTime became >= lastEdit. Cancelled.")
                Return UpdateResult.NoAction
            End If
        End Function

        Private Function ExecuteFlagMode(context As Models.UpdateContext) As UpdateResult
            Dim computerName As String = context.Tester.ComputerName
            Managers.LogManager.Info( _
                "Auto mode (flag) -- Setting update flag for restart for " & computerName & _
                ". Current: " & If(context.CurrentVersion, "N/A") & " -> Latest: " & If(context.LatestVersion, "N/A"))
            Try
                Managers.UpdateFlagManager.SetFlag(computerName, True)
                Managers.LogManager.Info("Auto update flag set successfully.")
                Return UpdateResult.UpdateScheduledForRestart
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to set update flag in Auto mode for " & computerName, ex)
                Return UpdateResult.[Error]
            End Try
        End Function

        Private Shared Function ReadLastEditTime(filePath As String) As DateTime
            Try
                Return File.GetLastWriteTime(filePath)
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: Cannot read last write time of " & filePath & ": " & ex.Message)
                Return DateTime.MinValue
            End Try
        End Function

        Private Shared Function ReadLatestStopTime(filePath As String) As DateTime
            Try
                Dim lines() As String = SafeReadAllLines(filePath)
                Dim latestStopTime As DateTime = DateTime.MinValue
                Dim sixDigitPattern As New Regex("\b(\d{6})\b")

                For i As Integer = lines.Length - 1 To 0 Step -1
                    Dim line As String = lines(i)
                    If line.IndexOf("stop", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        Dim m As Match = sixDigitPattern.Match(line)
                        If m.Success Then
                            Dim timeStr As String = m.Groups(1).Value
                            Dim hours As Integer
                            Dim mins As Integer
                            Dim secs As Integer
                            If Integer.TryParse(timeStr.Substring(0, 2), hours) AndAlso _
                               Integer.TryParse(timeStr.Substring(2, 2), mins) AndAlso _
                               Integer.TryParse(timeStr.Substring(4, 2), secs) AndAlso _
                               hours >= 0 AndAlso hours <= 23 AndAlso _
                               mins >= 0 AndAlso mins <= 59 AndAlso _
                               secs >= 0 AndAlso secs <= 59 Then

                                Dim stopTime As New DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, mins, secs)
                                If stopTime > latestStopTime Then
                                    latestStopTime = stopTime
                                End If
                                Exit For
                            End If
                        End If
                    End If
                Next

                If latestStopTime = DateTime.MinValue Then
                    Managers.LogManager.Warn("Auto mode: No valid 'stop' line with HHMMSS found in " & filePath)
                Else
                    Managers.LogManager.Info("Auto mode: Latest stop time parsed: " & latestStopTime.ToString("HH:mm:ss"))
                End If

                Return latestStopTime
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: Error reading stop log " & filePath & ": " & ex.Message)
                Return DateTime.MinValue
            End Try
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
                Managers.LogManager.Warn("Auto mode: SafeReadAllLines failed for " & filePath & ": " & ex.Message)
                Return New String() {}
            End Try
        End Function

        Private Shared Function WaitAndMonitor(watchPath As String, stopLogPath As String, waitMinutes As Integer) As Boolean
            Dim deadline As DateTime = DateTime.Now.AddMinutes(waitMinutes)

            Do While DateTime.Now < deadline
                System.Threading.Thread.Sleep(CheckIntervalMs)

                Dim currentLastEdit As DateTime = ReadLastEditTime(watchPath)
                Dim currentStopTime As DateTime = ReadLatestStopTime(stopLogPath)

                If currentLastEdit <= currentStopTime Then
                    Managers.LogManager.Info("Auto mode: Monitor check — stopTime caught up. lastEdit=" & _
                                            currentLastEdit.ToString("HH:mm:ss") & " <= stopTime=" & _
                                            currentStopTime.ToString("HH:mm:ss"))
                    Return False
                End If

                Dim remaining As Integer = Math.Max(0, CInt((deadline - DateTime.Now).TotalMinutes))
                Managers.LogManager.Info("Auto mode: Monitor check — still active. " & remaining.ToString() & " min remaining.")
            Loop

            Dim finalLastEdit As DateTime = ReadLastEditTime(watchPath)
            Dim finalStopTime As DateTime = ReadLatestStopTime(stopLogPath)
            Return (finalLastEdit > finalStopTime)
        End Function

    End Class

End Namespace
