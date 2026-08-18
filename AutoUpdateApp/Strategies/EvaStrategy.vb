Option Strict On
Option Explicit On

Namespace Strategies

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
