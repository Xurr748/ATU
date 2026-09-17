Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Forms

    Public Enum UpdatePromptResult
        UpdateNow = 0
        UpdateAfterRestart = 1
        RemindLater = 2
    End Enum

    Public Class UpdatePromptForm

        Private _userChoice As UpdatePromptResult = UpdatePromptResult.RemindLater

        Public ReadOnly Property UserChoice As UpdatePromptResult
            Get
                Return _userChoice
            End Get
        End Property

        Public Sub New(currentVersion As String, latestVersion As String)
            InitializeComponent()
            ApplyLanguage(currentVersion, latestVersion)
            WireEvents()
        End Sub

        Private Sub ApplyLanguage(currentVersion As String, latestVersion As String)
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Me.Text = L("PromptTitle")
            lblMessage.Text = L("PromptNewVersion")
            lblVersionInfo.Text = L("PromptCurrent") & ": " & currentVersion & "  →  " & L("PromptLatest") & ": " & latestVersion
            btnUpdateNow.Text = L("PromptUpdateNow")
            btnAfterRestart.Text = L("PromptAfterRestart")
            btnRemindLater.Text = L("PromptRemindLater")
        End Sub

        Private Sub WireEvents()
            AddHandler btnUpdateNow.Click, AddressOf BtnUpdateNow_Click
            AddHandler btnAfterRestart.Click, AddressOf BtnAfterRestart_Click
            AddHandler btnRemindLater.Click, AddressOf BtnRemindLater_Click
        End Sub

        Private Sub BtnUpdateNow_Click(sender As Object, e As EventArgs)
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Dim result As DialogResult = MessageBox.Show(
                L("ConfirmUpdate"),
                L("ConfirmTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                _userChoice = UpdatePromptResult.UpdateNow
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
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
                If components IsNot Nothing Then
                    components.Dispose()
                End If
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
