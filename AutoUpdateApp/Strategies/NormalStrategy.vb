Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Strategies

    ''' <summary>
    ''' Normal mode: Prompts the user with three choices:
    ''' - Update Now → runs installer immediately
    ''' - Update After Restart → sets flag in updateflag.txt
    ''' - Remind Me Later → defers to next cycle
    ''' Uses Control.Invoke to show the dialog on the UI thread
    ''' when called from a BackgroundWorker.
    ''' </summary>
    Public Class NormalStrategy
        Implements IUpdateStrategy

        Private ReadOnly _invokeControl As Control

        ''' <summary>
        ''' Creates a NormalStrategy that can invoke UI on the given control's thread.
        ''' </summary>
        ''' <param name="invokeControl">A control (e.g. MainForm) for thread marshaling.</param>
        Public Sub New(invokeControl As Control)
            _invokeControl = invokeControl
        End Sub

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Dim choice As Forms.UpdatePromptResult = Forms.UpdatePromptResult.RemindLater

            Try
                ' Show the prompt on the UI thread
                If _invokeControl IsNot Nothing AndAlso _invokeControl.InvokeRequired Then
                    _invokeControl.Invoke(New MethodInvoker(Sub()
                        choice = ShowPrompt(context)
                    End Sub))
                Else
                    choice = ShowPrompt(context)
                End If
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to show update prompt.", ex)
                Return UpdateResult.[Error]
            End Try

            ' Act on user's choice
            Select Case choice
                Case Forms.UpdatePromptResult.UpdateNow
                    Managers.LogManager.Info("User chose: Update Now")
                    Dim success As Boolean = Managers.InstallerManager.RunInstaller(context.Tester.TesterType)
                    If success Then
                        Return UpdateResult.UpdateCompleted
                    Else
                        Return UpdateResult.[Error]
                    End If

                Case Forms.UpdatePromptResult.UpdateAfterRestart
                    Managers.LogManager.Info("User chose: Update After Restart")
                    Managers.UpdateFlagManager.SetFlag(context.Tester.ComputerName, True)
                    Return UpdateResult.UpdateScheduledForRestart

                Case Else
                    Managers.LogManager.Info("User chose: Remind Me Later")
                    Return UpdateResult.UpdateDeferred
            End Select
        End Function

        Private Function ShowPrompt(context As Models.UpdateContext) As Forms.UpdatePromptResult
            Using dlg As New Forms.UpdatePromptForm(context.CurrentVersion, context.LatestVersion)
                dlg.ShowDialog()
                Return dlg.UserChoice
            End Using
        End Function

    End Class

End Namespace
