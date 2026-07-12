Option Strict On
Option Explicit On

Namespace Strategies

    ''' <summary>
    ''' ผลลัพธ์ของการทำงานของ Strategy การอัปเดต
    ''' </summary>
    Public Enum UpdateResult
        ''' <summary>ไม่มีการดำเนินการ (เวอร์ชันตรงกัน, โหมด Standby, หรือยังไม่ถึงเวลา)</summary>
        NoAction = 0
        ''' <summary>ติดตั้งอัปเดตสำเร็จ</summary>
        UpdateCompleted = 1
        ''' <summary>ผู้ใช้เลือก "เตือนฉันทีหลัง" — จะลองใหม่รอบหน้า</summary>
        UpdateDeferred = 2
        ''' <summary>ผู้ใช้เลือก "อัปเดตหลังรีสตาร์ท" — ตั้ง Flag ไว้แล้ว</summary>
        UpdateScheduledForRestart = 3
        ''' <summary>เกิดข้อผิดพลาดระหว่างกระบวนการอัปเดต</summary>
        [Error] = 4
    End Enum

    ''' <summary>
    ''' Interface สำหรับ Strategy การอัปเดตตามโหมด
    ''' Implementations: EvaStrategy, NormalStrategy, AutoStrategy.
    ''' </summary>
    Public Interface IUpdateStrategy

        ''' <summary>
        ''' ดำเนินการตรรกะการอัปเดตสำหรับ Context ที่ให้มา
        ''' </summary>
        ''' <param name="context">All data needed for the update decision.</param>
        ''' <returns>The result of the update attempt.</returns>
        Function Execute(context As Models.UpdateContext) As UpdateResult

    End Interface

End Namespace
