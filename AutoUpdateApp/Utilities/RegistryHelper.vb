Option Strict On
Option Explicit On

Imports Microsoft.Win32

Namespace Utilities

    ''' <summary>
    ''' อ่านค่าจาก Windows Registry อย่างปลอดภัย
    ''' รองรับทั้ง 64-bit และ 32-bit Registry View
    ''' แก้ปัญหา WOW6432Node Redirection บนระบบ 64-bit
    ''' </summary>
    Public NotInheritable Class RegistryHelper

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' อ่านค่า String จาก Registry ตาม Key Path และ Value Name ที่ระบุ
        ''' ลองอ่านจาก 64-bit Registry ก่อน ถ้าไม่พบจะลองอ่านจาก 32-bit (WOW6432Node)
        ''' คืนค่า Nothing หากไม่พบ Key/Value หรือสิทธิ์ไม่เพียงพอ
        ''' </summary>
        ''' <param name="keyPath">เส้นทาง Registry เต็ม (เช่น HKEY_LOCAL_MACHINE\SOFTWARE\MyApp)</param>
        ''' <param name="valueName">ชื่อของ Value ที่ต้องการอ่าน</param>
        Public Shared Function ReadValue(keyPath As String, valueName As String) As String
            ' แยก Root Key ออกจาก Sub Key Path
            Dim rootKey As RegistryHive
            Dim subKeyPath As String = ""
            If Not ParseKeyPath(keyPath, rootKey, subKeyPath) Then
                Return Nothing
            End If

            ' ลองอ่านจาก 64-bit Registry ก่อน
            Dim result As String = ReadFromView(rootKey, subKeyPath, valueName, RegistryView.Registry64)
            If result IsNot Nothing Then
                Return result
            End If

            ' ถ้าไม่พบ ลองอ่านจาก 32-bit Registry (WOW6432Node)
            result = ReadFromView(rootKey, subKeyPath, valueName, RegistryView.Registry32)
            Return result
        End Function

        Private Shared Function ReadFromView(rootKey As RegistryHive, subKeyPath As String, valueName As String, view As RegistryView) As String
            Try
                Using baseKey As RegistryKey = RegistryKey.OpenBaseKey(rootKey, view)
                    Using subKey As RegistryKey = baseKey.OpenSubKey(subKeyPath, False)
                        If subKey IsNot Nothing Then
                            Dim value As Object = subKey.GetValue(valueName, Nothing)
                            If value IsNot Nothing Then
                                Return value.ToString()
                            End If
                        End If
                    End Using
                End Using
            Catch ex As Security.SecurityException
                ' สิทธิ์ไม่เพียงพอ
            Catch ex As Exception
                ' ไม่พบ Key หรือเกิดข้อผิดพลาดอื่น
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' แยก Root Key (เช่น HKEY_LOCAL_MACHINE) และ Sub Key Path ออกจาก Full Path
        ''' </summary>
        Private Shared Function ParseKeyPath(keyPath As String, ByRef rootKey As RegistryHive, ByRef subKeyPath As String) As Boolean
            rootKey = RegistryHive.LocalMachine
            subKeyPath = ""

            If String.IsNullOrEmpty(keyPath) Then Return False

            Dim separatorIndex As Integer = keyPath.IndexOf("\"c)
            Dim rootPart As String
            If separatorIndex >= 0 Then
                rootPart = keyPath.Substring(0, separatorIndex).ToUpperInvariant()
                subKeyPath = keyPath.Substring(separatorIndex + 1)
            Else
                rootPart = keyPath.ToUpperInvariant()
                subKeyPath = ""
            End If

            Select Case rootPart
                Case "HKEY_LOCAL_MACHINE"
                    rootKey = RegistryHive.LocalMachine
                Case "HKEY_CURRENT_USER"
                    rootKey = RegistryHive.CurrentUser
                Case "HKEY_CLASSES_ROOT"
                    rootKey = RegistryHive.ClassesRoot
                Case "HKEY_USERS"
                    rootKey = RegistryHive.Users
                Case "HKEY_CURRENT_CONFIG"
                    rootKey = RegistryHive.CurrentConfig
                Case Else
                    Return False
            End Select

            Return True
        End Function

    End Class

End Namespace
