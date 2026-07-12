Option Strict On
Option Explicit On

Imports System.Configuration
Imports System.IO

Namespace Config

    ''' <summary>
    ''' Central access point for all app.config settings.
    ''' All paths are configurable — no hardcoded values.
    ''' Relative paths resolve against ConfigRoot.
    ''' </summary>
    Public NotInheritable Class AppSettings

        Private Sub New()
            ' Static-only class
        End Sub

        Private Shared Function GetSetting(key As String, Optional defaultValue As String = "") As String
            Dim value As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(value) Then
                Return defaultValue
            End If
            Return value
        End Function

        Private Shared Function ResolvePath(configRoot As String, path As String) As String
            If String.IsNullOrEmpty(path) Then
                Return path
            End If
            If IO.Path.IsPathRooted(path) Then
                Return path
            End If
            Return IO.Path.Combine(configRoot, path)
        End Function

        ' ───────────────────── Root Paths ─────────────────────

        Public Shared ReadOnly Property ConfigRoot As String
            Get
                Return GetSetting("ConfigRoot", "")
            End Get
        End Property

        Public Shared ReadOnly Property TesterTypePath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("TesterTypePath", "TesterType.csv"))
            End Get
        End Property

        Public Shared ReadOnly Property VersionFilePath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("VersionFilePath", "version.txt"))
            End Get
        End Property

        Public Shared ReadOnly Property UpdateFlagPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("UpdateFlagPath", "updateflag.txt"))
            End Get
        End Property

        ' ───────────────────── Installer Paths ─────────────────────

        Public Shared ReadOnly Property InstallerPathHE As String
            Get
                Return GetSetting("InstallerPathHE", "")
            End Get
        End Property

        Public Shared ReadOnly Property InstallerPathLLE As String
            Get
                Return GetSetting("InstallerPathLLE", "")
            End Get
        End Property

        Public Shared ReadOnly Property InstallerArgs As String
            Get
                Return GetSetting("InstallerArgs", "/silent /norestart")
            End Get
        End Property

        ' ───────────────────── Registry ─────────────────────

        Public Shared ReadOnly Property RegistryKeyPath As String
            Get
                Return GetSetting("RegistryKeyPath", "HKEY_LOCAL_MACHINE\SOFTWARE\MyApp")
            End Get
        End Property

        Public Shared ReadOnly Property RegistryValueName As String
            Get
                Return GetSetting("RegistryValueName", "Version")
            End Get
        End Property

        ' ───────────────────── Logging ─────────────────────

        Public Shared ReadOnly Property LogPath As String
            Get
                Return GetSetting("LogPath", "C:\Logs\AutoUpdate\")
            End Get
        End Property

        Public Shared ReadOnly Property LogLevel As String
            Get
                Return GetSetting("LogLevel", "Info")
            End Get
        End Property

        ' ───────────────────── Scheduler ─────────────────────

        Public Shared ReadOnly Property PollingIntervalMinutes As Integer
            Get
                Dim value As Integer
                If Integer.TryParse(GetSetting("PollingIntervalMinutes", "60"), value) Then
                    Return value
                End If
                Return 60
            End Get
        End Property

        ' ───────────────────── Reload ─────────────────────

        ''' <summary>
        ''' Forces a re-read of app.config on next property access.
        ''' </summary>
        Public Shared Sub Reload()
            ConfigurationManager.RefreshSection("appSettings")
        End Sub

    End Class

End Namespace
