Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' Aggregates all data needed for a single update cycle.
    ''' Passed to strategies for decision-making.
    ''' </summary>
    Public Class UpdateContext

        ''' <summary>Tester configuration from TesterType.csv</summary>
        Public Property Tester As TesterInfo

        ''' <summary>Current installed version (from registry)</summary>
        Public Property CurrentVersion As String

        ''' <summary>Latest available version (from version.txt)</summary>
        Public Property LatestVersion As String

        ''' <summary>True if updateflag.txt has a pending restart flag</summary>
        Public Property HasPendingRestartFlag As Boolean

        ''' <summary>
        ''' Computed: True when versions differ and both are non-empty.
        ''' </summary>
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
