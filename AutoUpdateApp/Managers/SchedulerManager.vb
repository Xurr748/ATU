Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Managers

    Public Class SchedulerManager
        Implements IDisposable

        Private _timer As Timer
        Private _disposed As Boolean

        Public Event TickFired As EventHandler

        Public Sub Start()
            If _timer IsNot Nothing Then
                Return
            End If

            Dim intervalMs As Integer = Config.AppSettings.PollingIntervalMinutes * 60 * 1000

            ' Check schedule at least every 30 seconds so scheduled updates trigger on time without waiting up to 60 minutes
            If intervalMs <= 0 OrElse intervalMs > 30000 Then
                intervalMs = 30000
            End If

            _timer = New Timer()
            _timer.Interval = intervalMs
            AddHandler _timer.Tick, AddressOf OnTimerTick
            _timer.Start()
            ' NOTE: No immediate first-tick here — UpdateWorker runs after the
            ' configured PollingIntervalMinutes elapses. Use the Check button for
            ' on-demand runs, or the AutoModeTimer for AUTO mode polling.
        End Sub

        Public Sub [Stop]()
            If _timer IsNot Nothing Then
                _timer.Stop()
                RemoveHandler _timer.Tick, AddressOf OnTimerTick
                _timer.Dispose()
                _timer = Nothing
            End If
        End Sub

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
