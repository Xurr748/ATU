Option Strict On
Option Explicit On

Imports System.Threading
Imports System.Windows.Forms

Module Program

    Private Const MutexName As String = "Local\AutoUpdateApp_SingleInstance"

    Sub Main()
        Try
            Dim createdNew As Boolean
            Using mutex As New Mutex(True, MutexName, createdNew)
                If Not createdNew Then
                    Return
                End If

                Application.EnableVisualStyles()
                Application.SetCompatibleTextRenderingDefault(False)

                Managers.LogManager.Info("═══════════════════════════════════════")
                Managers.LogManager.Info("Application starting.")
                Managers.LogManager.Info("Exe directory: " & AppDomain.CurrentDomain.BaseDirectory)

                If Not Config.AppSettings.IsLoaded Then
                    Dim msg As String = "ไม่สามารถโหลด config.txt ได้!" & Environment.NewLine & _
                                        Config.AppSettings.LoadStatus & Environment.NewLine & Environment.NewLine & _
                                        "กรุณาตรวจสอบว่า:" & Environment.NewLine & _
                                        "1) config.txt อยู่ข้างๆ exe  หรือ" & Environment.NewLine & _
                                        "2) ตั้ง ConfigFilePath ใน AutoUpdateApp.exe.config"
                    Managers.LogManager.[Error](msg)
                    MessageBox.Show(msg, "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If

                For Each issue As String In Config.AppSettings.ValidateConfig()
                    Managers.LogManager.Info(issue)
                Next

                Managers.LogManager.Info("═══════════════════════════════════════")

                Managers.InstallerManager.AddSelfToStartup()

                Managers.InstallerManager.CopyShortcutToStartup()

                CheckPendingRestartUpdate()

                Application.Run(New Forms.MainForm())

                Managers.LogManager.Info("Application shut down normally.")
                mutex.ReleaseMutex()
            End Using

        Catch ex As Exception
            Managers.LogManager.[Error]("Fatal error in application.", ex)
            MessageBox.Show("A fatal error occurred. Please check the log file." & _
                            Environment.NewLine & ex.Message, _
                            "Auto Update Error", _
                            MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub CheckPendingRestartUpdate()
        Try
            Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
            Managers.LogManager.Info("Startup restart check for: " & computerName)

            Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
            If tester Is Nothing Then
                Managers.LogManager.Info("Computer not in tester config. Skipping restart check.")
                Return
            End If

            Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
            If Not flag.HasValue OrElse Not flag.Value Then
                Managers.LogManager.Info("No pending restart update.")
                Return
            End If

            Managers.LogManager.Info("Pending restart flag detected. Starting update sequence.")

            Managers.InstallerManager.CloseProgramOfRegistryPath()

            Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
            Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()

            If String.IsNullOrEmpty(currentVersion) OrElse String.IsNullOrEmpty(latestVersion) Then
                Managers.LogManager.Warn("Cannot verify versions. Skipping restart update.")
                Return
            End If

            If String.Equals(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase) Then
                Managers.LogManager.Info("Versions match. Clearing stale restart flag.")
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                Return
            End If

            Managers.LogManager.Info("Running pending restart update. " & _
                                     currentVersion & " → " & latestVersion)

            Dim updateForm As New Forms.UpdatingForm()
            updateForm.TesterType = tester.TesterType
            
            ' เปิดหน้าต่างนี้ขึ้นมาค้างไว้ มันจะรันอัปเดตเบื้องหลังแล้วปิดตัวเองเมื่อเสร็จ
            updateForm.ShowDialog()

            Dim success As Boolean = updateForm.UpdateSuccess

            If success Then
                Dim verified As Boolean = Managers.InstallerManager.VerifyInstallation()

                If verified Then
                    Managers.InstallerManager.StartProgramOfRegistryPath()

                    Managers.InstallerManager.CopyShortcutToStartup()

                    Managers.UpdateFlagManager.SetFlag(computerName, False)
                    Managers.LogManager.Info("Restart update completed and verified successfully.")
                Else
                    Managers.LogManager.Warn("Install script ran but version not yet updated. Flag remains for retry.")
                End If
            Else
                Managers.LogManager.[Error]("Restart update failed. Flag will remain for retry.")
            End If

        Catch ex As Exception
            Managers.LogManager.[Error]("Error during startup restart check.", ex)
        End Try
    End Sub

End Module
