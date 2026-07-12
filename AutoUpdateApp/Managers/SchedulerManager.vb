Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Managers

    ''' <summary>
    ''' Timer-based scheduler using System.Windows.Forms.Timer.
    ''' Fires on the UI thread — safe for form interactions.
    ''' Polling interval is configurable via app.config.
    ''' </summary>
    Public Class SchedulerManager
        Implements IDisposable

        Private _timer As Timer
        Private _disposed As Boolean

        ''' <summary>Raised on each timer tick (UI thread).</summary>
        Public Event TickFired As EventHandler

        ''' <summary>
        ''' Starts the scheduler with the configured polling interval.
        ''' No-op if already running.
        ''' </summary>
        Public Sub Start()
            If _timer IsNot Nothing Then
                Return
            End If

            Dim intervalMs As Integer = Config.AppSettings.PollingIntervalMinutes * 60 * 1000

            ' Guard against zero or negative interval
            If intervalMs <= 0 Then
                intervalMs = 3600000 ' Default 1 hour
            End If

            _timer = New Timer()
            _timer.Interval = intervalMs
            AddHandler _timer.Tick, AddressOf OnTimerTick
            _timer.Start()

            LogManager.Info("Scheduler started. Interval: " & Config.AppSettings.PollingIntervalMinutes.ToString() & " minutes.")
        End Sub

        ''' <summary>
        ''' Stops the scheduler and releases the timer.
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
        ''' Returns True if the scheduler is currently running.
        ''' </summary>
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _timer IsNot Nothing AndAlso _timer.Enabled
            End Get
        End Property

        Private Sub OnTimerTick(sender As Object, e As EventArgs)
            RaiseEvent TickFired(Me, EventArgs.Empty)
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
