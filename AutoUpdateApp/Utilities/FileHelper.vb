Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading

Namespace Utilities

    Public NotInheritable Class FileHelper

        Private Const RetryDelayMs As Integer = 150

        Private Sub New()
        End Sub

        Public Shared Function ReadAllLinesSafe(filePath As String, Optional maxRetries As Integer = 3) As String()
            For attempt As Integer = 1 To maxRetries
                Try
                    Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As New StreamReader(fs)
                            Dim lines As New System.Collections.Generic.List(Of String)()
                            Dim line As String = reader.ReadLine()
                            While line IsNot Nothing
                                lines.Add(line)
                                line = reader.ReadLine()
                            End While
                            Return lines.ToArray()
                        End Using
                    End Using
                Catch ex As Exception
                    If attempt = maxRetries Then
                        Managers.LogManager.Warn("Failed to read lines from file: " & filePath & " - " & ex.Message)
                        Return Nothing
                    End If
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        Public Shared Function ReadAllTextSafe(filePath As String, Optional maxRetries As Integer = 3) As String
            For attempt As Integer = 1 To maxRetries
                Try
                    Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As New StreamReader(fs)
                            Return reader.ReadToEnd()
                        End Using
                    End Using
                Catch ex As Exception
                    If attempt = maxRetries Then
                        Managers.LogManager.Warn("Failed to read text from file: " & filePath & " - " & ex.Message)
                        Return Nothing
                    End If
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        Public Shared Sub WriteAllTextSafe(filePath As String, content As String, Optional maxRetries As Integer = 3)
            Dim dir As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            For attempt As Integer = 1 To maxRetries
                Try
                    File.WriteAllText(filePath, content)
                    Return
                Catch ex As Exception
                    If attempt = maxRetries Then
                        Managers.LogManager.Warn("Failed to write text to file: " & filePath & " - " & ex.Message)
                        Return
                    End If
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
        End Sub

        Public Shared Function FileExistsSafe(filePath As String) As Boolean
            Try
                Return File.Exists(filePath)
            Catch
                Return False
            End Try
        End Function

        Public Shared Function GetLastWriteTimeSafe(filePath As String) As DateTime
            Try
                Return File.GetLastWriteTimeUtc(filePath)
            Catch
                Return DateTime.MinValue
            End Try
        End Function

    End Class

End Namespace
