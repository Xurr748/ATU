Option Strict On
Option Explicit On

Namespace Strategies

    Public Class AutoStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Managers.LogManager.Info( _
                "Auto mode — Setting update flag for restart for " & context.Tester.ComputerName & _
                ". Current: " & context.CurrentVersion & " → Latest: " & context.LatestVersion)

            Try
                Managers.UpdateFlagManager.SetFlag(context.Tester.ComputerName, True)
                Managers.LogManager.Info("Auto update flag set successfully.")
                Return UpdateResult.UpdateScheduledForRestart
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to set update flag in Auto mode for " & context.Tester.ComputerName, ex)
                Return UpdateResult.[Error]
            End Try
        End Function

    End Class

End Namespace
