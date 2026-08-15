Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' หน้าต่างนับถอยหลังรีสตาร์ท (40% ของหน้าจอ)
    ''' - แสดงตัวเลขสีแดงขนาดใหญ่
    ''' - รีสตาร์ทอัตโนมัติเมื่อครบ 0
    ''' </summary>
    Public Class RestartCountdownForm
        Inherits Form

        Private _lblHeader As Label
        Private _lblCountdown As Label
        Private _btnCancel As Button
        Private WithEvents _countdownTimer As Timer
        Private _secondsLeft As Integer = 60
        Private _isRestarting As Boolean = False
        Private _parentForm As Form

        Public Sub New(parentForm As Form)
            _parentForm = parentForm
            InitUI()
        End Sub

        Private Sub InitUI()
            Me.SuspendLayout()
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText

            ' ── คำนวณขนาด 40% ของหน้าจอ (ปรับให้เหมาะกับ 1280x1024) ──
            Dim screen As Rectangle = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea
            Dim formW As Integer = CInt(screen.Width * 0.4)
            Dim formH As Integer = CInt(screen.Height * 0.4)

            ' กำหนดขนาดขั้นต่ำ/สูงสุด
            If formW < 400 Then formW = 400
            If formH < 300 Then formH = 300
            If formW > 600 Then formW = 600
            If formH > 450 Then formH = 450

            Me.Text = L("RestartNoticeTitle")
            Me.Size = New Size(formW, formH)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ControlBox = False ' ปิดปุ่ม X (บังคับให้กดปุ่มยกเลิกแทน)
            Me.TopMost = True
            Me.ShowInTaskbar = True
            Me.BackColor = Color.FromArgb(20, 20, 25)
            Me.Font = New Font("Segoe UI", 10.0F)

            Dim centerY As Integer = CInt(formH * 0.08)

            ' ── ข้อความด้านบน ──
            _lblHeader = New Label()
            _lblHeader.Text = L("RestartNoticeCountdown").Replace("{0}", "")
            _lblHeader.Font = New Font("Segoe UI", 14.0F, FontStyle.Bold)
            _lblHeader.ForeColor = Color.White
            _lblHeader.TextAlign = ContentAlignment.MiddleCenter
            _lblHeader.AutoSize = False
            _lblHeader.Size = New Size(formW - 40, 40)
            _lblHeader.Location = New Point(20, centerY)

            ' ── ตัวเลข countdown สีแดงตัวใหญ่ ──
            _lblCountdown = New Label()
            _lblCountdown.Text = _secondsLeft.ToString()
            _lblCountdown.Font = New Font("Segoe UI", 72.0F, FontStyle.Bold)
            _lblCountdown.ForeColor = Color.FromArgb(255, 70, 70)
            _lblCountdown.TextAlign = ContentAlignment.MiddleCenter
            _lblCountdown.AutoSize = False
            _lblCountdown.Size = New Size(formW - 40, 120)
            _lblCountdown.Location = New Point(20, centerY + 45)

            ' ── ปุ่ม Cancel ──
            _btnCancel = New Button()
            _btnCancel.Text = L("RestartNoticeBtnCancel")
            _btnCancel.Font = New Font("Segoe UI", 12.0F)
            _btnCancel.ForeColor = Color.White
            _btnCancel.BackColor = Color.FromArgb(80, 80, 95)
            _btnCancel.FlatStyle = FlatStyle.Flat
            _btnCancel.FlatAppearance.BorderSize = 1
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 140)
            _btnCancel.Size = New Size(220, 45)
            _btnCancel.Location = New Point(CInt((formW - 220) / 2), centerY + 185)
            _btnCancel.Cursor = Cursors.Hand
            AddHandler _btnCancel.Click, AddressOf BtnCancel_Click
            AddHandler _btnCancel.MouseEnter, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(100, 100, 115)
            AddHandler _btnCancel.MouseLeave, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(80, 80, 95)

            Me.Controls.Add(_lblHeader)
            Me.Controls.Add(_lblCountdown)
            Me.Controls.Add(_btnCancel)

            ' ── Timer: Countdown 1 วินาที ──
            _countdownTimer = New Timer()
            _countdownTimer.Interval = 1000
            AddHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
            _countdownTimer.Start()

            Me.ResumeLayout(False)
        End Sub

        Private Sub CountdownTimer_Tick(sender As Object, e As EventArgs)
            _secondsLeft -= 1
            _lblCountdown.Text = _secondsLeft.ToString()
            
            If _secondsLeft <= 0 Then
                _countdownTimer.Stop()
                DoRestart()
            End If
        End Sub

        Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
            _countdownTimer.Stop()
            Managers.LogManager.Info("User cancelled restart countdown from CountdownForm.")
            
            ' เรียกฟอร์มหน้าต่างเก่ากลับมาและซ่อนไว้เบื้องหลัง (หลอกว่าปิด)
            If _parentForm IsNot Nothing Then
                _parentForm.Show()
                _parentForm.Hide() ' ซ่อนอีกครั้งเพื่อให้ Trigger PopupTimer
                If TypeOf _parentForm Is RestartNoticeForm Then
                    DirectCast(_parentForm, RestartNoticeForm).ResumePopupTimer()
                End If
            End If
            Me.Close()
        End Sub

        Private Sub DoRestart()
            _isRestarting = True
            Try
                Managers.LogManager.Info("Initiating restart (countdown expired in CountdownForm).")
                ' ใช้ /r (restart), /f (force), /t 0 (ทันที)
                Diagnostics.Process.Start("shutdown", "/r /f /t 0")
            Catch ex As Exception
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Managers.LogManager.[Error]("Failed to initiate restart: " & ex.Message)
                MessageBox.Show(L("CantRestart") & ex.Message, L("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
                _isRestarting = False
                
                If _parentForm IsNot Nothing Then
                    _parentForm.Show()
                End If
            End Try
            Me.Close()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _countdownTimer IsNot Nothing Then
                    _countdownTimer.Stop()
                    RemoveHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
                    _countdownTimer.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
