Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel
Imports System.Threading

Namespace Forms
    Public Class ConnectingForm
        Inherits Form

        Private _lblStatus As Label
        Private _lblCountdown As Label
        Private _countdownTimer As System.Windows.Forms.Timer
        Private _worker As BackgroundWorker
        Private _secondsLeft As Integer = 60
        Private _isConnecting As Boolean = False
        Private _closing As Boolean = False
        Private Const TimeoutSeconds As Integer = 60

        Public Property Connected As Boolean = False
        Public Property ShouldRestart As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me._lblStatus = New Label()
            Me._lblCountdown = New Label()
            Me._countdownTimer = New System.Windows.Forms.Timer()
            Me._worker = New BackgroundWorker()
            Me.SuspendLayout()

            Me._lblStatus.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
            Me._lblStatus.Location = New Point(20, 15)
            Me._lblStatus.Name = "lblStatus"
            Me._lblStatus.Size = New Size(460, 30)
            Me._lblStatus.TabIndex = 0
            Me._lblStatus.Text = "Connecting to server. Please wait..."
            Me._lblStatus.TextAlign = ContentAlignment.MiddleCenter

            Me._lblCountdown.Font = New Font("Segoe UI", 36.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            Me._lblCountdown.ForeColor = Color.FromArgb(60, 60, 60)
            Me._lblCountdown.Location = New Point(20, 50)
            Me._lblCountdown.Name = "lblCountdown"
            Me._lblCountdown.Size = New Size(460, 60)
            Me._lblCountdown.TabIndex = 1
            Me._lblCountdown.Text = "60"
            Me._lblCountdown.TextAlign = ContentAlignment.MiddleCenter

            Me._countdownTimer.Interval = 1000

            Me._worker.WorkerSupportsCancellation = True
            AddHandler Me._worker.DoWork, AddressOf Worker_DoWork
            AddHandler Me._worker.RunWorkerCompleted, AddressOf Worker_Completed

            Me.AutoScaleDimensions = New SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(500, 120)
            Me.Controls.Add(Me._lblCountdown)
            Me.Controls.Add(Me._lblStatus)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ControlBox = False
            Me.Name = "ConnectingForm"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.Text = "Auto Update - Connecting"
            Me.TopMost = True

            AddHandler Me.Load, AddressOf ConnectingForm_Load
            AddHandler Me._countdownTimer.Tick, AddressOf CountdownTimer_Tick
            Me.ResumeLayout(False)
        End Sub

        Private Sub ConnectingForm_Load(sender As Object, e As EventArgs)
            _secondsLeft = TimeoutSeconds
            _lblCountdown.Text = _secondsLeft.ToString()
            _countdownTimer.Start()
            StartBackgroundConnect()
        End Sub

        Private Sub StartBackgroundConnect()
            If _closing Then Return
            If _worker.IsBusy Then Return
            _isConnecting = True
            _worker.RunWorkerAsync()
        End Sub

        Private Sub Worker_DoWork(sender As Object, e As DoWorkEventArgs)
            Config.AppSettings.Reload()

            Dim loaded As Boolean = Config.AppSettings.IsLoaded
            Dim isLocal As Boolean = Config.AppSettings.IsLocalConfigError
            Dim status As String = Config.AppSettings.LoadStatus

            e.Result = New String() {loaded.ToString(), isLocal.ToString(), status}
        End Sub

        Private Sub Worker_Completed(sender As Object, e As RunWorkerCompletedEventArgs)
            _isConnecting = False
            If _closing Then Return

            If e.Error IsNot Nothing Then
                Managers.LogManager.Info("Connection attempt error: " & e.Error.Message)
                Return
            End If

            Dim resultArr As String() = DirectCast(e.Result, String())
            Dim loaded As Boolean = Boolean.Parse(resultArr(0))
            Dim isLocal As Boolean = Boolean.Parse(resultArr(1))
            Dim status As String = resultArr(2)

            If loaded Then
                Connected = True
                _countdownTimer.Stop()
                Managers.LogManager.Info("Server config loaded successfully. Status: " & status)
                _closing = True
                Me.Close()
                Return
            End If

            If isLocal Then
                _countdownTimer.Stop()
                Managers.LogManager.Info("Local config error (will not retry): " & status)
                _closing = True
                Me.Close()
                Return
            End If

            Managers.LogManager.Info("Server connection failed: " & status)
        End Sub

        Private Sub CountdownTimer_Tick(sender As Object, e As EventArgs)
            _secondsLeft -= 1
            If _secondsLeft >= 0 Then
                _lblCountdown.Text = _secondsLeft.ToString()
            End If

            If Not Connected AndAlso Not _closing Then
                Try
                    If Config.AppSettings.IsLoaded Then
                        Connected = True
                        _countdownTimer.Stop()
                        Managers.LogManager.Info("Server config loaded (failsafe check). Closing connecting form.")
                        _closing = True
                        Me.Close()
                        Return
                    End If
                Catch
                End Try
            End If

            If _secondsLeft <= 0 Then
                _countdownTimer.Stop()
                If Connected Then Return

                Managers.LogManager.Info("Connection timeout after " & TimeoutSeconds.ToString() & "s. Will restart app.")
                ShouldRestart = True
                _closing = True
                Me.Close()
                Return
            End If

            If Not _isConnecting AndAlso Not Connected AndAlso Not _closing Then
                StartBackgroundConnect()
            End If
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Not _closing AndAlso Not Connected Then
                e.Cancel = True
                Return
            End If
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _closing = True
                If _countdownTimer IsNot Nothing Then
                    _countdownTimer.Stop()
                    RemoveHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
                    _countdownTimer.Dispose()
                    _countdownTimer = Nothing
                End If
                If _worker IsNot Nothing Then
                    RemoveHandler _worker.DoWork, AddressOf Worker_DoWork
                    RemoveHandler _worker.RunWorkerCompleted, AddressOf Worker_Completed
                    _worker.Dispose()
                    _worker = Nothing
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class
End Namespace
