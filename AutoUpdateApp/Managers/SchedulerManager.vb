Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Managers

    ''' <summary>
    ''' ตัวตั้งเวลาตรวจสอบ ใช้ System.Windows.Forms.Timer
    ''' ทำงานบน UI Thread — ปลอดภัยสำหรับการโต้ตอบกับ Form
    ''' ระยะเวลาตรวจสอบตั้งค่าได้ผ่าน app.config
    ''' </summary>
    Public Class SchedulerManager
        Implements IDisposable

        Private _timer As Timer
        Private _disposed As Boolean

        ''' <summary>เกิดขึ้นทุกครั้งที่ Timer ครบรอบ (UI Thread)</summary>
        Public Event TickFired As EventHandler

        ''' <summary>
        ''' เริ่มการทำงานของตัวตั้งเวลาด้วยระยะเวลาที่ตั้งค่าไว้
        ''' ไม่ทำอะไรหากกำลังทำงานอยู่แล้ว
        ''' </summary>
        Public Sub Start()
            If _timer IsNot Nothing Then
                Return
            End If

            Dim intervalMs As Integer = Config.AppSettings.PollingIntervalMinutes * 60 * 1000

            ' ป้องกันค่าระยะเวลาเป็น 0 หรือติดลบ
            If intervalMs <= 0 Then
                intervalMs = 3600000 ' ค่าเริ่มต้น 1 ชั่วโมง
            End If

            _timer = New Timer()
            _timer.Interval = intervalMs
            AddHandler _timer.Tick, AddressOf OnTimerTick
            _timer.Start()

            LogManager.Info("Scheduler started. Interval: " & Config.AppSettings.PollingIntervalMinutes.ToString() & " minutes.")

            ' ยิง Tick ครั้งแรกทันทีไม่ต้องรอ
            OnTimerTick(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' หยุดตัวตั้งเวลาและปล่อย Timer
        ''' </summary>
        Public Sub [Stop]()
            If _timer IsNot Nothing Then
                _timer.Stop()
                RemoveHandler _timer.Tick, AddressOf OnTimerTick
                _timer.Dispose()
                _timer = Nothing
                LogManager.Info("Scheduler stopped.")
            End If
        End Sub

        ''' <summary>
        ''' คืนค่า True หากตัวตั้งเวลากำลังทำงานอยู่
        ''' </summary>
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _timer IsNot Nothing AndAlso _timer.Enabled
            End Get
        End Property

        Private Sub OnTimerTick(sender As Object, e As EventArgs)
            If _timer IsNot Nothing Then _timer.Stop()
            Try
                RaiseEvent TickFired(Me, EventArgs.Empty)
            Finally
                If _timer IsNot Nothing AndAlso Not _disposed Then
                    _timer.Start()
                End If
            End Try
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    [Stop]()
                End If
                _disposed = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
