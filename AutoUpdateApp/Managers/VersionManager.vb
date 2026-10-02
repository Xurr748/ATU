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

            Return NormalizeVersion(version)
        End Function

        ''' <summary>
        ''' Reads the sub-version from registry using SubVersionRegistryValueName config key.
        ''' Returns empty string if the key is not configured or the value does not exist.
        ''' </summary>
        Public Shared Function ReadRegistrySubVersion() As String
            Dim valueName As String = Config.AppSettings.SubVersionRegistryValueName
            If String.IsNullOrEmpty(valueName) Then Return String.Empty

            Dim keyPath As String = Config.AppSettings.RegistryKeyPath
            Dim subVer As String = Utilities.RegistryHelper.ReadValue(keyPath, valueName)
            If subVer Is Nothing Then Return String.Empty
            Return NormalizeVersion(subVer)
        End Function

        Public Shared Function ReadLatestVersion() As String
            Dim filePath As String = Config.AppSettings.VersionFilePath
            Dim content As String = Utilities.FileHelper.ReadAllTextSafe(filePath)

            If content Is Nothing Then
                LogManager.Warn("Could not read version file: " & filePath)
                Return String.Empty
            End If

            Return NormalizeVersion(content)
        End Function

        ''' <summary>
        ''' Normalises a raw version string: takes the first non-empty line, normalises Unicode
        ''' (converts full-width characters like 'Ａ' or '５' to standard ASCII 'A' or '5'),
        ''' and strips all whitespace, BOM, null bytes, zero-width spaces, and control characters.
        ''' </summary>
        Public Shared Function NormalizeVersion(raw As String) As String
            If String.IsNullOrEmpty(raw) Then Return String.Empty

            ' 1. Take only the first non-blank line so extra lines / comments are ignored
            Dim firstLine As String = String.Empty
            For Each line As String In raw.Split(New Char() {ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
                Dim t As String = line.Trim()
                If t.Length > 0 Then
                    firstLine = t
                    Exit For
                End If
            Next
            If String.IsNullOrEmpty(firstLine) Then Return String.Empty

            ' 2. Unicode normalization FormKC: converts full-width characters (e.g. Japanese full-width 'Ａ' -> 'A', '５' -> '5')
            Dim normalized As String = firstLine.Normalize(System.Text.NormalizationForm.FormKC)

            ' 3. Extract only valid version characters: letters, digits, '.', '-', '_', '+'
            ' This automatically discards BOM (\uFEFF), null bytes (\0), zero-width spaces (\u200B), whitespace, control chars
            Dim sb As New System.Text.StringBuilder(normalized.Length)
            For Each c As Char In normalized
                If Char.IsLetterOrDigit(c) OrElse c = "."c OrElse c = "-"c OrElse c = "_"c OrElse c = "+"c Then
                    sb.Append(c)
                End If
            Next
            Return sb.ToString()
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
