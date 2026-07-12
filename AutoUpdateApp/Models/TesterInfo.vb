Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' ข้อมูลเครื่องทดสอบ 1 แถว จากไฟล์ TesterType.csv
    ''' คอลัมน์: ComputerName, Type, Mode, Time
    ''' </summary>
    Public Class TesterInfo

        ''' <summary>ชื่อเครื่อง (เช่น PC001)</summary>
        Public Property ComputerName As String

        ''' <summary>ประเภทเครื่องทดสอบ: HE หรือ LLE</summary>
        Public Property TesterType As String

        ''' <summary>โหมดการอัปเดต: EVA, Normal หรือ Auto</summary>
        Public Property Mode As String

        ''' <summary>เวลาที่กำหนดให้ตรวจสอบการอัปเดต</summary>
        Public Property ScheduledTime As TimeSpan

    End Class

End Namespace
