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
                If String.IsNullOrEmpty(CurrentVersion) OrElse String.IsNullOrEmpty(LatestVersion) Then
                    Return False
                End If
                Return Not String.Equals(CurrentVersion, LatestVersion, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

    End Class

End Namespace
