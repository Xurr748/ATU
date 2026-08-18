Option Strict On
Option Explicit On

Namespace Strategies

    Public Enum UpdateResult
        NoAction = 0
        UpdateCompleted = 1
        UpdateDeferred = 2
        UpdateScheduledForRestart = 3
        [Error] = 4
    End Enum

    Public Interface IUpdateStrategy

        Function Execute(context As Models.UpdateContext) As UpdateResult

    End Interface

End Namespace
