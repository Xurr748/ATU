Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' Result of an update strategy execution.
    ''' </summary>
    Public Enum UpdateResult
        ''' <summary>No action was taken (up to date, standby, or time not reached).</summary>
        NoAction = 0
        ''' <summary>Update was installed successfully.</summary>
        UpdateCompleted = 1
        ''' <summary>User chose "Remind Me Later" — will retry next cycle.</summary>
        UpdateDeferred = 2
        ''' <summary>User chose "Update After Restart" — flag was set.</summary>
        UpdateScheduledForRestart = 3
        ''' <summary>An error occurred during the update process.</summary>
        [Error] = 4
    End Enum

    ''' <summary>
    ''' Strategy interface for mode-specific update behavior.
    ''' Implementations: EvaStrategy, NormalStrategy, AutoStrategy.
    ''' </summary>
    Public Interface IUpdateStrategy

        ''' <summary>
        ''' Executes the update logic for the given context.
        ''' </summary>
        ''' <param name="context">All data needed for the update decision.</param>
        ''' <returns>The result of the update attempt.</returns>
        Function Execute(context As Models.UpdateContext) As UpdateResult

    End Interface

End Namespace
