Option Strict On
Option Explicit On

Imports Microsoft.Win32

Namespace Utilities

    Public NotInheritable Class RegistryHelper

        Private Sub New()
        End Sub

        Public Shared Function ReadValue(keyPath As String, valueName As String) As String
            Dim rootKey As RegistryHive
            Dim subKeyPath As String = ""
            If Not ParseKeyPath(keyPath, rootKey, subKeyPath) Then
                Return Nothing
            End If

            Dim result As String = ReadFromView(rootKey, subKeyPath, valueName, RegistryView.Registry64)
            If result IsNot Nothing Then
                Return result
            End If

            result = ReadFromView(rootKey, subKeyPath, valueName, RegistryView.Registry32)
            Return result
        End Function

        Private Shared Function ReadFromView(rootKey As RegistryHive, subKeyPath As String, valueName As String, view As RegistryView) As String
            Try
                Using baseKey As RegistryKey = RegistryKey.OpenBaseKey(rootKey, view)
                    Using subKey As RegistryKey = baseKey.OpenSubKey(subKeyPath, False)
                        If subKey IsNot Nothing Then
                            Dim value As Object = subKey.GetValue(valueName, Nothing)
                            If value IsNot Nothing Then
                                Return value.ToString()
                            End If
                        End If
                    End Using
                End Using
            Catch ex As Security.SecurityException
            Catch ex As Exception
            End Try
            Return Nothing
        End Function

        Private Shared Function ParseKeyPath(keyPath As String, ByRef rootKey As RegistryHive, ByRef subKeyPath As String) As Boolean
            rootKey = RegistryHive.LocalMachine
            subKeyPath = ""

            If String.IsNullOrEmpty(keyPath) Then Return False

            Dim separatorIndex As Integer = keyPath.IndexOf("\"c)
            Dim rootPart As String
            If separatorIndex >= 0 Then
                rootPart = keyPath.Substring(0, separatorIndex).ToUpperInvariant()
                subKeyPath = keyPath.Substring(separatorIndex + 1)
            Else
                rootPart = keyPath.ToUpperInvariant()
                subKeyPath = ""
            End If

            Select Case rootPart
                Case "HKEY_LOCAL_MACHINE"
                    rootKey = RegistryHive.LocalMachine
                Case "HKEY_CURRENT_USER"
                    rootKey = RegistryHive.CurrentUser
                Case "HKEY_CLASSES_ROOT"
                    rootKey = RegistryHive.ClassesRoot
                Case "HKEY_USERS"
                    rootKey = RegistryHive.Users
                Case "HKEY_CURRENT_CONFIG"
                    rootKey = RegistryHive.CurrentConfig
                Case Else
                    Return False
            End Select

            Return True
        End Function

    End Class

End Namespace
