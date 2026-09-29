Option Strict On
Option Explicit On

Namespace Strategies

    Public Class EvaStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Managers.LogManager.Info("EVA mode: Standby")
            Return UpdateResult.NoAction
        End Function

    End Class

End Namespace
