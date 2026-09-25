Option Strict On
Option Explicit On

Namespace Models

    Public Class UpdateContext

        Public Property Tester As TesterInfo

        Public Property CurrentVersion As String

        Public Property LatestVersion As String

        Public Property HasPendingRestartFlag As Boolean

        Public ReadOnly Property NeedsUpdate As Boolean
            Get
                If String.IsNullOrEmpty(LatestVersion) Then
                    Return False ' Cannot update if no latest version is available
                End If
                If String.IsNullOrEmpty(CurrentVersion) Then
                    Return True ' Needs update if currently not installed
                End If
                Return Not String.Equals(CurrentVersion, LatestVersion, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

    End Class

End Namespace
