Option Strict On
Option Explicit On

Imports Microsoft.Win32

Namespace Utilities

    ''' <summary>
    ''' Provides safe registry read operations.
    ''' Returns Nothing on failure — callers handle logging.
    ''' </summary>
    Public NotInheritable Class RegistryHelper

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Reads a string value from the registry.
        ''' Returns Nothing if the key/value is not found or access is denied.
        ''' </summary>
        ''' <param name="keyPath">Full registry key path (e.g. HKEY_LOCAL_MACHINE\SOFTWARE\MyApp)</param>
        ''' <param name="valueName">Name of the value to read</param>
        Public Shared Function ReadValue(keyPath As String, valueName As String) As String
            Try
                Dim value As Object = Registry.GetValue(keyPath, valueName, Nothing)
                If value IsNot Nothing Then
                    Return value.ToString()
                End If
            Catch ex As Security.SecurityException
                ' Insufficient permissions — caller should log
            Catch ex As Exception
                ' Key not found or other error — caller should log
            End Try
            Return Nothing
        End Function

    End Class

End Namespace
