Option Strict On
Option Explicit On

Imports System.Text
Imports System.IO

Namespace Managers

    Public NotInheritable Class UpdateFlagManager

        Private Shared ReadOnly _lock As New Object

        Private Sub New()
        End Sub

        Public Shared Function GetFlag(computerName As String) As Boolean?
            SyncLock _lock
                Dim maxRetries As Integer = 5
                For attempt As Integer = 1 To maxRetries
                    Try
                        Dim entries As List(Of Models.UpdateFlagEntry) = LoadAllSafe()
                        For Each entry In entries
                            If String.Equals(entry.ComputerName, computerName, StringComparison.OrdinalIgnoreCase) Then
                                Return entry.UpdateFlag
                            End If
                        Next
                        Return Nothing
                    Catch ex As IOException
                        If attempt < maxRetries Then
                            Threading.Thread.Sleep(300 * attempt)
                        Else
                            LogManager.Warn("GetFlag failed after " & maxRetries & " retries: " & ex.Message)
                            Return Nothing
                        End If
                    End Try
                Next
                Return Nothing
            End SyncLock
        End Function

        Public Shared Sub SetFlag(computerName As String, value As Boolean)
            SyncLock _lock
                Dim filePath As String = Config.AppSettings.UpdateFlagPath
                Dim maxRetries As Integer = 10
                Dim rng As New Random()

                For attempt As Integer = 1 To maxRetries
                    Try
                        Using fs As New FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
                            Dim reader As New StreamReader(fs, Encoding.UTF8)
                            Dim content As String = reader.ReadToEnd()

                            Dim entries As List(Of Models.UpdateFlagEntry) = ParseEntries(content)
                            Dim found As Boolean = False

                            For Each entry In entries
                                If String.Equals(entry.ComputerName, computerName, StringComparison.OrdinalIgnoreCase) Then
                                    entry.UpdateFlag = value
                                    found = True
                                    Exit For
                                End If
                            Next

                            If Not found Then
                                Dim newEntry As New Models.UpdateFlagEntry()
                                newEntry.ComputerName = computerName
                                newEntry.UpdateFlag = value
                                entries.Add(newEntry)
                            End If

                            Dim sb As New StringBuilder(entries.Count * 30)
                            sb.AppendLine("ComputerName,UpdateFlag")
                            For Each entry In entries
                                sb.Append(entry.ComputerName)
                                sb.Append(",")
                                sb.AppendLine(entry.UpdateFlag.ToString())
                            Next

                            fs.SetLength(0)
                            fs.Seek(0, SeekOrigin.Begin)
                            Dim writer As New StreamWriter(fs, Encoding.UTF8)
                            writer.Write(sb.ToString())
                            writer.Flush()
                        End Using

                        LogManager.Info("Update flag set: " & computerName & " = " & value.ToString())
                        Return
                    Catch ex As IOException
                        If attempt < maxRetries Then
                            Dim delayMs As Integer = (500 * attempt) + rng.Next(0, 300)
                            LogManager.Warn("UpdateFlagManager.SetFlag locked (attempt " & attempt & "/" & maxRetries & "): " & ex.Message)
                            Threading.Thread.Sleep(delayMs)
                        Else
                            LogManager.Error("UpdateFlagManager.SetFlag failed after " & maxRetries & " attempts.", ex)
                        End If
                    End Try
                Next
            End SyncLock
        End Sub

        Private Shared Function LoadAllSafe() As List(Of Models.UpdateFlagEntry)
            Dim result As New List(Of Models.UpdateFlagEntry)
            Dim filePath As String = Config.AppSettings.UpdateFlagPath

            If Not Utilities.FileHelper.FileExistsSafe(filePath) Then
                Return result
            End If

            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using reader As New StreamReader(fs, Encoding.UTF8)
                    Dim content As String = reader.ReadToEnd()
                    Return ParseEntries(content)
                End Using
            End Using
        End Function

        Private Shared Function ParseEntries(content As String) As List(Of Models.UpdateFlagEntry)
            Dim result As New List(Of Models.UpdateFlagEntry)
            If String.IsNullOrEmpty(content) Then Return result

            Dim lines As String() = content.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            Dim isFirst As Boolean = True

            For Each line As String In lines
                If isFirst Then
                    isFirst = False
                    Continue For
                End If

                Dim parts As String() = line.Split(","c)
                If parts.Length >= 2 Then
                    Dim entry As New Models.UpdateFlagEntry()
                    entry.ComputerName = parts(0).Trim()
                    Dim flag As Boolean
                    If Boolean.TryParse(parts(1).Trim(), flag) Then
                        entry.UpdateFlag = flag
                    End If
                    result.Add(entry)
                End If
            Next

            Return result
        End Function

    End Class

End Namespace
