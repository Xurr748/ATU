Option Strict On
Option Explicit On

Imports System.Configuration
Imports System.IO

Namespace Config

    ''' <summary>
    ''' ศูนย์กลางการอ่านค่าตั้งต่าง (Settings) จาก App.config
    ''' ทุกเส้นทาง (Path) ตั้งค่าได้ — ไม่มี Hardcode ในโค้ด
    ''' เส้นทางสัมพัทธ์จะถูกรวมกับ ConfigRoot อัตโนมัติ
    ''' </summary>
    Public NotInheritable Class AppSettings

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        Private Shared Function GetSetting(key As String, Optional defaultValue As String = "") As String
            Dim value As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(value) Then
                Return defaultValue
            End If
            Return value
        End Function

        Private Shared Function ResolvePath(configRoot As String, path As String) As String
            If String.IsNullOrEmpty(path) Then
                Return path
            End If
            If IO.Path.IsPathRooted(path) Then
                Return path
            End If
            Return IO.Path.Combine(configRoot, path)
        End Function

        ' ───────────────────── เส้นทางหลัก ─────────────────────

        ''' <summary>โฟลเดอร์หลักสำหรับไฟล์ Config</summary>
        Public Shared ReadOnly Property ConfigRoot As String
            Get
                Return GetSetting("ConfigRoot", "")
            End Get
        End Property

        ''' <summary>เส้นทางไฟล์ TesterType.csv (รวมกับ ConfigRoot อัตโนมัติ)</summary>
        Public Shared ReadOnly Property TesterTypePath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("TesterTypePath", "TesterType.csv"))
            End Get
        End Property

        ''' <summary>เส้นทางไฟล์ version.txt (รวมกับ ConfigRoot อัตโนมัติ)</summary>
        Public Shared ReadOnly Property VersionFilePath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("VersionFilePath", "version.txt"))
            End Get
        End Property

        ''' <summary>เส้นทางไฟล์ updateflag.txt (รวมกับ ConfigRoot อัตโนมัติ)</summary>
        Public Shared ReadOnly Property UpdateFlagPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("UpdateFlagPath", "updateflag.txt"))
            End Get
        End Property

        ' ───────────────────── เส้นทาง Installer ─────────────────────

        ''' <summary>เส้นทาง Installer สำหรับเครื่องประเภท HE</summary>
        Public Shared ReadOnly Property InstallerPathHE As String
            Get
                Return GetSetting("InstallerPathHE", "")
            End Get
        End Property

        ''' <summary>เส้นทาง Installer สำหรับเครื่องประเภท LLE</summary>
        Public Shared ReadOnly Property InstallerPathLLE As String
            Get
                Return GetSetting("InstallerPathLLE", "")
            End Get
        End Property

        ''' <summary>อาร์กิวเมนต์ที่ส่งให้ Installer (เช่น /silent /norestart)</summary>
        Public Shared ReadOnly Property InstallerArgs As String
            Get
                Return GetSetting("InstallerArgs", "/silent /norestart")
            End Get
        End Property

        ' ───────────────────── Registry ─────────────────────

        ''' <summary>เส้นทาง Registry Key สำหรับอ่านเวอร์ชันปัจจุบัน</summary>
        Public Shared ReadOnly Property RegistryKeyPath As String
            Get
                Return GetSetting("RegistryKeyPath", "HKEY_LOCAL_MACHINE\SOFTWARE\MyApp")
            End Get
        End Property

        ''' <summary>ชื่อ Registry Value สำหรับเวอร์ชัน</summary>
        Public Shared ReadOnly Property RegistryValueName As String
            Get
                Return GetSetting("RegistryValueName", "Version")
            End Get
        End Property

        ''' <summary>ชื่อ Registry Value สำหรับเส้นทางไฟล์ Executable ของโปรแกรม</summary>
        Public Shared ReadOnly Property RegistryPathValueName As String
            Get
                Return GetSetting("RegistryPathValueName", "Path")
            End Get
        End Property

        ''' <summary>ชื่อบริษัทสำหรับจัดกลุ่มโฟลเดอร์ Log</summary>
        Public Shared ReadOnly Property CompanyName As String
            Get
                Return GetSetting("CompanyName", "CompanyName")
            End Get
        End Property

        ' ───────────────────── การบันทึก Log ─────────────────────

        ''' <summary>โฟลเดอร์สำหรับเก็บไฟล์ Log</summary>
        Public Shared ReadOnly Property LogPath As String
            Get
                Return GetSetting("LogPath", "C:\Logs\AutoUpdate\")
            End Get
        End Property

        ''' <summary>
        ''' รูปแบบชื่อไฟล์ Log — ใช้ {ComputerName} เป็น placeholder แทนชื่อเครื่อง
        ''' ตัวอย่าง: "{ComputerName}_Logs.txt" → "PC001_Logs.txt"
        ''' </summary>
        Public Shared ReadOnly Property LogFileName As String
            Get
                Return GetSetting("LogFileName", "{ComputerName}_Logs.txt")
            End Get
        End Property

        ' ───────────────────── เอกสาร (Details) ─────────────────────

        ''' <summary>เส้นทางไฟล์ PDF สำหรับข้อมูลทั่วไป (Info)</summary>
        Public Shared ReadOnly Property DetailInfoPdfPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("DetailInfoPdfPath", ""))
            End Get
        End Property

        ''' <summary>เส้นทางไฟล์ PDF สำหรับรายละเอียดเพิ่มเติม (Detail)</summary>
        Public Shared ReadOnly Property DetailPdfPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("DetailPdfPath", ""))
            End Get
        End Property

        ''' <summary>ระดับการบันทึก Log (Info, Warn, Error)</summary>
        Public Shared ReadOnly Property LogLevel As String
            Get
                Return GetSetting("LogLevel", "Info")
            End Get
        End Property

        ' ───────────────────── ตัวตั้งเวลา ─────────────────────

        ''' <summary>ระยะเวลาตรวจสอบการอัปเดต (หน่วย: นาที)</summary>
        Public Shared ReadOnly Property PollingIntervalMinutes As Integer
            Get
                Dim value As Integer
                If Integer.TryParse(GetSetting("PollingIntervalMinutes", "60"), value) Then
                    Return value
                End If
                Return 60
            End Get
        End Property

        ' ───────────────────── โหลดใหม่ ─────────────────────

        ''' <summary>
        ''' บังคับให้อ่านค่า app.config ใหม่ในครั้งถัดไป
        ''' </summary>
        Public Shared Sub Reload()
            ConfigurationManager.RefreshSection("appSettings")
        End Sub

    End Class

End Namespace
