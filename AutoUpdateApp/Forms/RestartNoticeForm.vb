Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' หน้าต่างแจ้งเตือนรีสตาร์ท (50% ของหน้าจอ)
    ''' - ใช้ SystemIcons.Warning แทน emoji เพื่อรองรับ Win7
    ''' - ปุ่ม Close (X) = ซ่อนหน้าต่าง → เด้งขึ้นมาใหม่ใน 20 วินาที
    ''' </summary>
    Public Class RestartNoticeForm
        Inherits Form

        Private _picIcon As PictureBox
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

            ' ── คำนวณขนาด 50% ของหน้าจอ (ปรับให้เหมาะกับ 1280x1024) ──
            Dim screen As Rectangle = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea
            Dim formW As Integer = CInt(screen.Width * 0.5)
            Dim formH As Integer = CInt(screen.Height * 0.5)

            ' กำหนดขนาดขั้นต่ำ/สูงสุด
            If formW < 500 Then formW = 500
            If formH < 380 Then formH = 380
            If formW > 800 Then formW = 800
            If formH > 600 Then formH = 600

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

            Dim topY As Integer = 20

            ' ── ไอคอนเตือน (SystemIcons.Warning — รองรับ Win7) ──
            _picIcon = New PictureBox()
            _picIcon.Image = New Bitmap(SystemIcons.Warning.ToBitmap(), New Size(64, 64))
            _picIcon.SizeMode = PictureBoxSizeMode.CenterImage
            _picIcon.Size = New Size(formW - 40, 80)
            _picIcon.Location = New Point(20, topY)
            _picIcon.BackColor = Color.Transparent

            ' ── หัวข้อ ──
            _lblHeader = New Label()
            _lblHeader.Text = L("RestartNoticeHeader")
            _lblHeader.Font = New Font("Segoe UI", 24.0F, FontStyle.Bold)
            _lblHeader.ForeColor = Color.White
            _lblHeader.TextAlign = ContentAlignment.MiddleCenter
            _lblHeader.AutoSize = False
            _lblHeader.Size = New Size(formW - 40, 60)
            _lblHeader.Location = New Point(20, topY + 85)

            ' ── เนื้อหา ──
            _lblBody = New Label()
            _lblBody.Text = L("RestartNoticeBody")
            _lblBody.Font = New Font("Segoe UI", 11.0F)
            _lblBody.ForeColor = Color.FromArgb(200, 200, 210)
            _lblBody.TextAlign = ContentAlignment.MiddleCenter
            _lblBody.AutoSize = False
            _lblBody.Size = New Size(formW - 60, 100)
            _lblBody.Location = New Point(30, topY + 155)

            ' ── ปุ่ม Restart Now ──
            _btnRestart = New Button()
            _btnRestart.Text = L("RestartNoticeBtn")
            _btnRestart.Font = New Font("Segoe UI", 14.0F, FontStyle.Bold)
            _btnRestart.ForeColor = Color.White
            _btnRestart.BackColor = Color.FromArgb(220, 53, 69)
            _btnRestart.FlatStyle = FlatStyle.Flat
            _btnRestart.FlatAppearance.BorderSize = 0
            _btnRestart.Size = New Size(260, 50)
            _btnRestart.Location = New Point(CInt((formW - 260) / 2), topY + 270)
            _btnRestart.Cursor = Cursors.Hand
            AddHandler _btnRestart.Click, AddressOf BtnRestart_Click
            AddHandler _btnRestart.MouseEnter, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(200, 35, 51)
            AddHandler _btnRestart.MouseLeave, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(220, 53, 69)

            ' ── คำเตือนเมื่อปิด/ย่อ ──
            _lblWarn = New Label()
            _lblWarn.Text = L("RestartNoticeMinimizeWarn")
            _lblWarn.Font = New Font("Segoe UI", 9.0F, FontStyle.Italic)
            _lblWarn.ForeColor = Color.FromArgb(150, 150, 160)
            _lblWarn.TextAlign = ContentAlignment.MiddleCenter
            _lblWarn.AutoSize = False
            _lblWarn.Size = New Size(formW - 40, 25)
            _lblWarn.Location = New Point(20, topY + 330)

            Me.Controls.Add(_picIcon)
            Me.Controls.Add(_lblHeader)
            Me.Controls.Add(_lblBody)
            Me.Controls.Add(_btnRestart)
            Me.Controls.Add(_lblWarn)

            ' ── Timer: เด้งขึ้นมาใหม่ทุก 20 วินาที ──
            _popupTimer = New Timer()
            _popupTimer.Interval = 20000
            AddHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
            _popupTimer.Start()

            Me.ResumeLayout(False)
        End Sub

        Public Sub UpdateLanguage()
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Me.Text = L("RestartNoticeTitle")
            If _lblHeader IsNot Nothing Then _lblHeader.Text = L("RestartNoticeHeader")
            If _lblBody IsNot Nothing Then _lblBody.Text = L("RestartNoticeBody")
            If _btnRestart IsNot Nothing Then _btnRestart.Text = L("RestartNoticeBtn")
            If _lblWarn IsNot Nothing Then _lblWarn.Text = L("RestartNoticeMinimizeWarn")
        End Sub

        ''' <summary>
        ''' ปุ่มปิด (X) = ซ่อนหน้าต่างแทน (จะเด้งขึ้นมาใหม่ใน 20 วินาที)
        ''' </summary>
        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Not _isRestarting AndAlso e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.Hide()
            End If
            MyBase.OnFormClosing(e)
        End Sub

        ''' <summary>
        ''' เมื่อ user กดย่อ หรือ กดปิด(ซ่อน) แล้วไม่ restart ภายใน 20 วินาที → เด้งขึ้นมาใหม่
        ''' </summary>
        Private Sub PopupTimer_Tick(sender As Object, e As EventArgs)
            If Not Me.Visible OrElse Me.WindowState = FormWindowState.Minimized Then
                Me.Show()
                Me.WindowState = FormWindowState.Normal
                Me.TopMost = True
                Me.BringToFront()
                Me.Activate()
            End If
        End Sub

        Public Sub ResumePopupTimer()
            If _popupTimer IsNot Nothing Then _popupTimer.Start()
        End Sub

        Private Sub BtnRestart_Click(sender As Object, e As EventArgs)
            ' ซ่อนฟอร์มปัจจุบัน และเปิดหน้าต่างนับถอยหลังขึ้นมาแทน
            If _popupTimer IsNot Nothing Then _popupTimer.Stop()
            Me.Hide()
            Dim countdownForm As New RestartCountdownForm(Me)
            countdownForm.Show()
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
                If _picIcon IsNot Nothing AndAlso _picIcon.Image IsNot Nothing Then
                    _picIcon.Image.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
