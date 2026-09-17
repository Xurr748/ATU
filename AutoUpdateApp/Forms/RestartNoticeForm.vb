Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Namespace Forms

    Public Class RestartNoticeForm

        Private _isRestarting As Boolean = False

        Public Sub New()
            InitializeComponent()
            _picIcon.Image = New Bitmap(SystemIcons.Warning.ToBitmap(), New Size(64, 64))
            AdjustSize()
            UpdateLanguage()
            WireEvents()
        End Sub

        Private Sub AdjustSize()
            Dim screen As Rectangle = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea
            Dim formW As Integer = CInt(screen.Width * 0.5)
            Dim formH As Integer = CInt(screen.Height * 0.5)

            If formW < 500 Then formW = 500
            If formH < 380 Then formH = 380
            If formW > 800 Then formW = 800
            If formH > 600 Then formH = 600
            
            Me.Size = New Size(formW, formH)
            
            ' Center controls
            _picIcon.Size = New Size(formW - 40, 80)
            _lblHeader.Size = New Size(formW - 40, 60)
            _lblBody.Size = New Size(formW - 60, 100)
            _btnRestart.Location = New Point(CInt((formW - _btnRestart.Width) / 2), _btnRestart.Location.Y)
            _lblWarn.Size = New Size(formW - 40, 25)
        End Sub

        Private Sub WireEvents()
            AddHandler _btnRestart.Click, AddressOf BtnRestart_Click
            AddHandler _btnRestart.MouseEnter, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(200, 35, 51)
            AddHandler _btnRestart.MouseLeave, Sub(s, ev) _btnRestart.BackColor = Color.FromArgb(220, 53, 69)
            AddHandler _popupTimer.Tick, AddressOf PopupTimer_Tick
            _popupTimer.Start()
        End Sub

        Public Sub UpdateLanguage()
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Me.Text = L("RestartNoticeTitle")
            If _lblHeader IsNot Nothing Then _lblHeader.Text = L("RestartNoticeHeader")
            If _lblBody IsNot Nothing Then _lblBody.Text = L("RestartNoticeBody")
            If _btnRestart IsNot Nothing Then _btnRestart.Text = L("RestartNoticeBtn")
            If _lblWarn IsNot Nothing Then _lblWarn.Text = L("RestartNoticeMinimizeWarn")
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Not _isRestarting AndAlso e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.Hide()
            End If
            MyBase.OnFormClosing(e)
        End Sub

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
            If _popupTimer IsNot Nothing Then _popupTimer.Stop()
            Me.Hide()
            Dim countdownForm As New RestartCountdownForm(Me)
            countdownForm.Show()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If components IsNot Nothing Then
                    components.Dispose()
                End If
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
