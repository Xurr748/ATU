Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Strategies

    Public NotInheritable Class StrategyFactory

        Private Sub New()
        End Sub

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
