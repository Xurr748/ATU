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
                Dim fileDate As DateTime = DateTime.Today
                Try
                    If File.Exists(filePath) Then
                        fileDate = File.GetLastWriteTime(filePath).Date
                    End If
                Catch
                End Try

                For i As Integer = lines.Length - 1 To 0 Step -1
                    Dim line As String = lines(i)
                    Dim parsedTime As DateTime = DateTime.MinValue
                    If TryParseStopTimeFromLine(line, fileDate, parsedTime) Then
                        Managers.LogManager.Info("Auto mode: Latest stop time parsed: " & parsedTime.ToString("yyyy-MM-dd HH:mm:ss") & " from line: " & line.Trim())
                        Return parsedTime
                    End If
                Next

                Managers.LogManager.Warn("Auto mode: No valid 'stop' line with timestamp found in " & filePath)
                Return DateTime.MinValue
            Catch ex As Exception
                Managers.LogManager.Warn("Auto mode: Error reading stop log " & filePath & ": " & ex.Message)
                Return DateTime.MinValue
            End Try
        End Function

        Private Shared Function TryParseStopTimeFromLine(line As String, fileDate As DateTime, ByRef parsedTime As DateTime) As Boolean
            If String.IsNullOrWhiteSpace(line) Then Return False
            If line.IndexOf("stop", StringComparison.OrdinalIgnoreCase) < 0 Then Return False

            ' 1. Full 14-digit datetime (yyyyMMddHHmmssStop or yyyyMMdd_HHmmss Stop)
            Dim m14 As Match = Regex.Match(line, "(\d{4})(\d{2})(\d{2})[_\sT]?(\d{2})(\d{2})(\d{2})", RegexOptions.IgnoreCase)
            If m14.Success Then
                Dim yr As Integer = Integer.Parse(m14.Groups(1).Value)
                Dim mo As Integer = Integer.Parse(m14.Groups(2).Value)
                Dim dy As Integer = Integer.Parse(m14.Groups(3).Value)
                Dim hr As Integer = Integer.Parse(m14.Groups(4).Value)
                Dim mi As Integer = Integer.Parse(m14.Groups(5).Value)
                Dim sc As Integer = Integer.Parse(m14.Groups(6).Value)
                If IsValidDateTime(yr, mo, dy, hr, mi, sc) Then
                    parsedTime = New DateTime(yr, mo, dy, hr, mi, sc)
                    Return True
                End If
            End If

            ' 2. Standard formatted date + time (yyyy-MM-dd HH:mm:ss Stop or yyyy/MM/dd HH:mm:ss Stop)
            Dim mStandard As Match = Regex.Match(line, "(\d{4}[-/.]\d{1,2}[-/.]\d{1,2})[\s_T,]+(\d{1,2}:\d{2}:\d{2})", RegexOptions.IgnoreCase)
            If mStandard.Success Then
                Dim dtStr As String = mStandard.Groups(1).Value & " " & mStandard.Groups(2).Value
                Dim dt As DateTime
                If DateTime.TryParse(dtStr, dt) Then
                    parsedTime = dt
                    Return True
                End If
            End If

            ' 3. Time attached directly before "Stop" (e.g. 150530Stop, 150530_Stop, 15:05:30Stop, 150530 Stop)
            Dim mBefore As Match = Regex.Match(line, "(\d{2})[:.]?(\d{2})[:.]?(\d{2})\s*[_,-]?\s*stop", RegexOptions.IgnoreCase)
            If mBefore.Success Then
                Dim hr As Integer = Integer.Parse(mBefore.Groups(1).Value)
                Dim mi As Integer = Integer.Parse(mBefore.Groups(2).Value)
                Dim sc As Integer = Integer.Parse(mBefore.Groups(3).Value)
                If IsValidTime(hr, mi, sc) Then
                    parsedTime = New DateTime(fileDate.Year, fileDate.Month, fileDate.Day, hr, mi, sc)
                    Return True
                End If
            End If

            ' 4. Time attached directly after "Stop" (e.g. Stop150530, Stop_150530, Stop 15:05:30)
            Dim mAfter As Match = Regex.Match(line, "stop\s*[_,-]?\s*(\d{2})[:.]?(\d{2})[:.]?(\d{2})", RegexOptions.IgnoreCase)
            If mAfter.Success Then
                Dim hr As Integer = Integer.Parse(mAfter.Groups(1).Value)
                Dim mi As Integer = Integer.Parse(mAfter.Groups(2).Value)
                Dim sc As Integer = Integer.Parse(mAfter.Groups(3).Value)
                If IsValidTime(hr, mi, sc) Then
                    parsedTime = New DateTime(fileDate.Year, fileDate.Month, fileDate.Day, hr, mi, sc)
                    Return True
                End If
            End If

            ' 5. Any 6 consecutive digits in the line: (\d{6})
            Dim m6 As MatchCollection = Regex.Matches(line, "(\d{6})")
            For Each m As Match In m6
                Dim timeStr As String = m.Groups(1).Value
                Dim hr As Integer = Integer.Parse(timeStr.Substring(0, 2))
                Dim mi As Integer = Integer.Parse(timeStr.Substring(2, 2))
                Dim sc As Integer = Integer.Parse(timeStr.Substring(4, 2))
                If IsValidTime(hr, mi, sc) Then
                    parsedTime = New DateTime(fileDate.Year, fileDate.Month, fileDate.Day, hr, mi, sc)
                    Return True
                End If
            Next

            ' 6. Any HH:mm:ss in the line
            Dim mTimeOnly As Match = Regex.Match(line, "(\d{1,2}):(\d{2}):(\d{2})")
            If mTimeOnly.Success Then
                Dim hr As Integer = Integer.Parse(mTimeOnly.Groups(1).Value)
                Dim mi As Integer = Integer.Parse(mTimeOnly.Groups(2).Value)
                Dim sc As Integer = Integer.Parse(mTimeOnly.Groups(3).Value)
                If IsValidTime(hr, mi, sc) Then
                    parsedTime = New DateTime(fileDate.Year, fileDate.Month, fileDate.Day, hr, mi, sc)
                    Return True
                End If
            End If

            Return False
        End Function

        Private Shared Function IsValidTime(hr As Integer, mi As Integer, sc As Integer) As Boolean
            Return (hr >= 0 AndAlso hr <= 23 AndAlso mi >= 0 AndAlso mi <= 59 AndAlso sc >= 0 AndAlso sc <= 59)
        End Function

        Private Shared Function IsValidDateTime(yr As Integer, mo As Integer, dy As Integer, hr As Integer, mi As Integer, sc As Integer) As Boolean
            If yr < 2000 OrElse yr > 2100 Then Return False
            If mo < 1 OrElse mo > 12 Then Return False
            If dy < 1 OrElse dy > DateTime.DaysInMonth(yr, mo) Then Return False
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
                    Managers.LogManager.Info("Auto mode: Monitor check -- stopTime caught up. lastEdit=" & _
                                            currentLastEdit.ToString("HH:mm:ss") & " <= stopTime=" & _
                                            currentStopTime.ToString("HH:mm:ss"))
                    Return False
                End If

                Dim remaining As Integer = Math.Max(0, CInt((deadline - DateTime.Now).TotalMinutes))
                Managers.LogManager.Info("Auto mode: Monitor check -- still active. " & remaining.ToString() & " min remaining.")
            Loop

            Dim finalLastEdit As DateTime = ReadLastEditTime(watchPath)
            Dim finalStopTime As DateTime = ReadLatestStopTime(stopLogPath)
            Return (finalLastEdit > finalStopTime)
        End Function

    End Class

End Namespace
