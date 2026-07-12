Option Strict On
Option Explicit On

Imports System.Threading
Imports System.Windows.Forms

''' <summary>
''' จุดเริ่มต้นของแอปพลิเคชัน
''' 
''' ขั้นตอนเริ่มต้น:
''' 1. ตรวจสอบว่าเปิดโปรแกรมซ้ำหรือไม่ ด้วย Mutex
''' 2. ตรวจสอบ updateflag.txt ว่ามีอัปเดตค้างรอรีสตาร์ทหรือไม่
''' 3. ถ้า Flag เป็น True และเวอร์ชันไม่ตรง → รัน Installer → ล้าง Flag
''' 4. เปิด MainForm (System Tray)
''' </summary>
Module Program

    Private Const MutexName As String = "Local\AutoUpdateApp_SingleInstance"

    Sub Main()
        Try
            ' ── ตรวจสอบว่ามีโปรแกรมเปิดอยู่แล้วหรือไม่ ──
            Dim createdNew As Boolean
            Using mutex As New Mutex(True, MutexName, createdNew)
                If Not createdNew Then
                    ' มีโปรแกรมเปิดอยู่แล้ว ไม่เปิดซ้ำ
                    Return
                End If

                Application.EnableVisualStyles()
                Application.SetCompatibleTextRenderingDefault(False)

                Managers.LogManager.Info("═══════════════════════════════════════")
                Managers.LogManager.Info("Application starting.")
                Managers.LogManager.Info("═══════════════════════════════════════")

                ' ── ตรวจสอบการอัปเดตที่ค้างรอรีสตาร์ทตอนเริ่มโปรแกรม ──
                CheckPendingRestartUpdate()

                ' ── เปิดหน้าจอหลัก ──
                Application.Run(New Forms.MainForm())

                Managers.LogManager.Info("Application shut down normally.")
            End Using

        Catch ex As Exception
            Managers.LogManager.[Error]("Fatal error in application.", ex)
            MessageBox.Show("A fatal error occurred. Please check the log file." & _
                            Environment.NewLine & ex.Message, _
                            "Auto Update Error", _
                            MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    ''' <summary>
    ''' ตรวจสอบ updateflag.txt ตอนเริ่มโปรแกรม
    ''' If the current computer has a pending restart flag AND versions differ,
    ''' runs the installer and clears the flag.
    ''' </summary>
    Private Sub CheckPendingRestartUpdate()
        Try
            Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
            Managers.LogManager.Info("Startup restart check for: " & computerName)

            ' ค้นหาข้อมูลเครื่องทดสอบ
            Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
            If tester Is Nothing Then
                Managers.LogManager.Info("Computer not in tester config. Skipping restart check.")
                Return
            End If

            ' ตรวจสอบ Flag
            Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
            If Not flag.HasValue OrElse Not flag.Value Then
                Managers.LogManager.Info("No pending restart update.")
                Return
            End If

            ' ตรวจสอบว่าเวอร์ชันยังไม่ตรงกัน
            Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
            Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()

            If String.IsNullOrEmpty(currentVersion) OrElse String.IsNullOrEmpty(latestVersion) Then
                Managers.LogManager.Warn("Cannot verify versions. Skipping restart update.")
                Return
            End If

            If String.Equals(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase) Then
                ' อัปเดตแล้ว — ล้าง Flag เก่าทิ้ง
                Managers.LogManager.Info("Versions match. Clearing stale restart flag.")
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                Return
            End If

            ' เรียกใช้ Installer
            Managers.LogManager.Info("Running pending restart update. " & _
                                     currentVersion & " → " & latestVersion)

            Dim success As Boolean = Managers.InstallerManager.RunInstaller(tester.TesterType)

            If success Then
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                Managers.LogManager.Info("Restart update completed successfully.")
            Else
                Managers.LogManager.[Error]("Restart update failed. Flag will remain for retry.")
            End If

        Catch ex As Exception
            Managers.LogManager.[Error]("Error during startup restart check.", ex)
        End Try
    End Sub

End Module
