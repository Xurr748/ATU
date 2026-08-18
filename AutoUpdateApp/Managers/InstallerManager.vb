Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Managers

    Public NotInheritable Class InstallerManager

        Private Sub New()
        End Sub

        Public Shared Function GetInstallerPath(testerType As String) As String
            Select Case testerType.ToUpperInvariant()
                Case "HE"
                    Return Config.AppSettings.InstallerPathHE
                Case "LLE"
                    Return Config.AppSettings.InstallerPathLLE
                Case Else
                    LogManager.Warn("Unknown tester type: " & testerType)
                    Return String.Empty
            End Select
        End Function

        Public Shared Function RunInstaller(testerType As String, Optional progressCallback As Action(Of Integer, String) = Nothing) As Boolean
            Dim installerFolder As String = GetInstallerPath(testerType)

            If String.IsNullOrEmpty(installerFolder) Then
                LogManager.[Error]("Installer path is empty for type: " & testerType)
                Return False
            End If

            If Not Directory.Exists(installerFolder) Then
                LogManager.[Error]("Installer folder not found on server: " & installerFolder)
                Return False
            End If

            LogManager.LogIPAddress()

            Dim configLocalPath As String = Config.AppSettings.LocalInstallerPath
            Dim localFolder As String
            If Not String.IsNullOrEmpty(configLocalPath) Then
                localFolder = configLocalPath
            Else
                localFolder = Path.Combine(Path.GetTempPath(), "AutoUpdateApp_LocalInstaller")
            End If
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Dim result As Boolean = False

            Try
                LogManager.Info(String.Format("Downloading installer folder: {0} -> {1}", installerFolder, localFolder))

                If progressCallback IsNot Nothing Then
                    progressCallback(0, L("ProgressDownloading"))
                End If

                Try
                    If Directory.Exists(localFolder) Then
                        Directory.Delete(localFolder, True)
                    End If
                Catch ex As Exception
                    LogManager.Warn("Could not clear existing local installer folder: " & ex.Message)
                End Try

                Dim copySuccess As Boolean = True
                Try
                    CopyDirectoryWithProgress(installerFolder, localFolder, progressCallback)
                Catch ex As Exception
                    LogManager.[Error]("Failed to copy installer files from server: " & installerFolder, ex)
                    If progressCallback IsNot Nothing Then
                        progressCallback(0, L("ProgressFailed"))
                    End If
                    copySuccess = False
                End Try

                If copySuccess Then
                    KillTargetProcess()
                    CloseProgramOfRegistryPath()
                    Dim uninstallPath As String = IO.Path.Combine(localFolder, "uninstall.bat")
                    Dim installPath As String = IO.Path.Combine(localFolder, "install.bat")

                    Dim uninstallSuccess As Boolean = True
                    Dim productName As String = Config.AppSettings.UninstallProductName

                    If Not String.IsNullOrEmpty(productName) Then
                        If progressCallback IsNot Nothing Then
                            progressCallback(85, L("ProgressSearching"))
                        End If
                        Dim guid As String = FindUninstallGuid(productName)

                        If String.IsNullOrEmpty(guid) Then
                            LogManager.Warn("ไม่พบโปรแกรม '" & productName & "' ใน Registry (ข้ามขั้นตอน Uninstall)")
                        Else
                            LogManager.Info("พบ GUID: " & guid & " สำหรับ '" & productName & "'")
                            If progressCallback IsNot Nothing Then
                                progressCallback(90, String.Format(L("ProgressUninstallingProduct"), productName))
                            End If

                            Dim smartBatPath As String = IO.Path.Combine(localFolder, "uninstall.bat")
                            Dim batContent As String = "@echo off" & Environment.NewLine &
                                                       "msiexec.exe /x " & guid & " /quiet /norestart" & Environment.NewLine &
                                                       "exit /b %ERRORLEVEL%"
                            IO.File.WriteAllText(smartBatPath, batContent)
                            LogManager.Info("สร้าง uninstall.bat: msiexec /x " & guid & " /quiet /norestart")

                            If Not RunBatchFile(smartBatPath, "uninstall") Then
                                LogManager.[Error]("Uninstall process failed for GUID: " & guid)
                                uninstallSuccess = False
                            End If
                        End If
                    ElseIf Utilities.FileHelper.FileExistsSafe(uninstallPath) Then
                        If progressCallback IsNot Nothing Then
                            progressCallback(90, L("ProgressUninstalling"))
                        End If
                        If Not RunBatchFile(uninstallPath, "uninstall") Then
                            LogManager.[Error]("Uninstall process failed.")
                            uninstallSuccess = False
                        End If
                    Else
                        LogManager.Warn("Uninstall script not found and UninstallProductName not set. (Skipping uninstall step)")
                    End If

                    If uninstallSuccess Then
                        Dim msiFile As String = FindLatestMsi(localFolder)
                        Dim installerArgs As String = Config.AppSettings.InstallerArgs

                        If Not String.IsNullOrEmpty(msiFile) Then
                            LogManager.Info("พบ MSI: " & msiFile)
                            If progressCallback IsNot Nothing Then
                                progressCallback(95, String.Format(L("ProgressInstallingProduct"), IO.Path.GetFileName(msiFile)))
                            End If

                            Dim smartInstallPath As String = IO.Path.Combine(localFolder, "install.bat")
                            Dim batContent As String = "@echo off" & Environment.NewLine &
                                                       "msiexec.exe /i """ & msiFile & """ " & installerArgs & Environment.NewLine &
                                                       "exit /b %ERRORLEVEL%"
                            IO.File.WriteAllText(smartInstallPath, batContent)
                            LogManager.Info("สร้าง install.bat: msiexec /i """ & msiFile & """ " & installerArgs)

                            If Not RunBatchFile(smartInstallPath, "install") Then
                                LogManager.[Error]("Install process failed.")
                            Else
                                If progressCallback IsNot Nothing Then
                                    progressCallback(100, L("ProgressComplete"))
                                End If
                                result = True
                            End If
                        ElseIf Utilities.FileHelper.FileExistsSafe(installPath) Then
                            If progressCallback IsNot Nothing Then
                                progressCallback(95, L("ProgressInstalling"))
                            End If
                            If Not RunBatchFile(installPath, "install") Then
                                LogManager.[Error]("Install process failed.")
                            Else
                                If progressCallback IsNot Nothing Then
                                    progressCallback(100, L("ProgressComplete"))
                                End If
                                result = True
                            End If
                        Else
                            LogManager.[Error]("ไม่พบไฟล์ .msi ในโฟลเดอร์ " & installerFolder & " และไม่มี install.bat")
                        End If
                    End If
                End If
            Catch ex As Exception
                LogManager.[Error]("Error during installation process.", ex)
                result = False
            Finally
                Try
                    If Directory.Exists(localFolder) Then
                        Directory.Delete(localFolder, True)
                    End If
                Catch cleanupEx As Exception
                    LogManager.Warn("Could not clean up temp installer folder: " & cleanupEx.Message)
                End Try
            End Try

            If result Then
                CopyConfigFiles()
                LaunchTargetAppWithAutoConfirm()
            End If

            Return result
        End Function

        Public Shared Sub LaunchTargetApp()
            Try
                Dim appPath As String = Config.AppSettings.TargetAppExePath

                If String.IsNullOrEmpty(appPath) Then
                    appPath = Utilities.RegistryHelper.ReadValue(
                        Config.AppSettings.RegistryKeyPath, Config.AppSettings.RegistryPathValueName)
                End If

                If String.IsNullOrEmpty(appPath) Then
                    LogManager.Warn("TargetAppExePath is empty and could not find path from Registry. Skipping app launch.")
                    Return
                End If

                If IO.Directory.Exists(appPath) Then
                    Dim exeFiles = IO.Directory.GetFiles(appPath, "*.exe")
                    If exeFiles.Length > 0 Then
                        appPath = exeFiles(0)
                    Else
                        LogManager.Warn("No .exe found in directory: " & appPath)
                        Return
                    End If
                End If

                If Not IO.File.Exists(appPath) Then
                    LogManager.Warn("Target app not found: " & appPath & ". Skipping app launch.")
                    Return
                End If

                LogManager.Info("Launching target app: " & appPath)
                Process.Start(appPath)
            Catch ex As Exception
                LogManager.Warn("Failed to launch target app: " & ex.Message)
            End Try
        End Sub

        Private Shared Function FindLatestMsi(folderPath As String) As String
            Try
                If String.IsNullOrEmpty(folderPath) OrElse Not Directory.Exists(folderPath) Then
                    Return Nothing
                End If

                Dim msiFiles = New IO.DirectoryInfo(folderPath).GetFiles("*.msi")
                If msiFiles.Length = 0 Then
                    Return Nothing
                End If

                Dim latest As IO.FileInfo = msiFiles(0)
                For Each f In msiFiles
                    If f.LastWriteTime > latest.LastWriteTime Then
                        latest = f
                    End If
                Next

                Return latest.FullName
            Catch ex As Exception
                LogManager.Warn("Error searching for .msi files: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Private Shared Function FindUninstallGuid(productName As String) As String
            Dim registryPaths As String() = {
                "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                "SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            }

            For Each regPath As String In registryPaths
                Try
                    Using baseKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath)
                        If baseKey Is Nothing Then Continue For

                        For Each subKeyName As String In baseKey.GetSubKeyNames()
                            Try
                                Using subKey As Microsoft.Win32.RegistryKey = baseKey.OpenSubKey(subKeyName)
                                    If subKey Is Nothing Then Continue For

                                    Dim displayName As Object = subKey.GetValue("DisplayName")
                                    If displayName IsNot Nothing Then
                                        Dim name As String = displayName.ToString()
                                        If name.IndexOf(productName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                                            LogManager.Info("Registry match: " & name & " → " & subKeyName & " (in " & regPath & ")")
                                            Return subKeyName
                                        End If
                                    End If
                                End Using
                            Catch
                            End Try
                        Next
                    End Using
                Catch ex As Exception
                    LogManager.Warn("Error reading registry path: " & regPath & " - " & ex.Message)
                End Try
            Next

            Return Nothing
        End Function

        Private Shared Function RunBatchFile(batchPath As String, stepName As String) As Boolean
            Try
                LogManager.Info(String.Format("Starting {0} script: {1}", stepName, batchPath))

                Dim psi As New ProcessStartInfo()
                psi.FileName = batchPath
                psi.Arguments = ""
                psi.UseShellExecute = True
                psi.WindowStyle = ProcessWindowStyle.Hidden
                psi.WorkingDirectory = IO.Path.GetDirectoryName(batchPath)

                Using proc As Process = Process.Start(psi)
                    If proc IsNot Nothing Then
                        proc.WaitForExit(1800000)
                        If Not proc.HasExited Then
                            LogManager.Warn(stepName & " script timed out after 30 minutes.")
                            Return False
                        End If
                        Dim exitCode As Integer = proc.ExitCode
                        LogManager.Info(stepName & " script exited with code: " & exitCode.ToString())
                        Return (exitCode = 0)
                    End If
                End Using

                Return False
            Catch ex As Exception
                LogManager.[Error]("Failed to run " & stepName & " script: " & batchPath, ex)
                Return False
            End Try
        End Function

        Private Shared Sub CopyDirectoryWithProgress(sourceDir As String, destDir As String, progressCallback As Action(Of Integer, String))
            Dim sourceDirInfo As New DirectoryInfo(sourceDir)
            If Not sourceDirInfo.Exists Then
                Throw New DirectoryNotFoundException("Source directory not found: " & sourceDir)
            End If

            Dim allFiles As New List(Of FileInfo)()
            GetAllFilesRecursive(sourceDirInfo, allFiles)

            Dim totalBytes As Long = 0
            For Each f In allFiles
                totalBytes += f.Length
            Next

            Dim fileCount As Integer = allFiles.Count

            If Not Directory.Exists(destDir) Then
                Directory.CreateDirectory(destDir)
            End If

            LogManager.Info(String.Format("เริ่มการดาวน์โหลด/คัดลอกไฟล์จากเซิร์ฟเวอร์: พบทั้งหมด {0} ไฟล์ (ขนาดรวม {1} ไบต์)", fileCount, totalBytes))

            Dim copiedBytes As Long = 0
            Dim currentFileIndex As Integer = 0

            For Each file In allFiles
                Dim relativePath As String = file.FullName.Substring(sourceDirInfo.FullName.Length)
                If relativePath.StartsWith("\") OrElse relativePath.StartsWith("/") Then
                    relativePath = relativePath.Substring(1)
                End If
                Dim destFilePath As String = Path.Combine(destDir, relativePath)

                Dim destSubDir As String = Path.GetDirectoryName(destFilePath)
                If Not Directory.Exists(destSubDir) Then
                    Directory.CreateDirectory(destSubDir)
                End If

                Dim buffer(65536 - 1) As Byte
                Using sourceStream As New FileStream(file.FullName, FileMode.Open, FileAccess.Read)
                    Using destStream As New FileStream(destFilePath, FileMode.Create, FileAccess.Write)
                        Dim bytesRead As Integer = sourceStream.Read(buffer, 0, buffer.Length)
                        While bytesRead > 0
                            destStream.Write(buffer, 0, bytesRead)
                            copiedBytes += bytesRead

                            Dim percent As Integer = 0
                            If totalBytes > 0 Then
                                percent = CInt((copiedBytes * 100) \ totalBytes)
                            End If
                            If percent > 100 Then percent = 100

                            If progressCallback IsNot Nothing Then
                                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                                Dim statusMsg As String = String.Format(L("ProgressDownloadingFile"), file.Name, currentFileIndex + 1, fileCount)
                                progressCallback(percent, statusMsg)
                            End If

                            bytesRead = sourceStream.Read(buffer, 0, buffer.Length)
                        End While
                    End Using
                End Using


                currentFileIndex += 1
            Next

            LogManager.Info(String.Format("ดาวน์โหลด/คัดลอกโฟลเดอร์ตัวติดตั้งเสร็จสิ้น รวมทั้งหมด {0} ไฟล์ ไปยัง {1}", fileCount, destDir))
        End Sub

        Private Shared Sub GetAllFilesRecursive(dir As DirectoryInfo, fileList As List(Of FileInfo))
            fileList.AddRange(dir.GetFiles())
            For Each subdir In dir.GetDirectories()
                GetAllFilesRecursive(subdir, fileList)
            Next
        End Sub

        Public Shared Sub KillTargetProcess()
            Try
                Dim processNames As New List(Of String)()

                Dim killList As String = Config.AppSettings.KillProcessList
                If Not String.IsNullOrEmpty(killList) Then
                    For Each name As String In killList.Split(","c)
                        Dim trimmed As String = name.Trim().Replace(".exe", "")
                        If Not String.IsNullOrEmpty(trimmed) AndAlso Not processNames.Contains(trimmed) Then
                            processNames.Add(trimmed)
                        End If
                    Next
                End If

                Dim productName As String = Config.AppSettings.UninstallProductName
                If Not String.IsNullOrEmpty(productName) Then
                    Dim pName As String = productName.Replace(".exe", "")
                    If Not processNames.Contains(pName) Then
                        processNames.Add(pName)
                    End If
                End If

                If processNames.Count = 0 Then
                    LogManager.Info("No process names to kill. Skipping.")
                    Return
                End If

                LogManager.Info("Kill list: " & String.Join(", ", processNames.ToArray()))

                For Each processName As String In processNames
                    KillProcessByName(processName)
                Next
            Catch ex As Exception
                LogManager.Warn("Error in KillTargetProcess: " & ex.Message)
            End Try
        End Sub

        Private Shared Sub KillProcessByName(processName As String)
            Try
                Dim processes() As Process = Process.GetProcessesByName(processName)

                If processes.Length = 0 Then
                    LogManager.Info("No running process: " & processName)
                    Return
                End If

                For Each proc As Process In processes
                    Try
                        LogManager.Info("Closing process: " & proc.ProcessName & " (PID: " & proc.Id & ")")

                        proc.CloseMainWindow()

                        If Not proc.WaitForExit(2000) Then
                            LogManager.Info("Force killing (bypass confirmation): " & proc.ProcessName)
                            proc.Kill()
                            proc.WaitForExit(3000)
                        End If

                        LogManager.Info("Process closed: " & proc.ProcessName)
                    Catch ex As Exception
                        LogManager.Warn("Could not kill " & proc.ProcessName & ": " & ex.Message)
                    End Try
                Next

                Threading.Thread.Sleep(500)
                Dim remaining() As Process = Process.GetProcessesByName(processName)
                If remaining.Length > 0 Then
                    LogManager.Warn("Process still running after Kill(). Using taskkill /F /IM as fallback.")
                    Try
                        Dim psi As New ProcessStartInfo()
                        psi.FileName = "taskkill"
                        psi.Arguments = "/F /IM " & processName & ".exe"
                        psi.UseShellExecute = False
                        psi.CreateNoWindow = True
                        Dim p As Process = Process.Start(psi)
                        If p IsNot Nothing Then p.WaitForExit(5000)
                        LogManager.Info("taskkill /F /IM " & processName & ".exe completed.")
                    Catch ex As Exception
                        LogManager.Warn("taskkill fallback failed: " & ex.Message)
                    End Try
                End If
            Catch ex As Exception
                LogManager.Warn("Error killing " & processName & ": " & ex.Message)
            End Try
        End Sub

        Public Shared Sub CloseProgramOfRegistryPath()
            Try
                Dim keyPath As String = Config.AppSettings.RegistryKeyPath
                Dim pathValueName As String = Config.AppSettings.RegistryPathValueName
                Dim targetPath As String = Utilities.RegistryHelper.ReadValue(keyPath, pathValueName)

                If String.IsNullOrEmpty(targetPath) Then
                    LogManager.Warn("Cannot find target program path in registry to close: " & keyPath & "\" & pathValueName)
                    Return
                End If

                targetPath = targetPath.Trim()
                LogManager.Info("Target program path from registry to close: " & targetPath)

                Dim processName As String = ""
                If File.Exists(targetPath) Then
                    processName = Path.GetFileNameWithoutExtension(targetPath)
                End If

                For Each proc As Process In Process.GetProcesses()
                    Try
                        Dim isTarget As Boolean = False
                        If Not String.IsNullOrEmpty(processName) AndAlso String.Equals(proc.ProcessName, processName, StringComparison.OrdinalIgnoreCase) Then
                            isTarget = True
                        Else
                            Dim mainModulePath As String = proc.MainModule.FileName
                            If mainModulePath.StartsWith(targetPath, StringComparison.OrdinalIgnoreCase) Then
                                isTarget = True
                            End If
                        End If

                        If isTarget Then
                            LogManager.Info("Closing target process: " & proc.ProcessName & " (PID: " & proc.Id & ")")
                            proc.CloseMainWindow()
                            If Not proc.WaitForExit(5000) Then
                                LogManager.Warn("Process did not exit, force killing: " & proc.ProcessName)
                                proc.Kill()
                            End If
                        End If
                    Catch ex As Exception
                    End Try
                Next
            Catch ex As Exception
                LogManager.Error("Error closing target program of registry path.", ex)
            End Try
        End Sub

        Public Shared Sub StartProgramOfRegistryPath()
            Try
                Dim keyPath As String = Config.AppSettings.RegistryKeyPath
                Dim pathValueName As String = Config.AppSettings.RegistryPathValueName
                Dim targetPath As String = Utilities.RegistryHelper.ReadValue(keyPath, pathValueName)

                If String.IsNullOrEmpty(targetPath) Then
                    LogManager.Warn("Cannot find target program path in registry to start: " & keyPath & "\" & pathValueName)
                    Return
                End If

                targetPath = targetPath.Trim()
                If File.Exists(targetPath) Then
                    LogManager.Info("Starting target program: " & targetPath)
                    Process.Start(targetPath)
                ElseIf Directory.Exists(targetPath) Then
                    Dim exes As String() = Directory.GetFiles(targetPath, "*.exe")
                    If exes.Length > 0 Then
                        LogManager.Info("Starting target program exe from folder: " & exes(0))
                        Process.Start(exes(0))
                    Else
                        LogManager.Warn("No executable found in directory: " & targetPath)
                    End If
                Else
                    LogManager.Warn("Target program path does not exist: " & targetPath)
                End If
            Catch ex As Exception
                LogManager.Error("Error starting target program of registry path.", ex)
            End Try
        End Sub

        Public Shared Sub CopyShortcutToStartup()
            Try
                If Not Config.AppSettings.EnableTargetStartup Then
                    LogManager.Info("Target startup shortcut is disabled in config.")
                    Return
                End If

                Dim keyPath As String = Config.AppSettings.RegistryKeyPath
                Dim pathValueName As String = Config.AppSettings.RegistryPathValueName
                Dim targetPath As String = Utilities.RegistryHelper.ReadValue(keyPath, pathValueName)

                If String.IsNullOrEmpty(targetPath) Then
                    LogManager.Warn("Cannot find program executable to create shortcut: (empty registry value)")
                    Return
                End If

                targetPath = targetPath.Trim()

                If Directory.Exists(targetPath) AndAlso Not File.Exists(targetPath) Then
                    Dim exes As String() = Directory.GetFiles(targetPath, "*.exe")
                    If exes.Length > 0 Then
                        targetPath = exes(0)
                    Else
                        LogManager.Warn("No executable found in directory: " & targetPath)
                        Return
                    End If
                End If

                If Not File.Exists(targetPath) Then
                    LogManager.Warn("Cannot find program executable to create shortcut: " & targetPath)
                    Return
                End If

                If Config.AppSettings.RemoveOldStartupShortcut Then
                    Dim nameToRemove As String = Config.AppSettings.StartupShortcutName
                    If String.IsNullOrEmpty(nameToRemove) Then
                        nameToRemove = Path.GetFileNameWithoutExtension(targetPath)
                    End If
                    RemoveStartupShortcut(nameToRemove)
                End If

                Dim startupFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                Dim shortcutBaseName As String = Config.AppSettings.StartupShortcutName
                If String.IsNullOrEmpty(shortcutBaseName) Then
                    shortcutBaseName = Path.GetFileNameWithoutExtension(targetPath)
                End If
                Dim shortcutPath As String = Path.Combine(startupFolder, shortcutBaseName & ".lnk")

                LogManager.Info("Creating startup shortcut at: " & shortcutPath)
                CreateShortcut(shortcutPath, targetPath)
                LogManager.Info("Startup shortcut created successfully.")
            Catch ex As Exception
                LogManager.Error("Error creating startup shortcut.", ex)
            End Try
        End Sub

        Public Shared Sub AddSelfToStartup()
            Try
                If Not Config.AppSettings.EnableSelfStartup Then
                    LogManager.Info("Self startup is disabled in config.")
                    Return
                End If

                Dim selfExePath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
                If String.IsNullOrEmpty(selfExePath) OrElse Not File.Exists(selfExePath) Then
                    LogManager.Warn("Cannot determine self executable path for startup.")
                    Return
                End If

                Dim startupFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                Dim shortcutName As String = Path.GetFileNameWithoutExtension(selfExePath) & ".lnk"
                Dim shortcutPath As String = Path.Combine(startupFolder, shortcutName)

                Dim selfName As String = Path.GetFileNameWithoutExtension(selfExePath)
                RemoveStartupShortcut(selfName)

                LogManager.Info("Adding self to startup: " & shortcutPath)
                CreateShortcut(shortcutPath, selfExePath)
                LogManager.Info("Self startup shortcut created successfully.")
            Catch ex As Exception
                LogManager.Error("Error adding self to startup.", ex)
            End Try
        End Sub

        Public Shared Sub RemoveStartupShortcut(shortcutBaseName As String)
            Try
                If String.IsNullOrEmpty(shortcutBaseName) Then Return

                Dim currentUserStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                RemoveShortcutFromFolder(currentUserStartup, shortcutBaseName)

                Dim allUsersStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
                RemoveShortcutFromFolder(allUsersStartup, shortcutBaseName)
            Catch ex As Exception
                LogManager.Warn("Could not remove startup shortcut: " & ex.Message)
            End Try
        End Sub

        Private Shared Sub RemoveShortcutFromFolder(folderPath As String, shortcutBaseName As String)
            Try
                If String.IsNullOrEmpty(folderPath) Then Return

                Dim shortcutPath As String = Path.Combine(folderPath, shortcutBaseName & ".lnk")
                If File.Exists(shortcutPath) Then
                    File.Delete(shortcutPath)
                    LogManager.Info("Removed startup shortcut: " & shortcutPath)
                End If

                Dim exePath As String = Path.Combine(folderPath, shortcutBaseName & ".exe")
                If File.Exists(exePath) Then
                    File.Delete(exePath)
                    LogManager.Info("Removed startup exe: " & exePath)
                End If
            Catch ex As Exception
                LogManager.Warn("Could not remove shortcut from " & folderPath & ": " & ex.Message)
            End Try
        End Sub

        Public Shared Function VerifyInstallation() As Boolean
            Try
                Dim currentVersion As String = VersionManager.ReadRegistryVersion()
                Dim latestVersion As String = VersionManager.ReadLatestVersion()

                If String.IsNullOrEmpty(currentVersion) OrElse String.IsNullOrEmpty(latestVersion) Then
                    LogManager.Warn("Cannot verify installation: version info unavailable. Current=" & currentVersion & " Latest=" & latestVersion)
                    Return False
                End If

                If String.Equals(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase) Then
                    LogManager.Info("Installation verified successfully. Version: " & currentVersion)
                    Return True
                Else
                    LogManager.Warn("Installation verification failed. Registry=" & currentVersion & " Expected=" & latestVersion)
                    Return False
                End If
            Catch ex As Exception
                LogManager.Error("Error during installation verification.", ex)
                Return False
            End Try
        End Function

        Private Shared Sub CreateShortcut(shortcutPath As String, targetExePath As String)
            Dim shellType As Type = Type.GetTypeFromProgID("WScript.Shell")
            Dim shell As Object = Activator.CreateInstance(shellType)
            Dim shortcut As Object = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, Nothing, shell, New Object() {shortcutPath})

            Dim shortcutType As Type = shortcut.GetType()
            shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {targetExePath})
            shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {Path.GetDirectoryName(targetExePath)})
            shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, Nothing, shortcut, Nothing)
        End Sub


        Public Shared Sub CopyConfigFiles()
            Try
                Dim sources As String = Config.AppSettings.CopyFilesSource
                Dim destination As String = Config.AppSettings.CopyFilesDestination

                If String.IsNullOrEmpty(sources) Then
                    LogManager.Info("CopyFilesSource is empty. Skipping post-install file copy.")
                    Return
                End If

                If String.IsNullOrEmpty(destination) Then
                    LogManager.Warn("CopyFilesDestination is empty. Cannot copy files.")
                    Return
                End If

                If Not Directory.Exists(destination) Then
                    Directory.CreateDirectory(destination)
                    LogManager.Info("Created destination directory: " & destination)
                End If

                Dim filePaths As String() = sources.Split("|"c)

                For Each srcPath As String In filePaths
                    Dim trimmedPath As String = srcPath.Trim()
                    If String.IsNullOrEmpty(trimmedPath) Then Continue For

                    Try
                        If IO.File.Exists(trimmedPath) Then
                            Dim fileName As String = Path.GetFileName(trimmedPath)
                            Dim destFile As String = Path.Combine(destination, fileName)
                            IO.File.Copy(trimmedPath, destFile, True)

                        ElseIf Directory.Exists(trimmedPath) Then
                            Dim allFiles = Directory.GetFiles(trimmedPath)
                            For Each f As String In allFiles
                                Dim fileName As String = Path.GetFileName(f)
                                Dim destFile As String = Path.Combine(destination, fileName)
                                IO.File.Copy(f, destFile, True)
                            Next
                        Else
                            LogManager.Warn("Source path not found: " & trimmedPath)
                        End If
                    Catch ex As Exception
                        LogManager.Warn("Failed to copy '" & trimmedPath & "': " & ex.Message)
                    End Try
                Next

                LogManager.Info("Post-install file copy completed.")
            Catch ex As Exception
                LogManager.[Error]("Error in CopyConfigFiles: " & ex.Message)
            End Try
        End Sub


        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Private Shared Function FindWindow(lpClassName As String, lpWindowName As String) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Private Shared Function FindWindowEx(hwndParent As IntPtr, hwndChildAfter As IntPtr, lpszClass As String, lpszWindow As String) As IntPtr
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function SendMessage(hWnd As IntPtr, msg As UInteger, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Private Shared Function EnumChildWindows(hWndParent As IntPtr, lpEnumFunc As EnumChildProc, lParam As IntPtr) As Boolean
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function GetWindowText(hWnd As IntPtr, lpString As System.Text.StringBuilder, nMaxCount As Integer) As Integer
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function GetClassName(hWnd As IntPtr, lpClassName As System.Text.StringBuilder, nMaxCount As Integer) As Integer
        End Function

        Private Delegate Function EnumChildProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

        Private Const BM_CLICK As UInteger = &HF5UI
        Private Const WM_CLOSE As UInteger = &H10UI

        Public Shared Sub LaunchTargetAppWithAutoConfirm()
            Try
                LaunchTargetApp()

                If Not Config.AppSettings.AutoConfirmAfterLaunch Then
                    LogManager.Info("AutoConfirmAfterLaunch is disabled. Skipping auto-confirm.")
                    Return
                End If

                LogManager.Info("AutoConfirmAfterLaunch enabled. Waiting for dialog windows...")

                Dim maxAttempts As Integer = 15
                Dim confirmed As Boolean = False

                For attempt As Integer = 1 To maxAttempts
                    Threading.Thread.Sleep(2000)

                    confirmed = TryClickConfirmButtons()

                    If confirmed Then
                        LogManager.Info("Auto-confirm: Successfully clicked confirm button on attempt " & attempt)
                        Exit For
                    End If
                Next

                If Not confirmed Then
                    LogManager.Info("Auto-confirm: No dialog found after " & maxAttempts & " attempts. (Normal if app doesn't show dialogs)")
                End If
            Catch ex As Exception
                LogManager.Warn("Error in LaunchTargetAppWithAutoConfirm: " & ex.Message)
            End Try
        End Sub

        Private Shared Function TryClickConfirmButtons() As Boolean
            Dim clicked As Boolean = False

            Try
                Dim dialogHandle As IntPtr = FindWindow("#32770", Nothing)

                If dialogHandle = IntPtr.Zero Then Return False

                Dim confirmTexts As String() = {
                    "Yes", "yes", "YES",
                    "OK", "Ok", "ok",
                    "&Yes", "&yes",
                    "ใช่", "ตกลง", "ยืนยัน",
                    "はい", "OK"
                }

                Dim childButtons As New List(Of IntPtr)()
                EnumChildWindows(dialogHandle, Function(hWnd As IntPtr, lParam As IntPtr) As Boolean
                                                   Dim className As New System.Text.StringBuilder(256)
                                                   GetClassName(hWnd, className, 256)
                                                   If className.ToString() = "Button" Then
                                                       childButtons.Add(hWnd)
                                                   End If
                                                   Return True
                                               End Function, IntPtr.Zero)

                For Each btnHandle As IntPtr In childButtons
                    Dim btnText As New System.Text.StringBuilder(256)
                    GetWindowText(btnHandle, btnText, 256)
                    Dim text As String = btnText.ToString().Trim()

                    For Each confirmText As String In confirmTexts
                        If text.Equals(confirmText, StringComparison.OrdinalIgnoreCase) OrElse
                           text.Replace("&", "").Equals(confirmText.Replace("&", ""), StringComparison.OrdinalIgnoreCase) Then
                            LogManager.Info("Auto-confirm: Clicking button '" & text & "' in dialog")
                            SendMessage(btnHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero)
                            clicked = True
                            Exit For
                        End If
                    Next

                    If clicked Then Exit For
                Next
            Catch ex As Exception
                LogManager.Warn("Error in TryClickConfirmButtons: " & ex.Message)
            End Try

            Return clicked
        End Function

    End Class

End Namespace
