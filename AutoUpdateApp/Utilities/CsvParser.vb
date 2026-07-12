Option Strict On
Option Explicit On

Namespace Utilities

    ''' <summary>
    ''' ตัวแยกวิเคราะห์ไฟล์ CSV แบบเบา สำหรับไฟล์ CSV คอลัมน์คงที่
    ''' ไม่ต้องใช้ไลบรารีภายนอก ใช้ FileHelper ในการอ่านไฟล์
    ''' </summary>
    Public NotInheritable Class CsvParser

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' อ่านไฟล์ CSV แล้วแปลงแต่ละแถวเป็น Array ของ String
        ''' ข้ามบรรทัดว่างและหัวตาราง (Header) เมื่อ hasHeader เป็น True
        ''' </summary>
        Public Shared Function ParseFile(filePath As String, Optional hasHeader As Boolean = True) As List(Of String())
            Dim rows As New List(Of String())
            Dim lines = FileHelper.ReadAllLinesSafe(filePath)

            If lines Is Nothing OrElse lines.Length = 0 Then
                Return rows
            End If

            Dim startIndex As Integer = If(hasHeader, 1, 0)

            For i As Integer = startIndex To lines.Length - 1
                Dim line As String = lines(i)
                If Not String.IsNullOrWhiteSpace(line) Then
                    rows.Add(ParseLine(line))
                End If
            Next

            Return rows
        End Function

        ''' <summary>
        ''' แยกบรรทัด CSV หนึ่งบรรทัดเป็น Array ของ String พร้อมตัดช่องว่างหัวท้าย
        ''' </summary>
        Public Shared Function ParseLine(line As String) As String()
            If String.IsNullOrEmpty(line) Then
                Return New String() {}
            End If

            Dim fields As String() = line.Split(","c)
            For i As Integer = 0 To fields.Length - 1
                fields(i) = fields(i).Trim()
            Next
            Return fields
        End Function

    End Class

End Namespace
