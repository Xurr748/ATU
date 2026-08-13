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
    ''' </summary>
    Public Class RestartNoticeForm
        Inherits Form

        Private _lblIcon As Label
        Private _lblHeader As Label
        Private _lblBody As Label
        Private _lblWarn As Label
        Private _btnRestart As Button
        Private WithEvents _popupTimer As Timer
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
            Me.MaximizeBox = False       ' ปิดปุ่ม Maximize
            Me.MinimizeBox = True        ' เหลือแค่ Minimize
            Me.TopMost = True
            Me.ShowInTaskbar = True
            Me.BackColor = Color.FromArgb(25, 25, 35)
            Me.Font = New Font("Segoe UI", 10.0F)

            ' ── ⚠ ไอคอนเตือน ──
            _lblIcon = New Label()
            _lblIcon.Text = "⚠"
            _lblIcon.Font = New Font("Segoe UI Emoji", 72.0F)
            _lblIcon.ForeColor = Color.FromArgb(255, 193, 7)
            _lblIcon.TextAlign = ContentAlignment.MiddleCenter
            _lblIcon.AutoSize = False
            _lblIcon.Size = New Size(formW - 40, 130)
            _lblIcon.Location = New Point(20, CInt(formH * 0.08))

            ' ── หัวข้อขนาดใหญ่ ──
            _lblHeader = New Label()
            _lblHeader.Text = L("RestartNoticeHeader")
            _lblHeader.Font = New Font("Segoe UI", 32.0F, FontStyle.Bold)
            _lblHeader.ForeColor = Color.White
            _lblHeader.TextAlign = ContentAlignment.MiddleCenter
            _lblHeader.AutoSize = False
            _lblHeader.Size = New Size(formW - 40, 80)
            _lblHeader.Location = New Point(20, CInt(formH * 0.08) + 135)

            ' ── เนื้อหา ──
            _lblBody = New Label()
            _lblBody.Text = L("RestartNoticeBody")
            _lblBody.Font = New Font("Segoe UI", 15.0F)
            _lblBody.ForeColor = Color.FromArgb(200, 200, 210)
            _lblBody.TextAlign = ContentAlignment.MiddleCenter
            _lblBody.AutoSize = False
            _lblBody.Size = New Size(formW - 80, 140)
            _lblBody.Location = New Point(40, CInt(formH * 0.08) + 230)

            ' ── ปุ่ม Restart Now ──
            _btnRestart = New Button()
            _btnRestart.Text = L("RestartNoticeBtn")
            _btnRestart.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
            _btnRestart.ForeColor = Color.White
            _btnRestart.BackColor = Color.FromArgb(220, 53, 69)
            _btnRestart.FlatStyle = FlatStyle.Flat
            _btnRestart.FlatAppearance.BorderSize = 0
            _btnRestart.Size = New Size(300, 60)
            _btnRestart.Location = New Point(CInt((formW - 300) / 2), CInt(formH * 0.08) + 400)
            _btnRestart.Cursor = Cursors.Hand
            AddHandler _btnRestart.Click, AddressOf BtnRestart_Click
            AddHandler _btnRestart.MouseEnter, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(200, 35, 51)
            AddHandler _btnRestart.MouseLeave, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(220, 53, 69)

            ' ── คำเตือนเมื่อ Minimize ──
            _lblWarn = New Label()
            _lblWarn.Text = L("RestartNoticeMinimizeWarn")
            _lblWarn.Font = New Font("Segoe UI", 10.0F, FontStyle.Italic)
            _lblWarn.ForeColor = Color.FromArgb(150, 150, 160)
            _lblWarn.TextAlign = ContentAlignment.MiddleCenter
            _lblWarn.AutoSize = False
            _lblWarn.Size = New Size(formW - 40, 30)
            _lblWarn.Location = New Point(20, CInt(formH * 0.08) + 475)

            Me.Controls.Add(_lblIcon)
            Me.Controls.Add(_lblHeader)
            Me.Controls.Add(_lblBody)
            Me.Controls.Add(_btnRestart)
            Me.Controls.Add(_lblWarn)

            ' ── Timer: เด้งขึ้นมาใหม่ทุก 20 วินาที ──
            _popupTimer = New Timer()
            _popupTimer.Interval = 20000 ' 20 วินาที
            AddHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
            _popupTimer.Start()

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

        Private Sub BtnRestart_Click(sender As Object, e As EventArgs)
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Dim result As DialogResult = MessageBox.Show(
                L("RestartPromptMsg"),
                L("RestartPromptTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)

            If result = DialogResult.Yes Then
                _isRestarting = True
                Try
                    Managers.LogManager.Info("User confirmed restart from RestartNoticeForm.")
                    Diagnostics.Process.Start("shutdown", "/r /t 30 /c """"System will restart in 30 seconds for update.""""")
                Catch ex As Exception
                    Managers.LogManager.[Error]("Failed to initiate restart: " & ex.Message)
                    MessageBox.Show(L("CantRestart") & ex.Message, L("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
                    _isRestarting = False
                End Try

                If _isRestarting Then
                    Me.Close()
                End If
            End If
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _popupTimer IsNot Nothing Then
                    _popupTimer.Stop()
                    RemoveHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
                    _popupTimer.Dispose()
                End If
                If _btnRestart IsNot Nothing Then
                    RemoveHandler _btnRestart.Click, AddressOf BtnRestart_Click
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
