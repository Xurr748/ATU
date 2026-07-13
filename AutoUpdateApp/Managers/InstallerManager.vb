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
        ''' คืนค่า True หาก Installer ปิดตัวด้วยรหัส 0 มิฉะนั้นคืนค่า False
        ''' รอจนกว่า Installer จะทำงานเสร็จ
        ''' </summary>
        Public Shared Function RunInstaller(testerType As String) As Boolean
            Dim installerFolder As String = GetInstallerPath(testerType)

            If String.IsNullOrEmpty(installerFolder) Then
                LogManager.[Error]("Installer path is empty for type: " & testerType)
                Return False
            End If

            ' ต่อ \setup.exe เข้ากับ path โฟลเดอร์
            Dim installerPath As String = IO.Path.Combine(installerFolder, "setup.exe")

            If Not Utilities.FileHelper.FileExistsSafe(installerPath) Then
                LogManager.[Error]("Installer not found: " & installerPath)
                Return False
            End If

            Try
                Dim args As String = Config.AppSettings.InstallerArgs
                LogManager.Info("Starting installer: " & installerPath & " " & args)

                Dim psi As New ProcessStartInfo()
                psi.FileName = installerPath
                psi.Arguments = args
                psi.UseShellExecute = True
                psi.WindowStyle = ProcessWindowStyle.Hidden

                Using proc As Process = Process.Start(psi)
                    If proc IsNot Nothing Then
                        ' รอ 30 นาที ป้องกัน hang ตลอดกาล
                        proc.WaitForExit(1800000)
                        If Not proc.HasExited Then
                            LogManager.Warn("Installer timed out after 30 minutes.")
                            Return False
                        End If
                        Dim exitCode As Integer = proc.ExitCode
                        LogManager.Info("Installer exited with code: " & exitCode.ToString())
                        Return (exitCode = 0)
                    End If
                End Using

                LogManager.[Error]("ไม่สามารถเริ่ม Process ของ Installer ได้")
                Return False

            Catch ex As Exception
                LogManager.[Error]("Failed to run installer: " & installerPath, ex)
                Return False
            End Try
        End Function

    End Class

End Namespace
