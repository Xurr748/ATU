Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Strategies

    Public Class NormalStrategy
        Implements IUpdateStrategy

        Private ReadOnly _invokeControl As Control

        Public Sub New(invokeControl As Control)
            _invokeControl = invokeControl
        End Sub

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Dim choice As Forms.UpdatePromptResult = Forms.UpdatePromptResult.RemindLater

            Try
                If _invokeControl IsNot Nothing AndAlso _invokeControl.IsHandleCreated AndAlso _invokeControl.InvokeRequired Then
                    _invokeControl.Invoke(New MethodInvoker(Sub()
                        choice = ShowPrompt(context)
                    End Sub))
                ElseIf _invokeControl IsNot Nothing AndAlso _invokeControl.IsHandleCreated Then
                    choice = ShowPrompt(context)
                Else
                    Managers.LogManager.[Error]("InvokeControl not ready. Cannot show prompt on background thread.")
                    Return UpdateResult.[Error]
                End If
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to show update prompt.", ex)
                Return UpdateResult.[Error]
            End Try

            Select Case choice
                Case Forms.UpdatePromptResult.UpdateNow
                    Managers.LogManager.Info("User chose: Update Now")
                    Dim success As Boolean = Managers.InstallerManager.RunInstaller(context.Tester.TesterType, _
                        Sub(percent, msg)
                            If _invokeControl IsNot Nothing AndAlso _invokeControl.IsHandleCreated Then
                                Dim mainForm = TryCast(_invokeControl, Forms.MainForm)
                                If mainForm IsNot Nothing Then
                                    mainForm.UpdateProgressSafe(percent, msg)
                                End If
                            End If
                        End Sub)
                    If success Then
                        Managers.UpdateFlagManager.SetFlag(context.Tester.ComputerName, False)
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
