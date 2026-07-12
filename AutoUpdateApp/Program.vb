Option Strict On
Option Explicit On

Imports System.Threading
Imports System.Windows.Forms

''' <summary>
''' Application entry point.
''' 
''' Startup flow:
''' 1. Single-instance check via Mutex
''' 2. Check updateflag.txt for pending restart update
''' 3. If flag is True AND versions differ → run installer → clear flag
''' 4. Launch MainForm (system tray app with scheduler)
''' </summary>
Module Program

    Private Const MutexName As String = "Global\AutoUpdateApp_SingleInstance"

    Sub Main()
        ' ── Single-instance check ──
        Dim createdNew As Boolean
        Using mutex As New Mutex(True, MutexName, createdNew)
            If Not createdNew Then
                ' Another instance is already running
                Return
            End If

            Try
                Application.EnableVisualStyles()
                Application.SetCompatibleTextRenderingDefault(False)

                Managers.LogManager.Info("═══════════════════════════════════════")
                Managers.LogManager.Info("Application starting.")
                Managers.LogManager.Info("═══════════════════════════════════════")

                ' ── Startup: Check for pending restart update ──
                CheckPendingRestartUpdate()

                ' ── Launch main form ──
                Application.Run(New Forms.MainForm())

                Managers.LogManager.Info("Application shut down normally.")

            Catch ex As Exception
                Managers.LogManager.[Error]("Fatal error in application.", ex)
                MessageBox.Show("A fatal error occurred. Please check the log file." & _
                                Environment.NewLine & ex.Message, _
                                "Auto Update Error", _
                                MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Checks updateflag.txt at startup.
    ''' If the current computer has a pending restart flag AND versions differ,
    ''' runs the installer and clears the flag.
    ''' </summary>
    Private Sub CheckPendingRestartUpdate()
        Try
            Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
            Managers.LogManager.Info("Startup restart check for: " & computerName)

            ' Look up tester config
            Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
            If tester Is Nothing Then
                Managers.LogManager.Info("Computer not in tester config. Skipping restart check.")
                Return
            End If

            ' Check the flag
            Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
            If Not flag.HasValue OrElse Not flag.Value Then
                Managers.LogManager.Info("No pending restart update.")
                Return
            End If

            ' Verify versions still differ
            Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
            Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()

            If String.IsNullOrEmpty(currentVersion) OrElse String.IsNullOrEmpty(latestVersion) Then
                Managers.LogManager.Warn("Cannot verify versions. Skipping restart update.")
                Return
            End If

            If String.Equals(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase) Then
                ' Already updated — clear the flag
                Managers.LogManager.Info("Versions match. Clearing stale restart flag.")
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                Return
            End If

            ' Run the installer
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
