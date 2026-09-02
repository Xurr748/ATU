Option Strict On
Option Explicit On

Namespace Strategies

    Public Class AutoStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            If context Is Nothing OrElse context.Tester Is Nothing Then
                Managers.LogManager.[Error]("AutoStrategy: Invalid or null UpdateContext/Tester.")
                Return UpdateResult.[Error]
            End If

            Dim computerName As String = context.Tester.ComputerName

            Managers.LogManager.Info( _
                "Auto mode — Setting update flag for restart for " & computerName & _
                ". Current: " & If(context.CurrentVersion, "N/A") & " -> Latest: " & If(context.LatestVersion, "N/A"))

            Try
                Managers.UpdateFlagManager.SetFlag(computerName, True)
                Managers.LogManager.Info("Auto update flag set successfully.")
                Return UpdateResult.UpdateScheduledForRestart
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to set update flag in Auto mode for " & computerName, ex)
                Return UpdateResult.[Error]
            End Try
        End Function

    End Class

End Namespace
