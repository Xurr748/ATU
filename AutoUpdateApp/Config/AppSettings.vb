Option Strict On
Option Explicit On

Imports System.IO

Namespace Config

    Public NotInheritable Class AppSettings

        Private Shared ReadOnly _lock As New Object
        Private Shared _settings As Dictionary(Of String, String)
        Private Shared _configLoadedPath As String = ""
        Private Shared _configLoadStatus As String = ""
        Private Shared _isLocalError As Boolean = False

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property LoadedConfigPath As String
            Get
                EnsureLoaded()
                SyncLock _lock
                    Return _configLoadedPath
                End SyncLock
            End Get
        End Property

        Public Shared ReadOnly Property LoadStatus As String
            Get
                EnsureLoaded()
                SyncLock _lock
                    Return _configLoadStatus
                End SyncLock
            End Get
        End Property

        Public Shared ReadOnly Property IsLoaded As Boolean
            Get
                EnsureLoaded()
                SyncLock _lock
                    Return Not String.IsNullOrEmpty(_configLoadedPath)
                End SyncLock
            End Get
        End Property

        Public Shared ReadOnly Property IsLocalConfigError As Boolean
            Get
                EnsureLoaded()
                SyncLock _lock
                    Return _isLocalError
                End SyncLock
            End Get
        End Property

        Private Shared Sub EnsureLoaded()
            If _settings IsNot Nothing Then Return

            SyncLock _lock
                If _settings IsNot Nothing Then Return

                _settings = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                _configLoadedPath = ""
                _configLoadStatus = ""
                _isLocalError = False

                Dim serverConfigPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverconfig.txt")
                Dim realConfigPath As String = ""
                
                ' 1. Read serverconfig.txt to find real config path
                If File.Exists(serverConfigPath) Then
                    Try
                        Dim lines As String() = SafeReadAllLines(serverConfigPath)
                        For Each line As String In lines
                            Dim trimmed As String = line.Trim()
                            If trimmed.StartsWith(";") OrElse trimmed.StartsWith("#") Then Continue For
                            
                            Dim eqIndex As Integer = trimmed.IndexOf("="c)
                            If eqIndex > 0 Then
                                Dim key As String = trimmed.Substring(0, eqIndex).Trim()
                                If key.Equals("ConfigPath", StringComparison.OrdinalIgnoreCase) Then
                                    realConfigPath = trimmed.Substring(eqIndex + 1).Trim()
                                    If realConfigPath.StartsWith("""") AndAlso realConfigPath.EndsWith("""") Then
                                        realConfigPath = realConfigPath.Substring(1, realConfigPath.Length - 2)
                                    End If
                                    Exit For
                                End If
                            End If
                        Next
                    Catch ex As Exception
                        _configLoadStatus = "Failed to read serverconfig.txt: " & ex.Message
                        _isLocalError = True
                        Return
                    End Try
                Else
                    _configLoadStatus = "serverconfig.txt not found at: " & serverConfigPath
                    _isLocalError = True
                    Return
                End If

                If String.IsNullOrEmpty(realConfigPath) Then
                    _configLoadStatus = "serverconfig.txt does not contain ConfigPath=..."
                    _isLocalError = True
                    Return
                End If

                ' 2. Load real Config from the specified Path
                '    Do NOT use File.Exists() for network paths - it returns false
                '    on UNC paths when WiFi/network is not ready yet.
                '    Instead, try to read directly and catch exceptions.
                Try
                    LoadSettingsFromFile(realConfigPath)
                    _configLoadedPath = realConfigPath
                    _configLoadStatus = "Loaded " & _settings.Count & " settings from: " & realConfigPath
                    Managers.LogManager.Info("CONFIG_LOADED: " & _settings.Count.ToString() & " settings from " & realConfigPath)

                    ' 3. Read Language override from serverconfig.txt (local)
                    Try
                        Dim scLines As String() = SafeReadAllLines(serverConfigPath)
                        For Each scLine As String In scLines
                            Dim scTrimmed As String = scLine.Trim()
                            If scTrimmed.StartsWith(";") OrElse scTrimmed.StartsWith("#") Then Continue For
                            Dim scEq As Integer = scTrimmed.IndexOf("="c)
                            If scEq > 0 Then
                                Dim scKey As String = scTrimmed.Substring(0, scEq).Trim()
                                If scKey.Equals("Language", StringComparison.OrdinalIgnoreCase) Then
                                    Dim scVal As String = scTrimmed.Substring(scEq + 1).Trim()
                                    If scVal.StartsWith("""") AndAlso scVal.EndsWith("""") Then
                                        scVal = scVal.Substring(1, scVal.Length - 2)
                                    End If
                                    If Not String.IsNullOrEmpty(scVal) Then
                                        _settings("Language") = scVal
                                    End If
                                    Exit For
                                End If
                            End If
                        Next
                    Catch
                    End Try

                Catch ex As Exception
                    _configLoadStatus = "Server config unreachable: " & realConfigPath & " - " & ex.Message
                End Try
            End SyncLock
        End Sub

        Private Shared Sub LoadSettingsFromFile(filePath As String)
            Dim lines As String() = SafeReadAllLines(filePath)
            For Each line As String In lines
                Dim trimmed As String = line.Trim()

                If String.IsNullOrEmpty(trimmed) Then Continue For
                If trimmed.StartsWith(";") OrElse trimmed.StartsWith("#") Then Continue For

                Dim eqIndex As Integer = trimmed.IndexOf("="c)
                If eqIndex > 0 Then
                    Dim key As String = trimmed.Substring(0, eqIndex).Trim()
                    Dim value As String = trimmed.Substring(eqIndex + 1).Trim()

                    If value.Length >= 2 AndAlso value.StartsWith("""") AndAlso value.EndsWith("""") Then
                        value = value.Substring(1, value.Length - 2)
                    End If

                    _settings(key) = value
                End If
            Next
        End Sub

        Private Shared Function SafeReadAllLines(filePath As String) As String()
            For attempt As Integer = 1 To 5
                Try
                    Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As New StreamReader(fs)
                            Dim lines As New List(Of String)()
                            Dim line As String = reader.ReadLine()
                            While line IsNot Nothing
                                lines.Add(line)
                                line = reader.ReadLine()
                            End While
                            Return lines.ToArray()
                        End Using
                    End Using
                Catch ex As IOException
                    If attempt = 5 Then Throw
                    Threading.Thread.Sleep(100 + (attempt * 50))
                End Try
            Next
            Return New String() {}
        End Function

        Private Shared Function GetSetting(key As String, Optional defaultValue As String = "") As String
            EnsureLoaded()
            Dim value As String = Nothing
            If _settings.TryGetValue(key, value) Then
                If Not String.IsNullOrWhiteSpace(value) Then
                    Return value
                End If
            End If
            Return defaultValue
        End Function

        Private Shared Function GetBoolSetting(key As String, Optional defaultValue As Boolean = True) As Boolean
            Dim value As String = GetSetting(key, defaultValue.ToString())
            Dim result As Boolean
            If Boolean.TryParse(value, result) Then
                Return result
            End If
            Return defaultValue
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

        Public Shared Function ValidateConfig() As List(Of String)
            EnsureLoaded()
            Dim issues As New List(Of String)

            If Not IsLoaded Then
                issues.Add("[CONFIG] " & _configLoadStatus)
                Return issues
            End If

            issues.Add("[CONFIG] " & _configLoadStatus)

            CheckKey(issues, "RegistryKeyPath", RegistryKeyPath, "HKEY_LOCAL_MACHINE\SOFTWARE\MyApp")
            CheckKey(issues, "RegistryValueName", RegistryValueName, "")
            CheckKey(issues, "RegistryPathValueName", RegistryPathValueName, "")
            CheckKey(issues, "TesterTypePath", TesterTypePath, "")
            CheckKey(issues, "VersionFilePath", VersionFilePath, "")
            CheckKey(issues, "LogPath", LogPath, "C:\Logs\AutoUpdate\")
            CheckKey(issues, "InstallerPathHE", InstallerPathHE, "")
            CheckKey(issues, "InstallerPathLLE", InstallerPathLLE, "")

            CheckPathExists(issues, "TesterTypePath", TesterTypePath)
            CheckPathExists(issues, "VersionFilePath", VersionFilePath)

            Return issues
        End Function

        Private Shared Sub CheckKey(issues As List(Of String), keyName As String, currentValue As String, defaultValue As String)
            If String.IsNullOrEmpty(currentValue) Then
                issues.Add("[Empty] " & keyName & " = (no value)")
            ElseIf Not String.IsNullOrEmpty(defaultValue) AndAlso String.Equals(currentValue, defaultValue, StringComparison.OrdinalIgnoreCase) Then
                issues.Add("[OK] " & keyName & " = " & currentValue & " (may not be set yet)")
            Else
                issues.Add("[OK] " & keyName & " = " & currentValue)
            End If
        End Sub

        Private Shared Sub CheckPathExists(issues As List(Of String), keyName As String, pathValue As String)
            If String.IsNullOrEmpty(pathValue) Then Return
            If Not File.Exists(pathValue) AndAlso Not IO.Directory.Exists(pathValue) Then
                issues.Add("[Warning] " & keyName & " path not found: " & pathValue)
            End If
        End Sub


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

        Public Shared ReadOnly Property LocalExeDestinationPath As String
            Get
                Return GetSetting("LocalExeDestinationPath", "")
            End Get
        End Property


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

        Public Shared ReadOnly Property LocalInstallerPath As String
            Get
                Return GetSetting("LocalInstallerPath", "")
            End Get
        End Property

        Public Shared ReadOnly Property UninstallProductName As String
            Get
                Return GetSetting("UninstallProductName", "")
            End Get
        End Property


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

        Public Shared ReadOnly Property RegistryPathValueName As String
            Get
                Return GetSetting("RegistryPathValueName", "Path")
            End Get
        End Property

        Public Shared ReadOnly Property FolderName As String
            Get
                Return GetSetting("FolderName", "Logs")
            End Get
        End Property


        Public Shared ReadOnly Property LogPath As String
            Get
                Return GetSetting("LogPath", "C:\Logs\AutoUpdate\")
            End Get
        End Property

        Public Shared ReadOnly Property LogFileName As String
            Get
                Return GetSetting("LogFileName", "{ComputerName}_Logs.txt")
            End Get
        End Property


        Public Shared ReadOnly Property DetailInfoPdfPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("DetailInfoPdfPath", ""))
            End Get
        End Property

        Public Shared ReadOnly Property DetailPdfPath As String
            Get
                Return ResolvePath(ConfigRoot, GetSetting("DetailPdfPath", ""))
            End Get
        End Property

        Public Shared ReadOnly Property LogLevel As String
            Get
                Return GetSetting("LogLevel", "Info")
            End Get
        End Property


        Public Shared ReadOnly Property PollingIntervalMinutes As Integer
            Get
                Dim value As Integer
                If Integer.TryParse(GetSetting("PollingIntervalMinutes", "60"), value) Then
                    Return value
                End If
                Return 60
            End Get
        End Property


        Public Shared ReadOnly Property EnableSelfStartup As Boolean
            Get
                Return GetBoolSetting("EnableSelfStartup", True)
            End Get
        End Property

        Public Shared ReadOnly Property EnableTargetStartup As Boolean
            Get
                Return GetBoolSetting("EnableTargetStartup", True)
            End Get
        End Property

        Public Shared ReadOnly Property RemoveOldStartupShortcut As Boolean
            Get
                Return GetBoolSetting("RemoveOldStartupShortcut", True)
            End Get
        End Property

        Public Shared ReadOnly Property StartupShortcutName As String
            Get
                Return GetSetting("StartupShortcutName", "")
            End Get
        End Property


        Public Shared Sub Reload()
            SyncLock _lock
                _settings = Nothing
                _configLoadedPath = ""
                _configLoadStatus = ""
                _isLocalError = False
            End SyncLock
        End Sub


        Public Shared ReadOnly Property Language As String
            Get
                Return GetSetting("Language", "en")
            End Get
        End Property

        Public Shared Sub UpdateLanguage(lang As String)
            EnsureLoaded()
            If String.IsNullOrEmpty(lang) Then Return

            SyncLock _lock
                _settings("Language") = lang.ToLower()

                ' Save to local serverconfig.txt instead of server config
                Dim localPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverconfig.txt")

                Try
                    Dim lines As New List(Of String)()
                    Dim found As Boolean = False

                    If File.Exists(localPath) Then
                        Dim fileLines As String() = File.ReadAllLines(localPath)
                        For Each line As String In fileLines
                            Dim trimmed As String = line.Trim()
                            If Not trimmed.StartsWith(";") AndAlso Not trimmed.StartsWith("#") AndAlso trimmed.Contains("=") Then
                                Dim eqIndex As Integer = trimmed.IndexOf("="c)
                                Dim key As String = trimmed.Substring(0, eqIndex).Trim()
                                If String.Equals(key, "Language", StringComparison.OrdinalIgnoreCase) Then
                                    lines.Add("Language = " & lang.ToLower())
                                    found = True
                                    Continue For
                                End If
                            End If
                            lines.Add(line)
                        Next
                    End If

                    If Not found Then
                        lines.Add("")
                        lines.Add("; Language setting saved by app")
                        lines.Add("Language = " & lang.ToLower())
                    End If

                    File.WriteAllLines(localPath, lines.ToArray())
                Catch ex As Exception
                    Managers.LogManager.Warn("Failed to save to serverconfig.txt: " & ex.Message)
                End Try
            End SyncLock
        End Sub


        Public Shared ReadOnly Property TargetAppExePath As String
            Get
                Return GetSetting("TargetAppExePath", "")
            End Get
        End Property

        Public Shared ReadOnly Property KillProcessList As String
            Get
                Return GetSetting("KillProcessList", "")
            End Get
        End Property


        Public Shared ReadOnly Property CopyFilesSource As String
            Get
                Return GetSetting("CopyFilesSource", "")
            End Get
        End Property

        Public Shared ReadOnly Property CopyFilesDestination As String
            Get
                Return GetSetting("CopyFilesDestination", "")
            End Get
        End Property


        Public Shared ReadOnly Property AutoConfirmAfterLaunch As Boolean
            Get
                Dim val As String = GetSetting("AutoConfirmAfterLaunch", "false")
                Return String.Equals(val, "true", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

    End Class

End Namespace
