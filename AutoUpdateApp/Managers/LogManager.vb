Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text

Namespace Managers

    Public NotInheritable Class LogManager

        Private Shared ReadOnly _lock As New Object
        Private Shared _logDirectory As String

        Private Sub New()
        End Sub

        Private Shared ReadOnly Property LogDirectory As String
            Get
                If _logDirectory Is Nothing Then
                    Dim parentDir As String = Config.AppSettings.LogPath
                    Dim company As String = Config.AppSettings.FolderName
                    If String.IsNullOrEmpty(parentDir) Then
                        parentDir = AppDomain.CurrentDomain.BaseDirectory
                    End If
                    _logDirectory = Path.Combine(parentDir, company, Utilities.EnvironmentHelper.ComputerShortId)
                End If
                Return _logDirectory
            End Get
        End Property

        Private Shared ReadOnly Property LogsFilePath As String
            Get
                Dim pattern As String = Config.AppSettings.LogFileName
                Dim fileName As String = pattern.Replace("{ComputerName}", Utilities.EnvironmentHelper.ComputerShortId)
                Return Path.Combine(LogDirectory, fileName)
            End Get
        End Property

        Private Shared ReadOnly Property IPFilePath As String
            Get
                Return Path.Combine(LogDirectory, Utilities.EnvironmentHelper.ComputerShortId & "_IP.txt")
            End Get
        End Property

        Public Shared Sub Info(message As String)
            WriteLog("INFO", message)
        End Sub

        Public Shared Sub Warn(message As String)
            WriteLog("WARN", message)
        End Sub

        Public Shared Sub [Error](message As String, Optional ex As Exception = Nothing)
            Dim fullMessage As String = message
            If ex IsNot Nothing Then
                fullMessage = message & Environment.NewLine & ex.ToString()
            End If
            WriteLog("ERROR", fullMessage)
        End Sub

        Private Shared Sub WriteLog(level As String, message As String)
            Try
                Dim sb As New StringBuilder(128)
                sb.Append("["c)
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                sb.Append("] [")
                sb.Append(level)
                sb.Append("] ")
                sb.AppendLine(message)

                SyncLock _lock
                    Dim dir As String = LogDirectory
                    If Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If
                    File.AppendAllText(LogsFilePath, sb.ToString())
                End SyncLock
            Catch
            End Try
        End Sub

        Public Shared Sub LogIPAddress()
            Try
                Dim currentIP As String = GetLocalIPAddress()

                SyncLock _lock
                    Dim dir As String = LogDirectory
                    If Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If

                    Dim filePath As String = IPFilePath

                    Dim lastIP As String = ""
                    If File.Exists(filePath) Then
                        Dim content As String = File.ReadAllText(filePath).Trim()
                        Dim lines As String() = content.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        If lines.Length > 0 Then
                            Dim lastLine As String = lines(lines.Length - 1).Trim()
                            Dim bracketEnd As Integer = lastLine.IndexOf("] ")
                            If bracketEnd >= 0 AndAlso bracketEnd + 2 < lastLine.Length Then
                                lastIP = lastLine.Substring(bracketEnd + 2).Trim()
                            End If
                        End If
                    End If

                    If Not String.Equals(lastIP, currentIP, StringComparison.OrdinalIgnoreCase) Then
                        Dim sb As New StringBuilder(64)
                        sb.Append("["c)
                        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        sb.Append("] ")
                        sb.AppendLine(currentIP)
                        File.AppendAllText(filePath, sb.ToString())
                    End If
                End SyncLock
            Catch
            End Try
        End Sub

        Private Shared Function GetLocalIPAddress() As String
            Try
                Dim host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                For Each ip In host.AddressList
                    If ip.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                        Return ip.ToString()
                    End If
                Next
            Catch
            End Try
            Return "127.0.0.1"
        End Function

        Public Shared Sub Reset()
            SyncLock _lock
                _logDirectory = Nothing
            End SyncLock
        End Sub

    End Class

End Namespace
