Option Strict On
Option Explicit On

Namespace Utilities

    ''' <summary>
    ''' ดึงข้อมูลสภาพแวดล้อมของเครื่อง พร้อม Cache อัตโนมัติ
    ''' </summary>
    Public NotInheritable Class EnvironmentHelper

        Private Shared _computerName As String

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' ชื่อเครื่องคอมพิวเตอร์ (Cache หลังเรียกครั้งแรก ไม่ต้องเรียก System ซ้ำ)
        ''' </summary>
        Public Shared ReadOnly Property ComputerName As String
            Get
                If _computerName Is Nothing Then
                    _computerName = Environment.MachineName
                End If
                Return _computerName
            End Get
        End Property

    End Class

End Namespace
