Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' Result of the user's choice on the update prompt dialog.
    ''' </summary>
    Public Enum UpdatePromptResult
        UpdateNow = 0
        UpdateAfterRestart = 1
        RemindLater = 2
    End Enum

    ''' <summary>
    ''' Prompt dialog for Normal mode.
    ''' Shows current vs latest version and three action buttons.
    ''' You can customize the UI layout in the Designer file.
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
        ''' Gets the user's selected action.
        ''' </summary>
        Public ReadOnly Property UserChoice As UpdatePromptResult
            Get
                Return _userChoice
            End Get
        End Property

        Public Sub New(currentVersion As String, latestVersion As String)
            InitializeComponent()
            lblVersionInfo.Text = "Current: " & currentVersion & "  →  Latest: " & latestVersion
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()

            ' ── lblMessage ──
            lblMessage = New Label()
            lblMessage.Text = "A new update is available."
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
            btnUpdateNow.Text = "Update Now"
            btnUpdateNow.Location = New Drawing.Point(20, 90)
            btnUpdateNow.Size = New Drawing.Size(100, 35)
            AddHandler btnUpdateNow.Click, AddressOf BtnUpdateNow_Click

            ' ── btnAfterRestart ──
            btnAfterRestart = New Button()
            btnAfterRestart.Text = "After Restart"
            btnAfterRestart.Location = New Drawing.Point(135, 90)
            btnAfterRestart.Size = New Drawing.Size(110, 35)
            AddHandler btnAfterRestart.Click, AddressOf BtnAfterRestart_Click

            ' ── btnRemindLater ──
            btnRemindLater = New Button()
            btnRemindLater.Text = "Remind Later"
            btnRemindLater.Location = New Drawing.Point(260, 90)
            btnRemindLater.Size = New Drawing.Size(100, 35)
            AddHandler btnRemindLater.Click, AddressOf BtnRemindLater_Click

            ' ── Form ──
            Me.Text = "Application Update"
            Me.ClientSize = New Drawing.Size(380, 145)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ShowInTaskbar = True
            Me.TopMost = True
            Me.Font = New Drawing.Font("Segoe UI", 9.0F)

            Me.Controls.Add(lblMessage)
            Me.Controls.Add(lblVersionInfo)
            Me.Controls.Add(btnUpdateNow)
            Me.Controls.Add(btnAfterRestart)
            Me.Controls.Add(btnRemindLater)

            Me.ResumeLayout(False)
        End Sub

        Private Sub BtnUpdateNow_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.UpdateNow
            Me.Close()
        End Sub

        Private Sub BtnAfterRestart_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.UpdateAfterRestart
            Me.Close()
        End Sub

        Private Sub BtnRemindLater_Click(sender As Object, e As EventArgs)
            _userChoice = UpdatePromptResult.RemindLater
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
