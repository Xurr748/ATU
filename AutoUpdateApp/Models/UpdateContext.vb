Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' รวบรวมข้อมูลทั้งหมดที่จำเป็นสำหรับรอบการอัปเดตหนึ่งครั้ง
    ''' ส่งต่อให้ Strategy เพื่อตัดสินใจว่าจะทำอะไร
    ''' </summary>
    Public Class UpdateContext

        ''' <summary>ข้อมูลเครื่องทดสอบจาก TesterType.csv</summary>
        Public Property Tester As TesterInfo

        ''' <summary>เวอร์ชันที่ติดตั้งอยู่ปัจจุบัน (จาก Registry)</summary>
        Public Property CurrentVersion As String

        ''' <summary>เวอร์ชันล่าสุดที่มี (จาก version.txt)</summary>
        Public Property LatestVersion As String

        ''' <summary>True = มี Flag ค้างอยู่ใน updateflag.txt ว่ารอรีสตาร์ท</summary>
        Public Property HasPendingRestartFlag As Boolean

        ''' <summary>
        ''' คำนวณอัตโนมัติ: True เมื่อเวอร์ชันไม่ตรงกันและทั้งสองค่าไม่ว่าง
        ''' </summary>
        Public ReadOnly Property NeedsUpdate As Boolean
            Get
                If String.IsNullOrEmpty(CurrentVersion) OrElse String.IsNullOrEmpty(LatestVersion) Then
                    Return False
                End If
                Return Not String.Equals(CurrentVersion, LatestVersion, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

    End Class

End Namespace
