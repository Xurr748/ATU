Option Strict On
Option Explicit On

Imports System.IO

Namespace Config

    Public NotInheritable Class AppSettings

        Private Shared ReadOnly _lock As New Object
        Private Shared _settings As Dictionary(Of String, String)
        Private Shared _configLoadedPath As String = ""
        Private Shared _configLoadStatus As String = ""

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property LoadedConfigPath As String
            Get
                EnsureLoaded()
                Return _configLoadedPath
            End Get
        End Property

        Public Shared ReadOnly Property LoadStatus As String
            Get
                EnsureLoaded()
                Return _configLoadStatus
            End Get
        End Property

        Public Shared ReadOnly Property IsLoaded As Boolean
            Get
                EnsureLoaded()
                Return Not String.IsNullOrEmpty(_configLoadedPath)
            End Get
        End Property

        Private Shared Sub EnsureLoaded()
            If _settings IsNot Nothing Then Return

            SyncLock _lock
                If _settings IsNot Nothing Then Return

                _settings = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                _configLoadedPath = ""
                _configLoadStatus = ""

                Dim serverConfigPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverconfig.txt")
                Dim realConfigPath As String = ""
                
                ' 1. อ่านไฟล์ serverconfig.txt เพื่อหาว่า config จริงอยู่ที่ไหน
                If File.Exists(serverConfigPath) Then
                    Try
                        Dim lines As String() = File.ReadAllLines(serverConfigPath)
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
                        _configLoadStatus = "อ่านไฟล์ serverconfig.txt ล้มเหลว: " & ex.Message
                        Return
                    End Try
                Else
                    _configLoadStatus = "ไม่พบไฟล์ระบุตำแหน่ง (serverconfig.txt) ที่: " & serverConfigPath
                    Return
                End If

                If String.IsNullOrEmpty(realConfigPath) Then
                    _configLoadStatus = "ไฟล์ serverconfig.txt ไม่มีบรรทัด ConfigPath=..."
                    Return
                End If

                ' 2. ไปโหลด Config จริงจาก Path ที่ได้มา
                If Not File.Exists(realConfigPath) Then
                    _configLoadStatus = "ไม่พบไฟล์ Config จริงที่: " & realConfigPath
                    Return
                End If

                Try
                    LoadSettingsFromFile(realConfigPath)
                    _configLoadedPath = realConfigPath
                    _configLoadStatus = "โหลดสำเร็จ " & _settings.Count & " ค่า จาก: " & realConfigPath
                Catch ex As Exception
                    _configLoadStatus = "อ่านไฟล์ Config จริงล้มเหลว: " & ex.Message
                End Try
            End SyncLock
        End Sub

        Private Shared Sub LoadSettingsFromFile(filePath As String)
            Dim lines As String() = File.ReadAllLines(filePath)
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

        Private Shared Function GetConfigFilePath() As String
            Try
                Dim customPath As String = System.Configuration.ConfigurationManager.AppSettings("ConfigFilePath")
                If Not String.IsNullOrWhiteSpace(customPath) Then
                    Return customPath
                End If
            Catch
            End Try
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt")
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
                issues.Add("[ค่าว่าง] " & keyName & " = (ไม่มีค่า)")
            ElseIf Not String.IsNullOrEmpty(defaultValue) AndAlso String.Equals(currentValue, defaultValue, StringComparison.OrdinalIgnoreCase) Then
                issues.Add("[ค่าเริ่มต้น] " & keyName & " = " & currentValue & " (อาจยังไม่ได้ตั้งค่าจริง)")
            Else
                issues.Add("[OK] " & keyName & " = " & currentValue)
            End If
        End Sub

        Private Shared Sub CheckPathExists(issues As List(Of String), keyName As String, pathValue As String)
            If String.IsNullOrEmpty(pathValue) Then Return
            If Not File.Exists(pathValue) AndAlso Not IO.Directory.Exists(pathValue) Then
                issues.Add("[หาไม่เจอ] " & keyName & " path ไม่มีอยู่จริง: " & pathValue)
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
            End SyncLock
        End Sub


        Public Shared ReadOnly Property Language As String
            Get
                Return GetSetting("Language", "th")
            End Get
        End Property

        Public Shared Sub UpdateLanguage(lang As String)
            EnsureLoaded()
            If String.IsNullOrEmpty(lang) Then Return

            SyncLock _lock
                _settings("Language") = lang.ToLower()

                Dim configPath As String = _configLoadedPath
                If String.IsNullOrEmpty(configPath) Then
                    configPath = GetConfigFilePath()
                End If

                Try
                    Dim lines As New List(Of String)()
                    Dim found As Boolean = False

                    If File.Exists(configPath) Then
                        Dim fileLines As String() = File.ReadAllLines(configPath)
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
                        lines.Add("; ── ภาษาที่บันทึกจากการเปลี่ยนภาษา ──")
                        lines.Add("Language = " & lang.ToLower())
                    End If

                    File.WriteAllLines(configPath, lines.ToArray())
                Catch ex As Exception
                    Managers.LogManager.Warn("ไม่สามารถบันทึกภาษาลง config.txt ได้: " & ex.Message)
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
