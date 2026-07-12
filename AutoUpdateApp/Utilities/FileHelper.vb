Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading

Namespace Utilities

    ''' <summary>
    ''' จัดการอ่าน/เขียนไฟล์อย่างปลอดภัย พร้อมระบบลองใหม่อัตโนมัติ (Retry)
    ''' ออกแบบมาเพื่อรองรับ Network Path ที่อาจเชื่อมต่อไม่เสถียร
    ''' </summary>
    Public NotInheritable Class FileHelper

        Private Const RetryDelayMs As Integer = 150

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' อ่านไฟล์ทุกบรรทัด พร้อมลองใหม่อัตโนมัติหากเกิดข้อผิดพลาด
        ''' คืนค่า Nothing หากอ่านไม่สำเร็จหลังลองครบทุกรอบ
        ''' </summary>
        Public Shared Function ReadAllLinesSafe(filePath As String, Optional maxRetries As Integer = 3) As String()
            For attempt As Integer = 1 To maxRetries
                Try
                    Return File.ReadAllLines(filePath)
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                Catch ex As UnauthorizedAccessException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' อ่านเนื้อหาไฟล์ทั้งหมดเป็น String พร้อมลองใหม่อัตโนมัติ
        ''' คืนค่า Nothing หากอ่านไม่สำเร็จหลังลองครบทุกรอบ
        ''' </summary>
        Public Shared Function ReadAllTextSafe(filePath As String, Optional maxRetries As Integer = 3) As String
            For attempt As Integer = 1 To maxRetries
                Try
                    Return File.ReadAllText(filePath)
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                Catch ex As UnauthorizedAccessException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' เขียนข้อมูลลงไฟล์พร้อมลองใหม่อัตโนมัติ
        ''' สร้างโฟลเดอร์ให้อัตโนมัติหากยังไม่มี
        ''' </summary>
        Public Shared Sub WriteAllTextSafe(filePath As String, content As String, Optional maxRetries As Integer = 3)
            Dim dir As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            For attempt As Integer = 1 To maxRetries
                Try
                    File.WriteAllText(filePath, content)
                    Return
                Catch ex As IOException When attempt < maxRetries
                    Thread.Sleep(RetryDelayMs * attempt)
                End Try
            Next
        End Sub

        ''' <summary>
        ''' ตรวจสอบว่าไฟล์มีอยู่จริงหรือไม่ โดยไม่โยน Exception
        ''' </summary>
        Public Shared Function FileExistsSafe(filePath As String) As Boolean
            Try
                Return File.Exists(filePath)
            Catch
                Return False
            End Try
        End Function

        ''' <summary>
        ''' ดึงเวลาแก้ไขล่าสุดของไฟล์ (UTC) โดยไม่โยน Exception
        ''' คืนค่า DateTime.MinValue หากอ่านไม่ได้
        ''' </summary>
        Public Shared Function GetLastWriteTimeSafe(filePath As String) As DateTime
            Try
                Return File.GetLastWriteTimeUtc(filePath)
            Catch
                Return DateTime.MinValue
            End Try
        End Function

    End Class

End Namespace
