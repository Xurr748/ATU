Option Strict On
Option Explicit On

Namespace Managers

    Public NotInheritable Class ConfigManager

        Private Shared _testers As List(Of Models.TesterInfo)
        Private Shared _lastModified As DateTime = DateTime.MinValue
        Private Shared ReadOnly _lock As New Object

        Private Sub New()
        End Sub

        Public Shared Function LoadAll() As List(Of Models.TesterInfo)
            Dim filePath As String = Config.AppSettings.TesterTypePath

            SyncLock _lock
                Dim currentModified As DateTime = Utilities.FileHelper.GetLastWriteTimeSafe(filePath)

                If _testers IsNot Nothing AndAlso currentModified <= _lastModified Then
                    Return _testers
                End If

                Dim rows As List(Of String()) = Utilities.CsvParser.ParseFile(filePath, hasHeader:=True)
                Dim result As New List(Of Models.TesterInfo)(rows.Count)

                For Each row In rows
                    If row.Length >= 4 Then
                        Dim info As New Models.TesterInfo()
                        info.ComputerName = row(0)
                        info.TesterType = row(1)
                        info.Mode = row(2)

                        Dim ts As TimeSpan
                        If TimeSpan.TryParse(row(3), ts) Then
                            info.ScheduledTime = ts
                        End If

                        result.Add(info)
                    End If
                Next

                _testers = result
                _lastModified = currentModified
                LogManager.Info("Loaded " & result.Count.ToString() & " tester entries from config.")
                Return result
            End SyncLock
        End Function

        Public Shared Function GetTesterByName(computerName As String) As Models.TesterInfo
            Dim all As List(Of Models.TesterInfo) = LoadAll()
            For Each tester In all
                If String.Equals(tester.ComputerName, computerName, StringComparison.OrdinalIgnoreCase) Then
                    Return tester
                End If
            Next
            Return Nothing
        End Function

        Public Shared Sub InvalidateCache()
            SyncLock _lock
                _testers = Nothing
                _lastModified = DateTime.MinValue
            End SyncLock
        End Sub

    End Class

End Namespace
