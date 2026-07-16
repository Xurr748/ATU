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
            Dim localFolder As String = Path.Combine(Path.GetTempPath(), "AutoUpdateApp_LocalInstaller")
            LogManager.Info(String.Format("Downloading installer folder: {0} -> {1}", installerFolder, localFolder))

            If progressCallback IsNot Nothing Then
                progressCallback(0, "กำลังเตรียมดาวน์โหลด...")
            End If

            ' เคลียร์ไฟล์เก่าในเครื่องปลายทาง
            Try
                If Directory.Exists(localFolder) Then
                    Directory.Delete(localFolder, True)
                End If
            Catch ex As Exception
                LogManager.Warn("Could not clear existing local installer folder: " & ex.Message)
            End Try

            ' คัดลอกทั้งโฟลเดอร์แบบย่อยและอัปเดตความคืบหน้า (หัวข้อ 6)
            Try
                CopyDirectoryWithProgress(installerFolder, localFolder, progressCallback)
            Catch ex As Exception
                LogManager.[Error]("Failed to copy installer files from server: " & installerFolder, ex)
                If progressCallback IsNot Nothing Then
                    progressCallback(0, "ดาวน์โหลดล้มเหลว")
                End If
                Return False
            End Try

            Dim uninstallPath As String = IO.Path.Combine(localFolder, "uninstall.bat")
            Dim installPath As String = IO.Path.Combine(localFolder, "install.bat")

            ' รัน uninstall.bat (ถ้ามี)
            If Utilities.FileHelper.FileExistsSafe(uninstallPath) Then
                If progressCallback IsNot Nothing Then
                    progressCallback(90, "กำลังดำเนินการถอนการติดตั้ง...")
                End If
                If Not RunBatchFile(uninstallPath, "uninstall") Then
                    LogManager.[Error]("Uninstall process failed.")
                    Return False
                End If
            Else
                LogManager.Warn("Uninstall script not found: " & uninstallPath & " (Skipping uninstall step)")
            End If

            ' รัน install.bat
            If Not Utilities.FileHelper.FileExistsSafe(installPath) Then
                LogManager.[Error]("Install script not found: " & installPath)
                Return False
            End If

            If progressCallback IsNot Nothing Then
                progressCallback(95, "กำลังดำเนินการติดตั้ง...")
            End If
            
            If Not RunBatchFile(installPath, "install") Then
                LogManager.[Error]("Install process failed.")
                Return False
            End If

            If progressCallback IsNot Nothing Then
                progressCallback(100, "การอัปเดตเสร็จสมบูรณ์")
            End If

            Return True
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
                                Dim statusMsg As String = String.Format("กำลังดาวน์โหลด: {0} ({1}/{2} ไฟล์)", file.Name, currentFileIndex + 1, fileCount)
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
        ''' คัดลอก/สร้าง Shortcut ไปยังโฟลเดอร์ Startup เพื่อเปิดอัตโนมัติเมื่อเปิดเครื่อง (หัวข้อ 5.6)
        ''' </summary>
        Public Shared Sub CopyShortcutToStartup()
            Try
                Dim keyPath As String = Config.AppSettings.RegistryKeyPath
                Dim pathValueName As String = Config.AppSettings.RegistryPathValueName
                Dim targetPath As String = Utilities.RegistryHelper.ReadValue(keyPath, pathValueName)
                
                If String.IsNullOrEmpty(targetPath) OrElse Not File.Exists(targetPath) Then
                    LogManager.Warn("Cannot find program executable to create shortcut: " & targetPath)
                    Return
                End If
                
                Dim startupFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                Dim shortcutName As String = Path.GetFileNameWithoutExtension(targetPath) & ".lnk"
                Dim shortcutPath As String = Path.Combine(startupFolder, shortcutName)
                
                LogManager.Info("Creating startup shortcut at: " & shortcutPath)
                
                ' ใช้ WScript.Shell ผ่าน COM Reflection เพื่อสร้าง Lnk Shortcut โดยไม่ต้องแอด Reference .vbproj
                Dim shellType As Type = Type.GetTypeFromProgID("WScript.Shell")
                Dim shell As Object = Activator.CreateInstance(shellType)
                Dim shortcut As Object = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, Nothing, shell, New Object() {shortcutPath})
                
                Dim shortcutType As Type = shortcut.GetType()
                shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {targetPath})
                shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, Nothing, shortcut, New Object() {Path.GetDirectoryName(targetPath)})
                shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, Nothing, shortcut, Nothing)
                
                LogManager.Info("Startup shortcut created successfully.")
            Catch ex As Exception
                LogManager.Error("Error creating startup shortcut.", ex)
            End Try
        End Sub

    End Class

End Namespace
