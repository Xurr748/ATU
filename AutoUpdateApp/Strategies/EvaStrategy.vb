Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' โหมด EVA: Standby เท่านั้น
    ''' ไม่มีการอัปเดตอัตโนมัติ
    ''' ใช้สำหรับการติดตั้งและทดสอบด้วยตนเอง
    ''' บันทึกความแตกต่างของเวอร์ชันแล้วคืนค่า NoAction
    ''' </summary>
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
