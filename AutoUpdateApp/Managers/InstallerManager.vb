Option Strict On
Option Explicit On

Imports System.Diagnostics

Namespace Managers

    ''' <summary>
    ''' เรียกใช้ตัวติดตั้ง (Installer) ตามประเภทเครื่องทดสอบ (HE/LLE)
    ''' เส้นทางและอาร์กิวเมนต์ตั้งค่าได้ทั้งหมดผ่าน app.config
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
        Public Shared Function RunInstaller(testerType As String) As Boolean
            Dim installerFolder As String = GetInstallerPath(testerType)

            If String.IsNullOrEmpty(installerFolder) Then
                LogManager.[Error]("Installer path is empty for type: " & testerType)
                Return False
            End If

            Dim uninstallPath As String = IO.Path.Combine(installerFolder, "uninstall.bat")
            Dim installPath As String = IO.Path.Combine(installerFolder, "install.bat")

            ' รัน uninstall.bat (ถ้ามี)
            If Utilities.FileHelper.FileExistsSafe(uninstallPath) Then
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

            If Not RunBatchFile(installPath, "install") Then
                LogManager.[Error]("Install process failed.")
                Return False
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

    End Class

End Namespace
