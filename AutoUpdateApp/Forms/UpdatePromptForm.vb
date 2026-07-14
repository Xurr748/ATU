Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' ผลลัพธ์จากการเลือกของผู้ใช้บนหน้าต่างแจ้งเตือนอัปเดต
    ''' </summary>
    Public Enum UpdatePromptResult
        UpdateNow = 0
        UpdateAfterRestart = 1
        RemindLater = 2
    End Enum

    ''' <summary>
    ''' หน้าต่างแจ้งเตือนสำหรับโหมด Normal
    ''' แสดงเวอร์ชันปัจจุบัน vs ล่าสุด และปุ่มเลือก 3 ปุ่ม
    ''' ใช้ uninstall.bat/install.bat สำหรับการอัปเดต
    ''' </summary>
    Public Class UpdatePromptForm
        Inherits Form

        Private _userChoice As UpdatePromptResult = UpdatePromptResult.RemindLater

        Private lblMessage As Label
        Private lblVersionInfo As Label
        Private btnUpdateNow As Button
        Private btnAfterRestart As Button
        Private btnRemindLater As Button

        ''' <summary>
        ''' ดึงค่าที่ผู้ใช้เลือก
        ''' </summary>
        Public ReadOnly Property UserChoice As UpdatePromptResult
            Get
                Return _userChoice
            End Get
        End Property

        Public Sub New(currentVersion As String, latestVersion As String)
            InitializeComponent()
            lblVersionInfo.Text = "ปัจจุบัน: " & currentVersion & "  →  ล่าสุด: " & latestVersion
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()

            ' ── lblMessage ──
            lblMessage = New Label()
            lblMessage.Text = "พบเวอร์ชันใหม่พร้อมอัปเดต!"
            lblMessage.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
            lblMessage.Location = New Drawing.Point(20, 20)
            lblMessage.Size = New Drawing.Size(340, 25)
            lblMessage.AutoSize = False

            ' ── lblVersionInfo ──
            lblVersionInfo = New Label()
            lblVersionInfo.Text = ""
            lblVersionInfo.Font = New Drawing.Font("Segoe UI", 9.0F)
            lblVersionInfo.Location = New Drawing.Point(20, 50)
            lblVersionInfo.Size = New Drawing.Size(340, 20)
            lblVersionInfo.AutoSize = False

            ' ── btnUpdateNow ──
            btnUpdateNow = New Button()
            btnUpdateNow.Text = "อัปเดตตอนนี้"
            btnUpdateNow.Location = New Drawing.Point(20, 90)
            btnUpdateNow.Size = New Drawing.Size(105, 35)
            btnUpdateNow.Font = New Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold)
            btnUpdateNow.FlatStyle = FlatStyle.Flat
            btnUpdateNow.FlatAppearance.BorderColor = Drawing.Color.FromArgb(70, 130, 180)
            btnUpdateNow.BackColor = Drawing.Color.FromArgb(70, 130, 180)
            btnUpdateNow.ForeColor = Drawing.Color.White
            btnUpdateNow.Cursor = Cursors.Hand
            AddHandler btnUpdateNow.Click, AddressOf BtnUpdateNow_Click

            ' ── btnAfterRestart ──
            btnAfterRestart = New Button()
            btnAfterRestart.Text = "หลังรีสตาร์ท"
            btnAfterRestart.Location = New Drawing.Point(135, 90)
            btnAfterRestart.Size = New Drawing.Size(110, 35)
            btnAfterRestart.FlatStyle = FlatStyle.Flat
            btnAfterRestart.FlatAppearance.BorderColor = Drawing.Color.FromArgb(200, 200, 200)
            btnAfterRestart.BackColor = Drawing.Color.White
            btnAfterRestart.Cursor = Cursors.Hand
            AddHandler btnAfterRestart.Click, AddressOf BtnAfterRestart_Click

            ' ── btnRemindLater ──
            btnRemindLater = New Button()
            btnRemindLater.Text = "เตือนทีหลัง"
            btnRemindLater.Location = New Drawing.Point(255, 90)
            btnRemindLater.Size = New Drawing.Size(105, 35)
            btnRemindLater.FlatStyle = FlatStyle.Flat
            btnRemindLater.FlatAppearance.BorderColor = Drawing.Color.FromArgb(200, 200, 200)
            btnRemindLater.BackColor = Drawing.Color.White
            btnRemindLater.Cursor = Cursors.Hand
            AddHandler btnRemindLater.Click, AddressOf BtnRemindLater_Click

            ' ── Form ──
            Me.Text = "แจ้งเตือนอัปเดต"
            Me.ClientSize = New Drawing.Size(380, 145)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ShowInTaskbar = True
            Me.TopMost = True
            Me.Font = New Drawing.Font("Segoe UI", 9.0F)
            Me.BackColor = Drawing.Color.FromArgb(245, 245, 250)

            Me.Controls.Add(lblMessage)
            Me.Controls.Add(lblVersionInfo)
            Me.Controls.Add(btnUpdateNow)
            Me.Controls.Add(btnAfterRestart)
            Me.Controls.Add(btnRemindLater)

            Me.ResumeLayout(False)
        End Sub

        Private Sub BtnUpdateNow_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.UpdateNow
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

        Private Sub BtnAfterRestart_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.UpdateAfterRestart
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

        Private Sub BtnRemindLater_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.RemindLater
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If btnUpdateNow IsNot Nothing Then
                    RemoveHandler btnUpdateNow.Click, AddressOf BtnUpdateNow_Click
                End If
                If btnAfterRestart IsNot Nothing Then
                    RemoveHandler btnAfterRestart.Click, AddressOf BtnAfterRestart_Click
                End If
                If btnRemindLater IsNot Nothing Then
                    RemoveHandler btnRemindLater.Click, AddressOf BtnRemindLater_Click
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
