Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.IO

Namespace Managers

    ''' <summary>
    ''' เรียกใช้ตัวติดตั้ง (Installer) ตามประเภทเครื่องทดสอบ (HE/LLE)
    ''' คัดลอกโฟลเดอร์จากเซิร์ฟเวอร์แบบ Recursive มายังเครื่องปลายทางก่อนรันติดตั้ง
    ''' มีฟังก์ชันปิด/เปิดโปรแกรมหลัก และเพิ่ม Startup Shortcut
    ''' </summary>
    Public NotInheritable Class InstallerManager

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        ''' <summary>
        ''' คืนค่าเส้นทาง Installer สำหรับประเภทเครื่องที่ระบุ
        ''' </summary>
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

        ''' <summary>
        ''' เรียกใช้ Installer สำหรับประเภทเครื่องที่ระบุ
        ''' โดยจะรัน uninstall.bat ก่อน แล้วตามด้วย install.bat
        ''' คืนค่า True หากทั้งสองกระบวนการทำงานสำเร็จ
        ''' รอจนกว่าจะทำงานเสร็จทีละตัว
        ''' </summary>
        Public Shared Function RunInstaller(testerType As String, Optional progressCallback As Action(Of Integer, String) = Nothing) As Boolean
            Dim installerFolder As String = GetInstallerPath(testerType)

            If String.IsNullOrEmpty(installerFolder) Then
                LogManager.[Error]("Installer path is empty for type: " & testerType)
                Return False
            End If

            ' ตรวจสอบโฟลเดอร์ตัวติดตั้งบนเซิร์ฟเวอร์ (หัวข้อ 3)
            If Not Directory.Exists(installerFolder) Then
                LogManager.[Error]("Installer folder not found on server: " & installerFolder)
                Return False
            End If

            ' บันทึก IP Address เมื่อมีการรันฟังก์ชันดาวน์โหลด/อัปเดต (หัวข้อ 7)
            LogManager.LogIPAddress()

            ' โฟลเดอร์ปลายทางบนเครื่องที่รัน (หัวข้อ 6)
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

                ' เคลียร์ไฟล์เก่าในเครื่องปลายทาง
                Try
                    If Directory.Exists(localFolder) Then
                        Directory.Delete(localFolder, True)
                    End If
                Catch ex As Exception
                    LogManager.Warn("Could not clear existing local installer folder: " & ex.Message)
                End Try

                Dim copySuccess As Boolean = True
                ' คัดลอกทั้งโฟลเดอร์แบบย่อยและอัปเดตความคืบหน้า (หัวข้อ 6)
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
                    ' ── ปิดโปรแกรมเป้าหมายก่อนถอนการติดตั้ง ──
                    KillTargetProcess()
                    CloseProgramOfRegistryPath()
                    Dim uninstallPath As String = IO.Path.Combine(localFolder, "uninstall.bat")
                    Dim installPath As String = IO.Path.Combine(localFolder, "install.bat")

                    Dim uninstallSuccess As Boolean = True
                    ' รัน uninstall — ถ้ามี UninstallProductName จะสร้าง smart uninstall อัตโนมัติ
                    Dim productName As String = Config.AppSettings.UninstallProductName

                    If Not String.IsNullOrEmpty(productName) Then
                        ' ── ค้นหา GUID จากชื่อโปรแกรมอัตโนมัติ ──
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

                            ' สร้าง smart uninstall.bat
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
                        ' ── ใช้ uninstall.bat ที่มีอยู่ (แบบเดิม) ──
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
                        ' ── Install: ค้นหา .msi จากโฟลเดอร์ Installer อัตโนมัติ ──
                        Dim msiFile As String = FindLatestMsi(installerFolder)
                        Dim installerArgs As String = Config.AppSettings.InstallerArgs

                        If Not String.IsNullOrEmpty(msiFile) Then
                            ' สร้าง smart install.bat
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
                            ' ── ใช้ install.bat ที่มีอยู่ (แบบเดิม) ──
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
                ' Cleanup temp folder
                Try
                    If Directory.Exists(localFolder) Then
                        Directory.Delete(localFolder, True)
                    End If
                Catch cleanupEx As Exception
                    LogManager.Warn("Could not clean up temp installer folder: " & cleanupEx.Message)
                End Try
            End Try

            ' ── เปิดแอพเป้าหมายหลังอัปเดตสำเร็จ ──
            If result Then
                LaunchTargetApp()
            End If

            Return result
        End Function

        ''' <summary>
        ''' ค้นหา GUID ของโปรแกรมจาก Registry Uninstall keys (ค้นทั้ง 64-bit และ 32-bit)
        ''' </summary>
        ''' <summary>
        ''' ค้นหาไฟล์ .msi ล่าสุดในโฟลเดอร์ Installer
        ''' </summary>
        ''' <summary>
        ''' เปิดแอพเป้าหมายหลังอัปเดตสำเร็จ (ดึง path จาก config หรือ Registry)
        ''' </summary>
        Public Shared Sub LaunchTargetApp()
            Try
                Dim appPath As String = Config.AppSettings.TargetAppExePath

                ' ถ้าไม่ได้ตั้ง path → ดึงจาก Registry
                If String.IsNullOrEmpty(appPath) Then
                    appPath = Utilities.RegistryHelper.ReadValue(
                        Config.AppSettings.RegistryKeyPath, Config.AppSettings.RegistryPathValueName)
                End If

                If String.IsNullOrEmpty(appPath) Then
                    LogManager.Warn("TargetAppExePath is empty and could not find path from Registry. Skipping app launch.")
                    Return
                End If

                ' ถ้า path เป็นโฟลเดอร์ → หาไฟล์ .exe
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

                ' เรียงจากใหม่ไปเก่า เลือกอันใหม่ที่สุด
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
                                            ' subKeyName คือ GUID เช่น {4772073D-714A-40AE-B120-0561D01099B6}
                                            LogManager.Info("Registry match: " & name & " → " & subKeyName & " (in " & regPath & ")")
                                            Return subKeyName
                                        End If
                                    End If
                                End Using
                            Catch
                                ' ข้ามถ้าอ่าน subkey ไม่ได้
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
                        ' รอ 30 นาที ป้องกัน hang ตลอดกาล
                        proc.WaitForExit(1800000)
                        If Not proc.HasExited Then
                            LogManager.Warn(stepName & " script timed out after 30 minutes.")
                            Return False
                        End If
                        Dim exitCode As Integer = proc.ExitCode
                        LogManager.Info(stepName & " script exited with code: " & exitCode.ToString())
                        ' Bat scripts may not always return 0, but we assume 0 means success.
                        Return (exitCode = 0)
                    End If
                End Using

                Return False
            Catch ex As Exception
                LogManager.[Error]("Failed to run " & stepName & " script: " & batchPath, ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' คัดลอกโฟลเดอร์และโฟลเดอร์ย่อยทั้งหมดพร้อมบอกความคืบหน้า (หัวข้อ 6)
        ''' </summary>
        Private Shared Sub CopyDirectoryWithProgress(sourceDir As String, destDir As String, progressCallback As Action(Of Integer, String))
            Dim sourceDirInfo As New DirectoryInfo(sourceDir)
            If Not sourceDirInfo.Exists Then
                Throw New DirectoryNotFoundException("Source directory not found: " & sourceDir)
            End If

            ' สแกนหาไฟล์ทั้งหมดเพื่อคำนวณขนาดรวมทั้งหมด
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
                ' คำนวณพาธปลายทาง
                Dim relativePath As String = file.FullName.Substring(sourceDirInfo.FullName.Length)
                If relativePath.StartsWith("\") OrElse relativePath.StartsWith("/") Then
                    relativePath = relativePath.Substring(1)
                End If
                Dim destFilePath As String = Path.Combine(destDir, relativePath)

                ' ตรวจสอบและสร้างโฟลเดอร์ย่อยปลายทาง
                Dim destSubDir As String = Path.GetDirectoryName(destFilePath)
                If Not Directory.Exists(destSubDir) Then
                    Directory.CreateDirectory(destSubDir)
                End If

                ' คัดลอกโดยใช้ Buffer เพื่อรายงานความคืบหน้าแบบละเอียด
                Dim buffer(65536 - 1) As Byte ' 64KB
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

                LogManager.Info(String.Format("คัดลอกไฟล์สำเร็จ: {0} ({1}/{2})", file.Name, currentFileIndex + 1, fileCount))
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

        ''' <summary>
        ''' ปิดโปรแกรมเป้าหมายทั้งหมดก่อนทำการอัปเดต
        ''' อ่านรายชื่อจาก KillProcessList (คั่นด้วย ,) + UninstallProductName
        ''' ใช้ Force Kill เพื่อข้าม Confirmation Dialog ของแอพ
        ''' </summary>
        Public Shared Sub KillTargetProcess()
            Try
                ' รวบรวมรายชื่อ process ที่ต้องปิด
                Dim processNames As New List(Of String)()

                ' 1. จาก KillProcessList config (คั่นด้วย ,)
                Dim killList As String = Config.AppSettings.KillProcessList
                If Not String.IsNullOrEmpty(killList) Then
                    For Each name As String In killList.Split(","c)
                        Dim trimmed As String = name.Trim().Replace(".exe", "")
                        If Not String.IsNullOrEmpty(trimmed) AndAlso Not processNames.Contains(trimmed) Then
                            processNames.Add(trimmed)
                        End If
                    Next
                End If

                ' 2. จาก UninstallProductName (ถ้ายังไม่มีในรายชื่อ)
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

        ''' <summary>
        ''' ปิด process ที่ระบุชื่อ — ใช้ Force Kill เพื่อข้าม Confirmation Dialog
        ''' ลอง CloseMainWindow ก่อน รอ 2 วินาที ถ้าไม่ปิด → Kill ทันที
        ''' ถ้ายังไม่ปิด → ใช้ taskkill /F /IM เป็น fallback สุดท้าย
        ''' </summary>
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

                        ' ลอง CloseMainWindow ก่อน (ให้โอกาสปิดปกติ)
                        proc.CloseMainWindow()

                        ' รอแค่ 2 วินาที — ถ้าแอพมี confirmation dialog จะไม่ปิดทันเวลา
                        If Not proc.WaitForExit(2000) Then
                            ' Force Kill ทันที — ข้าม confirmation dialog
                            LogManager.Info("Force killing (bypass confirmation): " & proc.ProcessName)
                            proc.Kill()
                            proc.WaitForExit(3000)
                        End If

                        LogManager.Info("Process closed: " & proc.ProcessName)
                    Catch ex As Exception
                        LogManager.Warn("Could not kill " & proc.ProcessName & ": " & ex.Message)
                    End Try
                Next

                ' ── Fallback: ใช้ taskkill เผื่อยังหลุดรอดอยู่ ──
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

        ''' <summary>
        ''' ปิดโปรแกรมหลักที่ระบุใน registry path (หัวข้อ 5.3)
        ''' </summary>
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
                            ' ตรวจเช็กโมดูลหลัก (ใช้สิทธิ์แอดมินหรือดักจับ Error ในกรณีสิทธิ์ไม่ถึง)
                            Dim mainModulePath As String = proc.MainModule.FileName
                            If mainModulePath.StartsWith(targetPath, StringComparison.OrdinalIgnoreCase) Then
                                isTarget = True
                            End If
                        End If
                        
                        If isTarget Then
                            LogManager.Info("Closing target process: " & proc.ProcessName & " (PID: " & proc.Id & ")")
                            proc.CloseMainWindow()
                            ' รอสูงสุด 5 วินาที ถ้าไม่ปิดเองจะทำการ Kill
                            If Not proc.WaitForExit(5000) Then
                                LogManager.Warn("Process did not exit, force killing: " & proc.ProcessName)
                                proc.Kill()
                            End If
                        End If
                    Catch ex As Exception
                        ' ป้องกันการขัดข้องกรณีระบบป้องกันของ OS หรือสิทธิ์การเข้าถึง process อื่น
                    End Try
                Next
            Catch ex As Exception
                LogManager.Error("Error closing target program of registry path.", ex)
            End Try
        End Sub

        ''' <summary>
        ''' เปิดโปรแกรมหลักขึ้นมาใหม่หลังจากอัปเดตเสร็จ (หัวข้อ 5.5)
        ''' </summary>
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

        ''' <summary>
        ''' คัดลอก/สร้าง Shortcut ไปยังโฟลเดอร์ Startup เพื่อเปิดอัตโนมัติเมื่อเปิดเครื่อง
        ''' ตรวจสอบค่า EnableTargetStartup ก่อนทำงาน
        ''' </summary>
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

                ' ถ้า registry ชี้ไปที่โฟลเดอร์ ให้หา exe ข้างใน
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

                ' ลบ shortcut เก่าก่อน (ถ้าเปิดใช้งาน)
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

        ''' <summary>
        ''' ใส่ตัว AutoUpdateApp เองไปที่ Startup folder
        ''' ตรวจสอบค่า EnableSelfStartup ก่อนทำงาน
        ''' </summary>
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

                ' ลบ shortcut เก่าก่อนสร้างใหม่ทุกครั้ง (ทั้ง Current User + All Users)
                Dim selfName As String = Path.GetFileNameWithoutExtension(selfExePath)
                RemoveStartupShortcut(selfName)

                LogManager.Info("Adding self to startup: " & shortcutPath)
                CreateShortcut(shortcutPath, selfExePath)
                LogManager.Info("Self startup shortcut created successfully.")
            Catch ex As Exception
                LogManager.Error("Error adding self to startup.", ex)
            End Try
        End Sub

        ''' <summary>
        ''' ลบ Shortcut ที่ชื่อตรงกันออกจาก Startup folder ทั้ง Current User และ All Users
        ''' </summary>
        Public Shared Sub RemoveStartupShortcut(shortcutBaseName As String)
            Try
                If String.IsNullOrEmpty(shortcutBaseName) Then Return

                ' Current User Startup
                Dim currentUserStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                RemoveShortcutFromFolder(currentUserStartup, shortcutBaseName)

                ' All Users Startup (Common Startup)
                Dim allUsersStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
                RemoveShortcutFromFolder(allUsersStartup, shortcutBaseName)
            Catch ex As Exception
                LogManager.Warn("Could not remove startup shortcut: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' ลบ Shortcut จากโฟลเดอร์ที่ระบุ
        ''' </summary>
        Private Shared Sub RemoveShortcutFromFolder(folderPath As String, shortcutBaseName As String)
            Try
                If String.IsNullOrEmpty(folderPath) Then Return

                Dim shortcutPath As String = Path.Combine(folderPath, shortcutBaseName & ".lnk")
                If File.Exists(shortcutPath) Then
                    File.Delete(shortcutPath)
                    LogManager.Info("Removed startup shortcut: " & shortcutPath)
                End If

                ' ลบ .exe ตรงๆ ที่อาจถูกวางไว้ด้วย
                Dim exePath As String = Path.Combine(folderPath, shortcutBaseName & ".exe")
                If File.Exists(exePath) Then
                    File.Delete(exePath)
                    LogManager.Info("Removed startup exe: " & exePath)
                End If
            Catch ex As Exception
                LogManager.Warn("Could not remove shortcut from " & folderPath & ": " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' ตรวจสอบว่าการติดตั้งสำเร็จจริงหรือไม่ โดยเปรียบเทียบ Registry Version กับ version.txt
        ''' </summary>
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

        ''' <summary>
        ''' สร้าง Windows Shortcut (.lnk) ด้วย WScript.Shell COM
        ''' </summary>
        Private Shared Sub CreateShortcut(shortcutPath As String, targetExePath As String)
            Dim shellType As Type = Type.GetTypeFromProgID("WScript.Shell")
            Dim shell As Object = Activator.CreateInstance(shellType)
            Dim shortcut As Object = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, Nothing, shell, New Object() {shortcutPath})

            Dim shortcutType As Type = shortcut.GetType()
            shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {targetExePath})
            shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {Path.GetDirectoryName(targetExePath)})
            shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, Nothing, shortcut, Nothing)
        End Sub

    End Class

End Namespace

