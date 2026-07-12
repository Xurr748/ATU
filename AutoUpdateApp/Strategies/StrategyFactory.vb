Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Strategies

    ''' <summary>
    ''' Factory for creating mode-specific update strategies.
    ''' Maps mode strings (EVA, Normal, Auto) to strategy implementations.
    ''' Unknown modes default to EVA (standby) for safety.
    ''' </summary>
    Public NotInheritable Class StrategyFactory

        Private Sub New()
            ' Static-only class
        End Sub

        ''' <summary>
        ''' Creates the appropriate strategy for the given mode.
        ''' </summary>
        ''' <param name="mode">Mode string from TesterType.csv (EVA, Normal, Auto).</param>
        ''' <param name="invokeControl">Control for UI thread marshaling (needed for Normal mode).</param>
        Public Shared Function Create(mode As String, Optional invokeControl As Control = Nothing) As IUpdateStrategy
            Select Case mode.ToUpperInvariant()
                Case "EVA"
                    Return New EvaStrategy()
                Case "NORMAL"
                    Return New NormalStrategy(invokeControl)
                Case "AUTO"
                    Return New AutoStrategy()
                Case Else
                    Managers.LogManager.Warn("Unknown mode: " & mode & ". Defaulting to EVA (standby).")
                    Return New EvaStrategy()
            End Select
        End Function

    End Class

End Namespace
