Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading

Namespace Utilities

    ''' <summary>
    ''' Provides safe file I/O operations with retry logic,
    ''' designed for reliability over network paths.
    ''' </summary>
    Public NotInheritable Class FileHelper

        Private Const RetryDelayMs As Integer = 150

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Reads all lines from a file with retry logic.
        ''' Returns Nothing on failure after all retries.
        ''' </summary>
        Public Shared Function ReadAllLinesSafe(filePath As String, Optional maxRetries As Integer = 3) As String()
            For attempt As Integer = 1 To maxRetries
                Try
                    Return File.ReadAllLines(filePath)
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                Catch ex As UnauthorizedAccessException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Reads all text from a file with retry logic.
        ''' Returns Nothing on failure after all retries.
        ''' </summary>
        Public Shared Function ReadAllTextSafe(filePath As String, Optional maxRetries As Integer = 3) As String
            For attempt As Integer = 1 To maxRetries
                Try
                    Return File.ReadAllText(filePath)
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                Catch ex As UnauthorizedAccessException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Writes text to a file with retry logic.
        ''' Creates parent directories if they don't exist.
        ''' </summary>
        Public Shared Sub WriteAllTextSafe(filePath As String, content As String, Optional maxRetries As Integer = 3)
            Dim dir As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            For attempt As Integer = 1 To maxRetries
                Try
                    File.WriteAllText(filePath, content)
                    Return
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
        End Sub

        ''' <summary>
        ''' Checks if a file exists without throwing.
        ''' </summary>
        Public Shared Function FileExistsSafe(filePath As String) As Boolean
            Try
                Return File.Exists(filePath)
            Catch
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Gets the last write time (UTC) of a file without throwing.
        ''' Returns DateTime.MinValue on failure.
        ''' </summary>
        Public Shared Function GetLastWriteTimeSafe(filePath As String) As DateTime
            Try
                Return File.GetLastWriteTimeUtc(filePath)
            Catch
                Return DateTime.MinValue
            End Try
        End Function

    End Class

End Namespace
