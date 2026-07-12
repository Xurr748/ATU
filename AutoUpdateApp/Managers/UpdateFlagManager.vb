Option Strict On
Option Explicit On

Imports System.Text

Namespace Managers

    ''' <summary>
    ''' Reads and writes the updateflag.txt file.
    ''' The file uses CSV format: ComputerName,UpdateFlag
    ''' Thread-safe writes via SyncLock.
    ''' </summary>
    Public NotInheritable Class UpdateFlagManager

        Private Shared ReadOnly _lock As New Object

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Gets the update flag for a specific computer.
        ''' Returns Nothing if the computer is not found in the file.
        ''' </summary>
        Public Shared Function GetFlag(computerName As String) As Boolean?
            Dim entries As List(Of Models.UpdateFlagEntry) = LoadAll()
            For Each entry In entries
                If String.Equals(entry.ComputerName, computerName, StringComparison.OrdinalIgnoreCase) Then
                    Return entry.UpdateFlag
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Sets the update flag for a specific computer.
        ''' Updates existing entry or adds a new one.
        ''' Writes the entire file back atomically.
        ''' </summary>
        Public Shared Sub SetFlag(computerName As String, value As Boolean)
            SyncLock _lock
                Dim entries As List(Of Models.UpdateFlagEntry) = LoadAll()
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

                ' Rebuild file content
                Dim sb As New StringBuilder(entries.Count * 30)
                sb.AppendLine("ComputerName,UpdateFlag")
                For Each entry In entries
                    sb.Append(entry.ComputerName)
                    sb.Append(",")
                    sb.AppendLine(entry.UpdateFlag.ToString())
                Next

                Dim filePath As String = Config.AppSettings.UpdateFlagPath
                Utilities.FileHelper.WriteAllTextSafe(filePath, sb.ToString())
                LogManager.Info("Update flag set: " & computerName & " = " & value.ToString())
            End SyncLock
        End Sub

        ''' <summary>
        ''' Loads all flag entries from updateflag.txt.
        ''' Returns empty list if file doesn't exist or is empty.
        ''' </summary>
        Private Shared Function LoadAll() As List(Of Models.UpdateFlagEntry)
            Dim result As New List(Of Models.UpdateFlagEntry)
            Dim filePath As String = Config.AppSettings.UpdateFlagPath

            If Not Utilities.FileHelper.FileExistsSafe(filePath) Then
                Return result
            End If

            Dim rows As List(Of String()) = Utilities.CsvParser.ParseFile(filePath, hasHeader:=True)
            For Each row In rows
                If row.Length >= 2 Then
                    Dim entry As New Models.UpdateFlagEntry()
                    entry.ComputerName = row(0)

                    Dim flag As Boolean
                    If Boolean.TryParse(row(1), flag) Then
                        entry.UpdateFlag = flag
                    End If

                    result.Add(entry)
                End If
            Next

            Return result
        End Function

    End Class

End Namespace
