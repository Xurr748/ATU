Option Strict On
Option Explicit On

Imports System.Diagnostics

Namespace Managers

    ''' <summary>
    ''' Launches the appropriate installer based on tester type (HE/LLE).
    ''' Installer path and arguments are fully configurable via app.config.
    ''' </summary>
    Public NotInheritable Class InstallerManager

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Returns the installer path for the given tester type.
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
        ''' Runs the installer for the given tester type.
        ''' Returns True if the installer exited with code 0, False otherwise.
        ''' Blocks until the installer process completes.
        ''' </summary>
        Public Shared Function RunInstaller(testerType As String) As Boolean
            Dim installerPath As String = GetInstallerPath(testerType)

            If String.IsNullOrEmpty(installerPath) Then
                LogManager.[Error]("Installer path is empty for type: " & testerType)
                Return False
            End If

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
                psi.UseShellExecute = False
                psi.CreateNoWindow = True

                Using proc As Process = Process.Start(psi)
                    If proc IsNot Nothing Then
                        proc.WaitForExit()
                        Dim exitCode As Integer = proc.ExitCode
                        LogManager.Info("Installer exited with code: " & exitCode.ToString())
                        Return (exitCode = 0)
                    End If
                End Using

                LogManager.[Error]("Failed to start installer process.")
                Return False

            Catch ex As Exception
                LogManager.[Error]("Failed to run installer: " & installerPath, ex)
                Return False
            End Try
        End Function

    End Class

End Namespace
