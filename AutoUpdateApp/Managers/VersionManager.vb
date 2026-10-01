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
        ''' Normalises a raw version string: takes the first non-empty line, then removes
        ''' all remaining internal whitespace so "5.83.42 A" and "5.83.42A" compare equal.
        ''' Multi-line files (e.g. version.txt with notes on line 2) are handled safely.
        ''' </summary>
        Private Shared Function NormalizeVersion(raw As String) As String
            If raw Is Nothing Then Return String.Empty
            ' Take only the first non-blank line so extra lines / comments are ignored
            Dim firstLine As String = String.Empty
            For Each line As String In raw.Split(New Char() {ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
                Dim t As String = line.Trim()
                If t.Length > 0 Then
                    firstLine = t
                    Exit For
                End If
            Next
            If firstLine.Length = 0 Then Return String.Empty
            ' Strip any remaining internal whitespace (e.g. "5.83.42 A" → "5.83.42A")
            Dim sb As New System.Text.StringBuilder(firstLine.Length)
            For Each c As Char In firstLine
                If Not Char.IsWhiteSpace(c) Then
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
