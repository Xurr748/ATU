Option Strict On
Option Explicit On

Namespace Managers

    ''' <summary>
    ''' Reads and compares application versions.
    ''' Current version comes from the Windows Registry.
    ''' Latest version comes from version.txt on the config share.
    ''' </summary>
    Public NotInheritable Class VersionManager

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Reads the currently installed version from the registry.
        ''' Returns empty string if not found.
        ''' </summary>
        Public Shared Function ReadRegistryVersion() As String
            Dim keyPath As String = Config.AppSettings.RegistryKeyPath
            Dim valueName As String = Config.AppSettings.RegistryValueName
            Dim version As String = Utilities.RegistryHelper.ReadValue(keyPath, valueName)

            If version Is Nothing Then
                LogManager.Warn("Could not read version from registry: " & keyPath & "\" & valueName)
                Return String.Empty
            End If

            Return version.Trim()
        End Function

        ''' <summary>
        ''' Reads the latest available version from version.txt.
        ''' Returns empty string if the file cannot be read.
        ''' </summary>
        Public Shared Function ReadLatestVersion() As String
            Dim filePath As String = Config.AppSettings.VersionFilePath
            Dim content As String = Utilities.FileHelper.ReadAllTextSafe(filePath)

            If content Is Nothing Then
                LogManager.Warn("Could not read version file: " & filePath)
                Return String.Empty
            End If

            Return content.Trim()
        End Function

        ''' <summary>
        ''' Returns True if the installed version differs from the latest version.
        ''' Returns False if either version is empty (to avoid false positives).
        ''' </summary>
        Public Shared Function NeedsUpdate() As Boolean
            Dim current As String = ReadRegistryVersion()
            Dim latest As String = ReadLatestVersion()

            If String.IsNullOrEmpty(current) OrElse String.IsNullOrEmpty(latest) Then
                Return False
            End If

            Return Not String.Equals(current, latest, StringComparison.OrdinalIgnoreCase)
        End Function

    End Class

End Namespace
