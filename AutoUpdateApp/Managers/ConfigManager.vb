Option Strict On
Option Explicit On

Namespace Managers

    ''' <summary>
    ''' จัดการข้อมูล TesterType.csv พร้อมระบบ Cache อัตโนมัติ
    ''' เก็บ Cache รายชื่อเครื่อง โหลดใหม่เฉพาะเมื่อไฟล์ถูกแก้ไข
    ''' ปลอดภัยต่อ Thread ด้วย SyncLock
    ''' </summary>
    Public NotInheritable Class ConfigManager

        Private Shared _testers As List(Of Models.TesterInfo)
        Private Shared _lastModified As DateTime = DateTime.MinValue
        Private Shared ReadOnly _lock As New Object

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' คืนค่ารายการเครื่องทดสอบทั้งหมดจาก TesterType.csv
        ''' ใช้ข้อมูลจาก Cache หากไฟล์ไม่ได้ถูกแก้ไข
        ''' </summary>
        Public Shared Function LoadAll() As List(Of Models.TesterInfo)
            Dim filePath As String = Config.AppSettings.TesterTypePath

            SyncLock _lock
                Dim currentModified As DateTime = Utilities.FileHelper.GetLastWriteTimeSafe(filePath)

                ' คืนค่าจาก Cache หากไฟล์ไม่ได้ถูกแก้ไข
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

        ''' <summary>
        ''' ค้นหาข้อมูลเครื่องทดสอบตามชื่อเครื่อง (ไม่สนตัวพิมพ์เล็ก-ใหญ่)
        ''' คืนค่า Nothing หากไม่พบ
        ''' </summary>
        Public Shared Function GetTesterByName(computerName As String) As Models.TesterInfo
            Dim all As List(Of Models.TesterInfo) = LoadAll()
            For Each tester In all
                If String.Equals(tester.ComputerName, computerName, StringComparison.OrdinalIgnoreCase) Then
                    Return tester
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' บังคับโหลดใหม่ในครั้งถัดไปโดยล้าง Cache
        ''' </summary>
        Public Shared Sub InvalidateCache()
            SyncLock _lock
                _testers = Nothing
                _lastModified = DateTime.MinValue
            End SyncLock
        End Sub

    End Class

End Namespace
