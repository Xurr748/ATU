Option Strict On
Option Explicit On

Imports System.Text

Namespace Managers

    ''' <summary>
    ''' อ่านและเขียนไฟล์ updateflag.txt
    ''' ไฟล์ใช้รูปแบบ CSV: ComputerName,UpdateFlag
    ''' เขียนอย่างปลอดภัยต่อ Thread ด้วย SyncLock
    ''' </summary>
    Public NotInheritable Class UpdateFlagManager

        Private Shared ReadOnly _lock As New Object

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' ดึงค่า Flag การอัปเดตของเครื่องที่ระบุ
        ''' คืนค่า Nothing หากไม่พบเครื่องในไฟล์
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
        ''' ตั้งค่า Flag การอัปเดตสำหรับเครื่องที่ระบุ
        ''' แก้ไขรายการเดิม หรือเพิ่มรายการใหม่
        ''' เขียนไฟล์ทั้งหมดกลับคืนแบบ Atomic
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

                ' สร้างเนื้อหาไฟล์ใหม่
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
        ''' โหลดรายการ Flag ทั้งหมดจาก updateflag.txt
        ''' คืนค่า List ว่างหากไฟล์ไม่มีอยู่หรือว่างเปล่า
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
