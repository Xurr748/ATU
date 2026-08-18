Option Strict On
Option Explicit On

Namespace Utilities

    Public NotInheritable Class CsvParser

        Private Sub New()
        End Sub

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
