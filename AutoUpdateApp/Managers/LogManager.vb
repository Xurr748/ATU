Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text

Namespace Managers

    ''' <summary>
    ''' Thread-safe file-based logger with daily rotation.
    ''' Log format: [yyyy-MM-dd HH:mm:ss] [LEVEL] message
    ''' Logging never throws — swallows all internal errors.
    ''' </summary>
    Public NotInheritable Class LogManager

        Private Shared ReadOnly _lock As New Object
        Private Shared _logDirectory As String

        Private Sub New()
            ' Static-only class
        End Sub

        Private Shared ReadOnly Property LogDirectory As String
            Get
                If _logDirectory Is Nothing Then
                    _logDirectory = Config.AppSettings.LogPath
                    Try
                        If Not Directory.Exists(_logDirectory) Then
                            Directory.CreateDirectory(_logDirectory)
                        End If
                    Catch
                        ' Fall back to app directory
                        _logDirectory = AppDomain.CurrentDomain.BaseDirectory
                    End Try
                End If
                Return _logDirectory
            End Get
        End Property

        Private Shared ReadOnly Property LogFilePath As String
            Get
                Return Path.Combine(LogDirectory, "AutoUpdate_" & DateTime.Now.ToString("yyyy-MM-dd") & ".log")
            End Get
        End Property

        ''' <summary>Logs an informational message.</summary>
        Public Shared Sub Info(message As String)
            WriteLog("INFO", message)
        End Sub

        ''' <summary>Logs a warning message.</summary>
        Public Shared Sub Warn(message As String)
            WriteLog("WARN", message)
        End Sub

        ''' <summary>Logs an error message with optional exception details.</summary>
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
                    File.AppendAllText(LogFilePath, sb.ToString())
                End SyncLock
            Catch
                ' Logging must never crash the application
            End Try
        End Sub

        ''' <summary>
        ''' Resets the cached log directory (e.g. after config reload).
        ''' </summary>
        Public Shared Sub Reset()
            SyncLock _lock
                _logDirectory = Nothing
            End SyncLock
        End Sub

    End Class

End Namespace
