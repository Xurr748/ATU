Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel

Namespace Forms
    Public Class ConnectingForm
        Inherits Form

        Private _lblStatus As Label
        Private _lblDetail As Label
        Private _progressBar As ProgressBar
        Private _retryTimer As Timer
        Private _attemptCount As Integer = 0
        Private Const RetryIntervalMs As Integer = 5000
        Private Const MaxAttempts As Integer = 0

        Public Property Connected As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me._lblStatus = New Label()
            Me._lblDetail = New Label()
            Me._progressBar = New ProgressBar()
            Me._retryTimer = New Timer()
            Me.SuspendLayout()

            Me._lblStatus.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
            Me._lblStatus.Location = New Point(20, 15)
            Me._lblStatus.Name = "lblStatus"
            Me._lblStatus.Size = New Size(460, 30)
            Me._lblStatus.TabIndex = 0
            Me._lblStatus.Text = "Connecting to server. Please wait..."
            Me._lblStatus.TextAlign = ContentAlignment.MiddleCenter

            Me._progressBar.Location = New Point(20, 55)
            Me._progressBar.Name = "progressBar"
            Me._progressBar.Size = New Size(460, 25)
            Me._progressBar.Style = ProgressBarStyle.Marquee
            Me._progressBar.MarqueeAnimationSpeed = 30
            Me._progressBar.TabIndex = 1

            Me._lblDetail.Font = New Font("Segoe UI", 8.5!, FontStyle.Italic, GraphicsUnit.Point, CType(0, Byte))
            Me._lblDetail.ForeColor = Color.FromArgb(100, 100, 100)
            Me._lblDetail.Location = New Point(20, 88)
            Me._lblDetail.Name = "lblDetail"
            Me._lblDetail.Size = New Size(460, 20)
            Me._lblDetail.TabIndex = 2
            Me._lblDetail.Text = "Waiting for server..."
            Me._lblDetail.TextAlign = ContentAlignment.MiddleCenter

            Me._retryTimer.Interval = RetryIntervalMs

            Me.AutoScaleDimensions = New SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(500, 120)
            Me.Controls.Add(Me._progressBar)
            Me.Controls.Add(Me._lblStatus)
            Me.Controls.Add(Me._lblDetail)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ControlBox = False
            Me.Name = "ConnectingForm"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.Text = "Auto Update - Connecting"
            Me.TopMost = True

            AddHandler Me.Load, AddressOf ConnectingForm_Load
            AddHandler Me._retryTimer.Tick, AddressOf RetryTimer_Tick
            Me.ResumeLayout(False)
        End Sub

        Private Sub ConnectingForm_Load(sender As Object, e As EventArgs)
            TryConnect()
        End Sub

        Private Sub TryConnect()
            _attemptCount += 1
            _lblDetail.Text = "Attempt " & _attemptCount.ToString() & "..."

            Config.AppSettings.Reload()

            If Config.AppSettings.IsLoaded Then
                Connected = True
                _retryTimer.Stop()
                Managers.LogManager.Info("Server config loaded on attempt " & _attemptCount.ToString())
                Me.Close()
            ElseIf Config.AppSettings.IsLocalConfigError Then
                _retryTimer.Stop()
                Managers.LogManager.Info("Local config error (will not retry): " & Config.AppSettings.LoadStatus)
                Me.Close()
            Else
                Dim status As String = Config.AppSettings.LoadStatus
                Managers.LogManager.Info("Server connection attempt " & _attemptCount.ToString() & " failed: " & status)
                _lblDetail.Text = "Attempt " & _attemptCount.ToString() & " - Retrying in " & (RetryIntervalMs \ 1000).ToString() & "s..."
                _retryTimer.Start()
            End If
        End Sub

        Private Sub RetryTimer_Tick(sender As Object, e As EventArgs)
            _retryTimer.Stop()
            TryConnect()
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Not Connected AndAlso e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Return
            End If
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _retryTimer IsNot Nothing Then
                    _retryTimer.Stop()
                    RemoveHandler _retryTimer.Tick, AddressOf RetryTimer_Tick
                    _retryTimer.Dispose()
                    _retryTimer = Nothing
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class
End Namespace
