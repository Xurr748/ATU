Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' Auto mode: Automatically runs the installer when the scheduled time
    ''' is reached and versions differ. No user interaction.
    ''' </summary>
    Public Class AutoStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Managers.LogManager.Info( _
                "Auto mode — Starting automatic update for " & context.Tester.ComputerName & _
                ". Current: " & context.CurrentVersion & " → Latest: " & context.LatestVersion)

            Dim success As Boolean = Managers.InstallerManager.RunInstaller(context.Tester.TesterType)

            If success Then
                Managers.LogManager.Info("Auto update completed successfully.")
                Return UpdateResult.UpdateCompleted
            Else
                Managers.LogManager.[Error]("Auto update failed for " & context.Tester.ComputerName)
                Return UpdateResult.[Error]
            End If
        End Function

    End Class

End Namespace
