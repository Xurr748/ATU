Option Strict On
Option Explicit On

Namespace Utilities

    ''' <summary>
    ''' Provides environment information with lazy caching.
    ''' </summary>
    Public NotInheritable Class EnvironmentHelper

        Private Shared _computerName As String

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Gets the machine name (cached after first access).
        ''' </summary>
        Public Shared ReadOnly Property ComputerName As String
            Get
                If _computerName Is Nothing Then
                    _computerName = Environment.MachineName
                End If
                Return _computerName
            End Get
        End Property

    End Class

End Namespace
