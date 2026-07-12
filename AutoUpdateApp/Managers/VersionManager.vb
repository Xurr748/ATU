Option Strict On
Option Explicit On

Namespace Managers

    ''' <summary>
    ''' อ่านและเปรียบเทียบเวอร์ชันของแอปพลิเคชัน
    ''' เวอร์ชันปัจจุบันอ่านจาก Windows Registry
    ''' เวอร์ชันล่าสุดอ่านจากไฟล์ version.txt บน Config Share
    ''' </summary>
    Public NotInheritable Class VersionManager

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' อ่านเวอร์ชันที่ติดตั้งอยู่จาก Registry
        ''' คืนค่า String ว่างหากอ่านไม่ได้
        ''' </summary>
        Public Shared Function ReadRegistryVersion() As String
            Dim keyPath As String = Config.AppSettings.RegistryKeyPath
            Dim valueName As String = Config.AppSettings.RegistryValueName
            Dim version As String = Utilities.RegistryHelper.ReadValue(keyPath, valueName)

            If version Is Nothing Then
                LogManager.Warn("Could not read version from registry: " & keyPath & "\" & valueName)
                Return String.Empty
            End If

            Return version.Trim()
        End Function

        ''' <summary>
        ''' อ่านเวอร์ชันล่าสุดจากไฟล์ version.txt
        ''' คืนค่า String ว่างหากอ่านไฟล์ไม่ได้
        ''' </summary>
        Public Shared Function ReadLatestVersion() As String
            Dim filePath As String = Config.AppSettings.VersionFilePath
            Dim content As String = Utilities.FileHelper.ReadAllTextSafe(filePath)

            If content Is Nothing Then
                LogManager.Warn("Could not read version file: " & filePath)
                Return String.Empty
            End If

            Return content.Trim()
        End Function

        ''' <summary>
        ''' คืนค่า True หากเวอร์ชันที่ติดตั้งต่างจากเวอร์ชันล่าสุด
        ''' คืนค่า False หากเวอร์ชันใดเวอร์ชันหนึ่งว่าง (ป้องกันการอัปเดตที่ผิดพลาด)
        ''' </summary>
        Public Shared Function NeedsUpdate() As Boolean
            Dim current As String = ReadRegistryVersion()
            Dim latest As String = ReadLatestVersion()

            If String.IsNullOrEmpty(current) OrElse String.IsNullOrEmpty(latest) Then
                Return False
            End If

            Return Not String.Equals(current, latest, StringComparison.OrdinalIgnoreCase)
        End Function

    End Class

End Namespace
