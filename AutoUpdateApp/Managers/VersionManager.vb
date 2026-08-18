Option Strict On
Option Explicit On

Namespace Managers

    Public NotInheritable Class VersionManager

        Private Sub New()
        End Sub

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

        Public Shared Function ReadLatestVersion() As String
            Dim filePath As String = Config.AppSettings.VersionFilePath
            Dim content As String = Utilities.FileHelper.ReadAllTextSafe(filePath)

            If content Is Nothing Then
                LogManager.Warn("Could not read version file: " & filePath)
                Return String.Empty
            End If

            Return content.Trim()
        End Function

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
