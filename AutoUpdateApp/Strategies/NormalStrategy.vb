Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Strategies

    ''' <summary>
    ''' โหมด Normal: แสดงหน้าต่างให้ผู้ใช้เลือก 3 ตัวเลือก:
    ''' - อัปเดตตอนนี้ → รัน uninstall.bat/install.bat ทันที
    ''' - อัปเดตหลังรีสตาร์ท → ตั้ง Flag ใน updateflag.txt
    ''' - เตือนฉันทีหลัง → เลื่อนไปรอบถัดไป
    ''' ใช้ Control.Invoke เพื่อแสดงหน้าต่างบน UI Thread
    ''' </summary>
    Public Class NormalStrategy
        Implements IUpdateStrategy

        Private ReadOnly _invokeControl As Control

        ''' <summary>
        ''' สร้าง NormalStrategy พร้อม Control สำหรับเรียก UI Thread
        ''' </summary>
        Public Sub New(invokeControl As Control)
            _invokeControl = invokeControl
        End Sub

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Dim choice As Forms.UpdatePromptResult = Forms.UpdatePromptResult.RemindLater

            Try
                ' แสดงหน้าต่างบน UI Thread
                If _invokeControl IsNot Nothing AndAlso _invokeControl.IsHandleCreated AndAlso _invokeControl.InvokeRequired Then
                    _invokeControl.Invoke(New MethodInvoker(Sub()
                        choice = ShowPrompt(context)
                    End Sub))
                ElseIf _invokeControl IsNot Nothing AndAlso _invokeControl.IsHandleCreated Then
                    choice = ShowPrompt(context)
                Else
                    ' ไม่มี Control สำหรับแสดง UI → ลองแสดงตรงๆ
                    Managers.LogManager.Warn("InvokeControl not ready. Showing prompt directly.")
                    choice = ShowPrompt(context)
                End If
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to show update prompt.", ex)
                Return UpdateResult.[Error]
            End Try

            ' ดำเนินการตามที่ผู้ใช้เลือก
            Select Case choice
                Case Forms.UpdatePromptResult.UpdateNow
                    Managers.LogManager.Info("User chose: Update Now")
                    Dim success As Boolean = Managers.InstallerManager.RunInstaller(context.Tester.TesterType)
                    If success Then
                        ' ล้าง Flag เผื่อมีค้างอยู่
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
