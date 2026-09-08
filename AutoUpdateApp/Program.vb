Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Threading
Imports System.Windows.Forms

Module Program

    Private Const MutexName As String = "Local\AutoUpdateApp_SingleInstance"
    Private Const ConnectionTimeoutSeconds As Integer = 30
    Private Const RetryIntervalMs As Integer = 3000
    Private Const MaxRestartAttempts As Integer = 20

    Sub Main()
        Try
            Dim restartCount As Integer = GetRestartCount()

            Dim createdNew As Boolean
            Using mutex As New Mutex(True, MutexName, createdNew)
                If Not createdNew Then
                    Return
                End If

                Application.EnableVisualStyles()
                Application.SetCompatibleTextRenderingDefault(False)

                Managers.LogManager.Info("===================================")
                Managers.LogManager.Info("Application starting. (restart #" & restartCount.ToString() & ")")
                Managers.LogManager.Info("Exe directory: " & AppDomain.CurrentDomain.BaseDirectory)

                If Not WaitForConfig(ConnectionTimeoutSeconds) Then
                    If Config.AppSettings.IsLocalConfigError Then
                        Dim msg As String = "Config error: " & Config.AppSettings.LoadStatus & Environment.NewLine & _
                                            "Please check serverconfig.txt"
                        Managers.LogManager.[Error](msg)
                        MessageBox.Show(msg, "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    If restartCount >= MaxRestartAttempts Then
                        Dim msg As String = "Failed to connect after " & MaxRestartAttempts.ToString() & " restart attempts." & _
                                            Environment.NewLine & "Last status: " & Config.AppSettings.LoadStatus
                        Managers.LogManager.[Error](msg)
                        MessageBox.Show(msg, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    Managers.LogManager.Info("Config not loaded after " & ConnectionTimeoutSeconds.ToString() & "s. Scheduling restart #" & (restartCount + 1).ToString())
                    RestartSelf(restartCount + 1)
                    Return
                End If

                Managers.LogManager.Info("Config loaded successfully.")

                For Each issue As String In Config.AppSettings.ValidateConfig()
                    Managers.LogManager.Info(issue)
                Next

                Managers.LogManager.Info("===================================")

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

    Private Function WaitForConfig(timeoutSeconds As Integer) As Boolean
        Dim deadline As DateTime = DateTime.Now.AddSeconds(timeoutSeconds)

        Config.AppSettings.Reload()
        If Config.AppSettings.IsLoaded Then Return True
        If Config.AppSettings.IsLocalConfigError Then Return False

        If Managers.LogManager.LogContains("CONFIG_LOADED:") Then Return True

        Managers.LogManager.Info("Config not loaded. Waiting for network/server (timeout: " & timeoutSeconds.ToString() & "s)...")

        Do While DateTime.Now < deadline
            WaitForNetwork(3000)

            Config.AppSettings.Reload()
            If Config.AppSettings.IsLoaded Then
                Managers.LogManager.Info("Config loaded after retry.")
                Return True
            End If
            If Config.AppSettings.IsLocalConfigError Then Return False

            If Managers.LogManager.LogContains("CONFIG_LOADED:") Then
                Managers.LogManager.Info("Config confirmed loaded via log check.")
                Return True
            End If

            Dim remaining As Integer = CInt((deadline - DateTime.Now).TotalSeconds)
            Managers.LogManager.Info("Retry... " & remaining.ToString() & "s remaining. Status: " & Config.AppSettings.LoadStatus)

            Thread.Sleep(RetryIntervalMs)
        Loop

        Return False
    End Function

    Private Sub WaitForNetwork(maxWaitMs As Integer)
        Dim waited As Integer = 0
        Do While waited < maxWaitMs
            If IsNetworkAvailable() Then Return
            Thread.Sleep(500)
            waited += 500
        Loop
    End Sub

    Private Function IsNetworkAvailable() As Boolean
        Try
            For Each ni As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
                If ni.OperationalStatus = OperationalStatus.Up AndAlso _
                   (ni.NetworkInterfaceType = NetworkInterfaceType.Ethernet OrElse _
                    ni.NetworkInterfaceType = NetworkInterfaceType.Wireless80211) Then

                    For Each addr In ni.GetIPProperties().UnicastAddresses
                        If addr.Address.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                            Dim ip As String = addr.Address.ToString()
                            If Not ip.StartsWith("169.254.") AndAlso ip <> "127.0.0.1" AndAlso ip <> "0.0.0.0" Then
                                Return True
                            End If
                        End If
                    Next
                End If
            Next
        Catch
        End Try
        Return False
    End Function

    Private Function GetRestartCount() As Integer
        Try
            Dim args As String() = Environment.GetCommandLineArgs()
            For Each arg As String In args
                If arg.StartsWith("/restart:", StringComparison.OrdinalIgnoreCase) Then
                    Dim countStr As String = arg.Substring("/restart:".Length)
                    Dim count As Integer
                    If Integer.TryParse(countStr, count) Then
                        Return count
                    End If
                End If
            Next
        Catch
        End Try
        Return 0
    End Function

    Private Sub RestartSelf(newCount As Integer)
        Try
            Thread.Sleep(2000)

            Dim selfExe As String = System.Reflection.Assembly.GetExecutingAssembly().Location
            Dim psi As New ProcessStartInfo(selfExe, "/restart:" & newCount.ToString())
            psi.UseShellExecute = True
            Process.Start(psi)
            Managers.LogManager.Info("Restart process launched.")
        Catch ex As Exception
            Managers.LogManager.[Error]("Failed to restart: " & ex.Message)
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
                                     currentVersion & " -> " & latestVersion)

            Managers.InstallerManager.CloseProgramOfRegistryPath()

            Using updateForm As New Forms.UpdatingForm()
                updateForm.TesterType = tester.TesterType
                updateForm.ShowDialog()

                If updateForm.UpdateSuccess Then
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
            End Using

        Catch ex As Exception
            Managers.LogManager.[Error]("Error during startup restart check.", ex)
        End Try
    End Sub

End Module
