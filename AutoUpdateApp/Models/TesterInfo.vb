Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' Represents a single entry from TesterType.csv.
    ''' Maps: ComputerName, Type, Mode, Time
    ''' </summary>
    Public Class TesterInfo

        ''' <summary>Machine name (e.g. PC001)</summary>
        Public Property ComputerName As String

        ''' <summary>Tester type: HE or LLE</summary>
        Public Property TesterType As String

        ''' <summary>Update mode: EVA, Normal, or Auto</summary>
        Public Property Mode As String

        ''' <summary>Scheduled time for update check</summary>
        Public Property ScheduledTime As TimeSpan

    End Class

End Namespace
