Option Strict On
Option Explicit On

Imports System.IO

Namespace Config

    ''' <summary>
    ''' ศูนย์กลางการอ่านค่าตั้งต่าง (Settings) จากไฟล์ config.txt
    ''' ไฟล์อยู่ข้างๆ .exe — แก้ไขจาก Server ได้โดยไม่ต้องคอมไพล์ใหม่
    ''' รูปแบบ: Key = Value (1 บรรทัดต่อ 1 ค่า, ; เป็น comment)
    ''' </summary>
    Public NotInheritable Class AppSettings

        Private Shared ReadOnly _lock As New Object
        Private Shared _settings As Dictionary(Of String, String)

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' โหลดค่าทั้งหมดจาก config.txt เข้า Dictionary (เรียกครั้งเดียว หรือเมื่อ Reload)
        ''' </summary>
        Private Shared Sub EnsureLoaded()
            If _settings IsNot Nothing Then Return

            SyncLock _lock
                If _settings IsNot Nothing Then Return

                _settings = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                Dim configPath As String = GetConfigFilePath()

                If Not File.Exists(configPath) Then Return

                Try
                    Dim lines As String() = File.ReadAllLines(configPath)
                    For Each line As String In lines
                        Dim trimmed As String = line.Trim()

                        ' ข้ามบรรทัดว่างและ comment
                        If String.IsNullOrEmpty(trimmed) Then Continue For
                        If trimmed.StartsWith(";") OrElse trimmed.StartsWith("#") Then Continue For

                        ' แยก Key = Value
                        Dim eqIndex As Integer = trimmed.IndexOf("="c)
                        If eqIndex > 0 Then
                            Dim key As String = trimmed.Substring(0, eqIndex).Trim()
                            Dim value As String = trimmed.Substring(eqIndex + 1).Trim()

                            ' ลบ quotes ถ้ามี
                            If value.Length >= 2 AndAlso value.StartsWith("""") AndAlso value.EndsWith("""") Then
                                value = value.Substring(1, value.Length - 2)
                            End If

                            _settings(key) = value
                        End If
                    Next
                Catch
                    ' ถ้าอ่านไฟล์ไม่ได้ ใช้ค่าเริ่มต้น
                End Try
            End SyncLock
        End Sub

        Private Shared Function GetConfigFilePath() As String
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt")
        End Function

        Private Shared Function GetSetting(key As String, Optional defaultValue As String = "") As String
            EnsureLoaded()
            Dim value As String = Nothing
            If _settings.TryGetValue(key, value) Then
                If Not String.IsNullOrWhiteSpace(value) Then
                    Return value
                End If
            End If
            Return defaultValue
        End Function

        Private Shared Function GetBoolSetting(key As String, Optional defaultValue As Boolean = True) As Boolean
            Dim value As String = GetSetting(key, defaultValue.ToString())
            Dim result As Boolean
            If Boolean.TryParse(value, result) Then
                Return result
            End If
            Return defaultValue
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

        ' ───────────────────── Startup Management ─────────────────────

        ''' <summary>เปิด/ปิด การใส่ตัว AutoUpdateApp เองไปที่ Startup</summary>
        Public Shared ReadOnly Property EnableSelfStartup As Boolean
            Get
                Return GetBoolSetting("EnableSelfStartup", True)
            End Get
        End Property

        ''' <summary>เปิด/ปิด การใส่ Target App ไปที่ Startup หลังอัปเดต</summary>
        Public Shared ReadOnly Property EnableTargetStartup As Boolean
            Get
                Return GetBoolSetting("EnableTargetStartup", True)
            End Get
        End Property

        ''' <summary>เปิด/ปิด การลบ Shortcut เดิมที่ชื่อตรงกันก่อนสร้างใหม่</summary>
        Public Shared ReadOnly Property RemoveOldStartupShortcut As Boolean
            Get
                Return GetBoolSetting("RemoveOldStartupShortcut", True)
            End Get
        End Property

        ''' <summary>ชื่อ Shortcut ที่ต้องการลบ/สร้าง (ว่าง = ใช้ชื่อ exe จาก registry)</summary>
        Public Shared ReadOnly Property StartupShortcutName As String
            Get
                Return GetSetting("StartupShortcutName", "")
            End Get
        End Property

        ' ───────────────────── โหลดใหม่ ─────────────────────

        ''' <summary>
        ''' บังคับให้อ่านค่า config.txt ใหม่ในครั้งถัดไป
        ''' </summary>
        Public Shared Sub Reload()
            SyncLock _lock
                _settings = Nothing
            End SyncLock
        End Sub

    End Class

End Namespace
