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
            Dim sb As New StringBuilder(128)
            sb.Append("["c)
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.Append("] [")
            sb.Append(level)
            sb.Append("] ")
            sb.AppendLine(message)

            Dim text As String = sb.ToString()

            SyncLock _lock
                Try
                    Dim dir As String = LogDirectory
                    If Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If
                    AppendTextSafe(LogsFilePath, text)
                Catch
                    Try
                        Dim fallbackDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                        If Not Directory.Exists(fallbackDir) Then
                            Directory.CreateDirectory(fallbackDir)
                        End If
                        Dim fallbackFile As String = Path.Combine(fallbackDir, "AutoUpdate_Log.txt")
                        AppendTextSafe(fallbackFile, text)
                    Catch
                        System.Diagnostics.Debug.Write(text)
                    End Try
                End Try
            End SyncLock
        End Sub

        Private Shared Sub AppendTextSafe(filePath As String, text As String)
            Using fs As New FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
                Dim bytes As Byte() = Encoding.UTF8.GetBytes(text)
                fs.Write(bytes, 0, bytes.Length)
            End Using
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
                        Try
                            Dim content As String = ""
                            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                                Using reader As New StreamReader(fs, Encoding.UTF8)
                                    content = reader.ReadToEnd().Trim()
                                End Using
                            End Using
                            Dim lines As String() = content.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
                            If lines.Length > 0 Then
                                Dim lastLine As String = lines(lines.Length - 1).Trim()
                                Dim bracketEnd As Integer = lastLine.IndexOf("] ")
                                If bracketEnd >= 0 AndAlso bracketEnd + 2 < lastLine.Length Then
                                    lastIP = lastLine.Substring(bracketEnd + 2).Trim()
                                End If
                            End If
                        Catch
                        End Try
                    End If

                    If Not String.Equals(lastIP, currentIP, StringComparison.OrdinalIgnoreCase) Then
                        Dim sb As New StringBuilder(64)
                        sb.Append("["c)
                        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        sb.Append("] ")
                        sb.AppendLine(currentIP)
                        AppendTextSafe(filePath, sb.ToString())
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

        Public Shared Function LogContains(keyword As String) As Boolean
            Try
                Dim filePath As String = LogsFilePath
                If SearchFileForKeyword(filePath, keyword) Then Return True

                Dim fallbackFile As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "AutoUpdate_Log.txt")
                If SearchFileForKeyword(fallbackFile, keyword) Then Return True
            Catch
            End Try
            Return False
        End Function

        Private Shared Function SearchFileForKeyword(filePath As String, keyword As String) As Boolean
            Try
                If Not File.Exists(filePath) Then Return False
                Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using reader As New StreamReader(fs, Encoding.UTF8)
                        Dim content As String = reader.ReadToEnd()
                        Return content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                    End Using
                End Using
            Catch
                Return False
            End Try
        End Function

    End Class

End Namespace
