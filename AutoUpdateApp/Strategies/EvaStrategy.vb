Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' EVA mode: Standby only.
    ''' No automatic update action is performed.
    ''' Used for manual installation and testing by the user.
    ''' Logs the version difference and returns NoAction.
    ''' </summary>
    Public Class EvaStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Managers.LogManager.Info( _
                "EVA mode (standby) — No automatic action for " & context.Tester.ComputerName & _
                ". Current: " & context.CurrentVersion & _
                ", Latest: " & context.LatestVersion)
            Return UpdateResult.NoAction
        End Function

    End Class

End Namespace
