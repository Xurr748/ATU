Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' โหมด Auto: รัน uninstall.bat/install.bat อัตโนมัติเมื่อถึงเวลาที่กำหนดและเวอร์ชันไม่ตรงกัน
    ''' ไม่ต้องมีการโต้ตอบกับผู้ใช้
    ''' หลังติดตั้งสำเร็จจะล้าง Flag ใน updateflag.txt อัตโนมัติ
    ''' </summary>
    Public Class AutoStrategy
        Implements IUpdateStrategy

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            Managers.LogManager.Info( _
                "Auto mode — Starting automatic update for " & context.Tester.ComputerName & _
                ". Current: " & context.CurrentVersion & " → Latest: " & context.LatestVersion)

            Dim success As Boolean = Managers.InstallerManager.RunInstaller(context.Tester.TesterType)

            If success Then
                ' ล้าง Flag เผื่อมีค้างอยู่จากการกด "อัปเดตหลังรีสตาร์ท"
                Managers.UpdateFlagManager.SetFlag(context.Tester.ComputerName, False)
                Managers.LogManager.Info("Auto update completed successfully.")
                Return UpdateResult.UpdateCompleted
            Else
                Managers.LogManager.[Error]("Auto update failed for " & context.Tester.ComputerName)
                Return UpdateResult.[Error]
            End If
        End Function

    End Class

End Namespace
