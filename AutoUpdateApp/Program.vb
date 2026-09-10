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

                RemoveProductFromAllUsersStartup()

                EnsureTargetAppRunning()

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

        Dim serverHost As String = ExtractServerHost(Config.AppSettings.LoadStatus)
        If Not String.IsNullOrEmpty(serverHost) Then
            Managers.LogManager.Info("Server host detected: " & serverHost)
        End If

        Dim networkSignal As New ManualResetEvent(False)
        Dim networkHandler As NetworkAvailabilityChangedEventHandler = _
            Sub(s, ev)
                If ev.IsAvailable Then
                    networkSignal.Set()
                End If
            End Sub

        AddHandler NetworkChange.NetworkAvailabilityChanged, networkHandler

        Try
            Do While DateTime.Now < deadline
                If Not IsNetworkAvailable() Then
                    networkSignal.WaitOne(1000)
                    Continue Do
                End If

            If Not String.IsNullOrEmpty(serverHost) Then
                If Not PingHost(serverHost) Then
                    Dim remaining As Integer = Math.Max(0, CInt((deadline - DateTime.Now).TotalSeconds))
                    Managers.LogManager.Info("Ping " & serverHost & " failed. " & remaining.ToString() & "s remaining.")
                    Thread.Sleep(2000)
                    Continue Do
                End If
                Managers.LogManager.Info("Ping " & serverHost & " OK. Attempting config load...")
            End If

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

            Dim secs As Integer = Math.Max(0, CInt((deadline - DateTime.Now).TotalSeconds))
            Managers.LogManager.Info("Config load failed. " & secs.ToString() & "s remaining. Status: " & Config.AppSettings.LoadStatus)

            Thread.Sleep(RetryIntervalMs)
            Loop

            Return False
        Finally
            RemoveHandler NetworkChange.NetworkAvailabilityChanged, networkHandler
            networkSignal.Dispose()
        End Try
    End Function

    Private Function PingHost(host As String) As Boolean
        Try
            Using pinger As New Ping()
                Dim reply As PingReply = pinger.Send(host, 1500)
                Return reply.Status = IPStatus.Success
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function ExtractServerHost(loadStatus As String) As String
        Try
            Dim configPath As String = Config.AppSettings.LoadedConfigPath
            If String.IsNullOrEmpty(configPath) Then
                If Not String.IsNullOrEmpty(loadStatus) Then
                    Dim uncIdx As Integer = loadStatus.IndexOf("\\")
                    If uncIdx >= 0 Then
                        Dim pathPart As String = loadStatus.Substring(uncIdx)
                        Dim parts As String() = pathPart.Split(New Char() {"\"c}, StringSplitOptions.RemoveEmptyEntries)
                        If parts.Length >= 1 Then Return parts(0)
                    End If
                End If
                Return ""
            End If

            If configPath.StartsWith("\\") Then
                Dim parts As String() = configPath.Substring(2).Split(New Char() {"\"c}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length >= 1 Then Return parts(0)
            End If
        Catch
        End Try
        Return ""
    End Function

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

    Private Sub RemoveProductFromAllUsersStartup()
        Try
            Dim productName As String = Config.AppSettings.UninstallProductName
            If String.IsNullOrEmpty(productName) Then Return

            Dim allUsersStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
            If String.IsNullOrEmpty(allUsersStartup) OrElse Not IO.Directory.Exists(allUsersStartup) Then Return

            Dim removed As Boolean = False
            For Each filePath As String In IO.Directory.GetFiles(allUsersStartup)
                Dim fileName As String = IO.Path.GetFileNameWithoutExtension(filePath).ToUpperInvariant()
                Dim ext As String = IO.Path.GetExtension(filePath).ToUpperInvariant()
                If (ext = ".LNK" OrElse ext = ".EXE") AndAlso fileName.Contains(productName.ToUpperInvariant()) Then
                    Try
                        IO.File.Delete(filePath)
                        Managers.LogManager.Info("Removed from All Users startup: " & filePath)
                        removed = True
                    Catch exDel As Exception
                        Managers.LogManager.Warn("Failed to remove from startup: " & filePath & " - " & exDel.Message)
                    End Try
                End If
            Next

            If Not removed Then
                Managers.LogManager.Info("No " & productName & " entries found in All Users startup.")
            End If
        Catch ex As Exception
            Managers.LogManager.Warn("Error cleaning All Users startup: " & ex.Message)
        End Try
    End Sub

    Private Sub EnsureTargetAppRunning()
        Try
            Dim processNames As String() = {"SX5000MANAGEMENT", "RHYTHMSECTION"}
            Dim found As Boolean = False

            For Each proc As Process In Process.GetProcesses()
                Try
                    Dim name As String = proc.ProcessName.ToUpperInvariant()
                    For Each target As String In processNames
                        If name.Contains(target) Then
                            found = True
                            Managers.LogManager.Info("Target app already running: " & proc.ProcessName)
                            Exit For
                        End If
                    Next
                    If found Then Exit For
                Catch
                End Try
            Next

            If Not found Then
                Dim targetExe As String = "C:\RSX-5000\bin\RSX 5000 IC Syste Management.exe"
                If IO.File.Exists(targetExe) Then
                    Managers.LogManager.Info("Target app not running. Launching: " & targetExe)
                    Dim psi As New ProcessStartInfo(targetExe)
                    psi.WorkingDirectory = IO.Path.GetDirectoryName(targetExe)
                    psi.UseShellExecute = True
                    Process.Start(psi)
                Else
                    Managers.LogManager.Warn("Target exe not found: " & targetExe)
                End If
            End If
        Catch ex As Exception
            Managers.LogManager.Warn("Error checking/launching target app: " & ex.Message)
        End Try
    End Sub

End Module
