Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' หน้าต่างแจ้งเตือนรีสตาร์ทขนาดใหญ่ (60% ของหน้าจอ)
    ''' - ปิดปุ่ม Close (X) และ Maximize
    ''' - เหลือแค่ปุ่ม Minimize
    ''' - ถ้ายกลงไปจะเด้งขึ้นมาใหม่ใน 20 วินาที
    ''' - Countdown 60 วินาที ถ้าไม่กดจะ restart อัตโนมัติ
    ''' </summary>
    Public Class RestartNoticeForm
        Inherits Form

        Private _lblIcon As Label
        Private _lblHeader As Label
        Private _lblBody As Label
        Private _lblCountdown As Label
        Private _lblWarn As Label
        Private _btnRestart As Button
        Private _btnCancel As Button
        Private WithEvents _popupTimer As Timer
        Private WithEvents _countdownTimer As Timer
        Private _secondsLeft As Integer = 60
        Private _isRestarting As Boolean = False

        Public Sub New()
            InitUI()
        End Sub

        Private Sub InitUI()
            Me.SuspendLayout()

            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText

            ' ── คำนวณขนาด 60% ของหน้าจอ ──
            Dim screen As Rectangle = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea
            Dim formW As Integer = CInt(screen.Width * 0.6)
            Dim formH As Integer = CInt(screen.Height * 0.6)

            ' ── Form Settings ──
            Me.Text = L("RestartNoticeTitle")
            Me.Size = New Size(formW, formH)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.MinimizeBox = True
            Me.TopMost = True
            Me.ShowInTaskbar = True
            Me.BackColor = Color.FromArgb(25, 25, 35)
            Me.Font = New Font("Segoe UI", 10.0F)

            Dim centerY As Integer = CInt(formH * 0.06)

            ' ── ⚠ ไอคอนเตือน ──
            _lblIcon = New Label()
            _lblIcon.Text = "⚠"
            _lblIcon.Font = New Font("Segoe UI Emoji", 64.0F)
            _lblIcon.ForeColor = Color.FromArgb(255, 193, 7)
            _lblIcon.TextAlign = ContentAlignment.MiddleCenter
            _lblIcon.AutoSize = False
            _lblIcon.Size = New Size(formW - 40, 110)
            _lblIcon.Location = New Point(20, centerY)

            ' ── หัวข้อขนาดใหญ่ ──
            _lblHeader = New Label()
            _lblHeader.Text = L("RestartNoticeHeader")
            _lblHeader.Font = New Font("Segoe UI", 30.0F, FontStyle.Bold)
            _lblHeader.ForeColor = Color.White
            _lblHeader.TextAlign = ContentAlignment.MiddleCenter
            _lblHeader.AutoSize = False
            _lblHeader.Size = New Size(formW - 40, 70)
            _lblHeader.Location = New Point(20, centerY + 115)

            ' ── เนื้อหา ──
            _lblBody = New Label()
            _lblBody.Text = L("RestartNoticeBody")
            _lblBody.Font = New Font("Segoe UI", 14.0F)
            _lblBody.ForeColor = Color.FromArgb(200, 200, 210)
            _lblBody.TextAlign = ContentAlignment.MiddleCenter
            _lblBody.AutoSize = False
            _lblBody.Size = New Size(formW - 80, 120)
            _lblBody.Location = New Point(40, centerY + 195)

            ' ── ตัวนับถอยหลัง (ตัวเลขใหญ่) ──
            _lblCountdown = New Label()
            _lblCountdown.Text = L("RestartNoticeCountdown").Replace("{0}", _secondsLeft.ToString())
            _lblCountdown.Font = New Font("Segoe UI", 22.0F, FontStyle.Bold)
            _lblCountdown.ForeColor = Color.FromArgb(255, 100, 100)
            _lblCountdown.TextAlign = ContentAlignment.MiddleCenter
            _lblCountdown.AutoSize = False
            _lblCountdown.Size = New Size(formW - 40, 50)
            _lblCountdown.Location = New Point(20, centerY + 325)

            ' ── ปุ่ม Restart Now ──
            Dim btnW As Integer = 220
            Dim btnGap As Integer = 20
            Dim totalBtnW As Integer = btnW * 2 + btnGap
            Dim btnStartX As Integer = CInt((formW - totalBtnW) / 2)

            _btnRestart = New Button()
            _btnRestart.Text = L("RestartNoticeBtn")
            _btnRestart.Font = New Font("Segoe UI", 14.0F, FontStyle.Bold)
            _btnRestart.ForeColor = Color.White
            _btnRestart.BackColor = Color.FromArgb(220, 53, 69)
            _btnRestart.FlatStyle = FlatStyle.Flat
            _btnRestart.FlatAppearance.BorderSize = 0
            _btnRestart.Size = New Size(btnW, 55)
            _btnRestart.Location = New Point(btnStartX, centerY + 390)
            _btnRestart.Cursor = Cursors.Hand
            AddHandler _btnRestart.Click, AddressOf BtnRestart_Click
            AddHandler _btnRestart.MouseEnter, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(200, 35, 51)
            AddHandler _btnRestart.MouseLeave, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(220, 53, 69)

            ' ── ปุ่ม Cancel (ยกเลิก countdown) ──
            _btnCancel = New Button()
            _btnCancel.Text = L("RestartNoticeBtnCancel")
            _btnCancel.Font = New Font("Segoe UI", 14.0F)
            _btnCancel.ForeColor = Color.White
            _btnCancel.BackColor = Color.FromArgb(80, 80, 95)
            _btnCancel.FlatStyle = FlatStyle.Flat
            _btnCancel.FlatAppearance.BorderSize = 1
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 140)
            _btnCancel.Size = New Size(btnW, 55)
            _btnCancel.Location = New Point(btnStartX + btnW + btnGap, centerY + 390)
            _btnCancel.Cursor = Cursors.Hand
            AddHandler _btnCancel.Click, AddressOf BtnCancel_Click
            AddHandler _btnCancel.MouseEnter, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(100, 100, 115)
            AddHandler _btnCancel.MouseLeave, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(80, 80, 95)

            ' ── คำเตือนเมื่อ Minimize ──
            _lblWarn = New Label()
            _lblWarn.Text = L("RestartNoticeMinimizeWarn")
            _lblWarn.Font = New Font("Segoe UI", 10.0F, FontStyle.Italic)
            _lblWarn.ForeColor = Color.FromArgb(150, 150, 160)
            _lblWarn.TextAlign = ContentAlignment.MiddleCenter
            _lblWarn.AutoSize = False
            _lblWarn.Size = New Size(formW - 40, 30)
            _lblWarn.Location = New Point(20, centerY + 460)

            Me.Controls.Add(_lblIcon)
            Me.Controls.Add(_lblHeader)
            Me.Controls.Add(_lblBody)
            Me.Controls.Add(_lblCountdown)
            Me.Controls.Add(_btnRestart)
            Me.Controls.Add(_btnCancel)
            Me.Controls.Add(_lblWarn)

            ' ── Timer: เด้งขึ้นมาใหม่ทุก 20 วินาที ──
            _popupTimer = New Timer()
            _popupTimer.Interval = 20000
            AddHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
            _popupTimer.Start()

            ' ── Timer: Countdown 1 วินาที ──
            _countdownTimer = New Timer()
            _countdownTimer.Interval = 1000
            AddHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
            _countdownTimer.Start()

            Me.ResumeLayout(False)
        End Sub

        ''' <summary>
        ''' ป้องกันการกดปิดหน้าต่าง (X) — ย่อแทน
        ''' </summary>
        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Not _isRestarting AndAlso e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.WindowState = FormWindowState.Minimized
            End If
            MyBase.OnFormClosing(e)
        End Sub

        ''' <summary>
        ''' เมื่อ user กดย่อ แล้วไม่ restart ภายใน 20 วินาที → เด้งขึ้นมาใหม่
        ''' </summary>
        Private Sub PopupTimer_Tick(sender As Object, e As EventArgs)
            If Me.WindowState = FormWindowState.Minimized Then
                Me.WindowState = FormWindowState.Normal
                Me.TopMost = True
                Me.BringToFront()
                Me.Activate()
            End If
        End Sub

        ''' <summary>
        ''' นับถอยหลังทุก 1 วินาที — เมื่อหมดเวลา auto restart
        ''' </summary>
        Private Sub CountdownTimer_Tick(sender As Object, e As EventArgs)
            _secondsLeft -= 1

            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            If _lblCountdown IsNot Nothing Then
                _lblCountdown.Text = L("RestartNoticeCountdown").Replace("{0}", _secondsLeft.ToString())
            End If

            ' ── อัปเดตข้อความบนปุ่ม Restart ──
            If _btnRestart IsNot Nothing Then
                _btnRestart.Text = L("RestartNoticeBtn") & " (" & _secondsLeft.ToString() & ")"
            End If

            If _secondsLeft <= 0 Then
                ' หมดเวลา → restart อัตโนมัติ
                _countdownTimer.Stop()
                DoRestart()
            End If
        End Sub

        ''' <summary>
        ''' กดปุ่ม Restart Now → restart ทันที
        ''' </summary>
        Private Sub BtnRestart_Click(sender As Object, e As EventArgs)
            _countdownTimer.Stop()
            DoRestart()
        End Sub

        ''' <summary>
        ''' กดปุ่ม Cancel → หยุด countdown, ปิดหน้าต่าง (ย่อลง tray)
        ''' </summary>
        Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
            _countdownTimer.Stop()
            _secondsLeft = 60 ' รีเซ็ต countdown
            Managers.LogManager.Info("User cancelled restart countdown.")
            Me.WindowState = FormWindowState.Minimized
        End Sub

        ''' <summary>
        ''' ดำเนินการ restart เครื่อง
        ''' </summary>
        Private Sub DoRestart()
            _isRestarting = True
            Try
                Managers.LogManager.Info("Initiating restart (countdown expired or user confirmed).")
                Diagnostics.Process.Start("shutdown", "/r /t 30 /c """"System will restart in 30 seconds for update.""""")
            Catch ex As Exception
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Managers.LogManager.[Error]("Failed to initiate restart: " & ex.Message)
                MessageBox.Show(L("CantRestart") & ex.Message, L("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
                _isRestarting = False
                ' รีเซ็ต countdown ใหม่
                _secondsLeft = 60
                _countdownTimer.Start()
                Return
            End Try

            Me.Close()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _popupTimer IsNot Nothing Then
                    _popupTimer.Stop()
                    RemoveHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
                    _popupTimer.Dispose()
                End If
                If _countdownTimer IsNot Nothing Then
                    _countdownTimer.Stop()
                    RemoveHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
                    _countdownTimer.Dispose()
                End If
                If _btnRestart IsNot Nothing Then
                    RemoveHandler _btnRestart.Click, AddressOf BtnRestart_Click
                End If
                If _btnCancel IsNot Nothing Then
                    RemoveHandler _btnCancel.Click, AddressOf BtnCancel_Click
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
