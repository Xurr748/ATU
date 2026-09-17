Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Namespace Forms

    Public Class RestartCountdownForm

        Private _secondsLeft As Integer = 60
        Private _isRestarting As Boolean = False
        Private _parentForm As Form

        Public Sub New(parentForm As Form)
            _parentForm = parentForm
            InitializeComponent()
            AdjustSize()
            UpdateLanguage()
            WireEvents()
        End Sub

        Private Sub AdjustSize()
            Dim screen As Rectangle = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea
            Dim formW As Integer = CInt(screen.Width * 0.4)
            Dim formH As Integer = CInt(screen.Height * 0.4)

            If formW < 400 Then formW = 400
            If formH < 300 Then formH = 300
            If formW > 600 Then formW = 600
            If formH > 450 Then formH = 450

            Me.Size = New Size(formW, formH)

            Dim centerY As Integer = CInt(formH * 0.08)
            _lblHeader.Size = New Size(formW - 40, 40)
            _lblHeader.Location = New Point(20, centerY)
            _lblCountdown.Size = New Size(formW - 40, 120)
            _lblCountdown.Location = New Point(20, centerY + 45)
            _btnCancel.Location = New Point(CInt((formW - 220) / 2), centerY + 185)
        End Sub

        Private Sub WireEvents()
            AddHandler _btnCancel.Click, AddressOf BtnCancel_Click
            AddHandler _btnCancel.MouseEnter, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(100, 100, 115)
            AddHandler _btnCancel.MouseLeave, Sub(s, ev) _btnCancel.BackColor = Color.FromArgb(80, 80, 95)
            AddHandler _countdownTimer.Tick, AddressOf CountdownTimer_Tick
            _countdownTimer.Start()
        End Sub

        Public Sub UpdateLanguage()
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Me.Text = L("RestartNoticeTitle")
            If _lblHeader IsNot Nothing Then _lblHeader.Text = L("RestartNoticeCountdown").Replace("{0}", "")
            If _btnCancel IsNot Nothing Then _btnCancel.Text = L("RestartNoticeBtnCancel")
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

            If _parentForm IsNot Nothing Then
                _parentForm.Show()
                _parentForm.Hide()
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
                If components IsNot Nothing Then
                    components.Dispose()
                End If
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
