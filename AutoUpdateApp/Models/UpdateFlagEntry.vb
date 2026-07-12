Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' ข้อมูล Flag การอัปเดต 1 แถว จากไฟล์ updateflag.txt
    ''' คอลัมน์: ComputerName, UpdateFlag
    ''' </summary>
    Public Class UpdateFlagEntry

        ''' <summary>ชื่อเครื่อง (เช่น PC001)</summary>
        Public Property ComputerName As String

        ''' <summary>True = มีการอัปเดตค้างอยู่หลังรีสตาร์ท</summary>
        Public Property UpdateFlag As Boolean

    End Class

End Namespace
