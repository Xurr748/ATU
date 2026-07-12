Option Strict On
Option Explicit On

Namespace Models

    ''' <summary>
    ''' Represents a single entry from updateflag.txt.
    ''' Maps: ComputerName, UpdateFlag
    ''' </summary>
    Public Class UpdateFlagEntry

        ''' <summary>Machine name (e.g. PC001)</summary>
        Public Property ComputerName As String

        ''' <summary>True if update is pending after restart</summary>
        Public Property UpdateFlag As Boolean

    End Class

End Namespace
