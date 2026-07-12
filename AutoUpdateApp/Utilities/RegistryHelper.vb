Option Strict On
Option Explicit On

Imports Microsoft.Win32

Namespace Utilities

    ''' <summary>
    ''' อ่านค่าจาก Windows Registry อย่างปลอดภัย
    ''' คืนค่า Nothing หากอ่านไม่ได้ — ผู้เรียกใช้เป็นคนจัดการ Log เอง
    ''' </summary>
    Public NotInheritable Class RegistryHelper

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' อ่านค่า String จาก Registry ตาม Key Path และ Value Name ที่ระบุ
        ''' คืนค่า Nothing หากไม่พบ Key/Value หรือสิทธิ์ไม่เพียงพอ
        ''' </summary>
        ''' <param name="keyPath">เส้นทาง Registry เต็ม (เช่น HKEY_LOCAL_MACHINE\SOFTWARE\MyApp)</param>
        ''' <param name="valueName">ชื่อของ Value ที่ต้องการอ่าน</param>
        Public Shared Function ReadValue(keyPath As String, valueName As String) As String
            Try
                Dim value As Object = Registry.GetValue(keyPath, valueName, Nothing)
                If value IsNot Nothing Then
                    Return value.ToString()
                End If
            Catch ex As Security.SecurityException
                ' สิทธิ์ไม่เพียงพอ — ผู้เรียกใช้จัดการ Log เอง
            Catch ex As Exception
                ' ไม่พบ Key หรือเกิดข้อผิดพลาดอื่น — ผู้เรียกใช้จัดการ Log เอง
            End Try
            Return Nothing
        End Function

    End Class

End Namespace
