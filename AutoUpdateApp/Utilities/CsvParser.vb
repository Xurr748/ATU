Option Strict On
Option Explicit On

Namespace Utilities

    ''' <summary>
    ''' Lightweight CSV parser for simple fixed-column CSV files.
    ''' No external dependencies required.
    ''' </summary>
    Public NotInheritable Class CsvParser

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Parses a CSV file and returns data rows as a list of string arrays.
        ''' Skips empty lines and the header row when hasHeader is True.
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
        ''' Splits a single CSV line into trimmed fields.
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
