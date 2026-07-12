Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' Main application form — minimal skeleton.
    ''' Hosts the SchedulerManager and UpdateWorker.
    ''' Provides a system tray icon with "Check Now" and "Exit" options.
    ''' 
    ''' Customize the UI in this file as needed.
    ''' </summary>
    Public Class MainForm
        Inherits Form

        Private WithEvents _scheduler As Managers.SchedulerManager
        Private WithEvents _updateWorker As Workers.UpdateWorker

        Private _notifyIcon As NotifyIcon
        Private _contextMenu As ContextMenuStrip
        Private _mnuCheckNow As ToolStripMenuItem
        Private _mnuSeparator As ToolStripSeparator
        Private components As System.ComponentModel.IContainer
        Private _mnuExit As ToolStripMenuItem

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
            Me._contextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me._mnuCheckNow = New System.Windows.Forms.ToolStripMenuItem()
            Me._mnuSeparator = New System.Windows.Forms.ToolStripSeparator()
            Me._mnuExit = New System.Windows.Forms.ToolStripMenuItem()
            Me._notifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
            Me._contextMenu.SuspendLayout()
            Me.SuspendLayout()
            '
            '_contextMenu
            '
            Me._contextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._mnuCheckNow, Me._mnuSeparator, Me._mnuExit})
            Me._contextMenu.Name = "_contextMenu"
            Me._contextMenu.Size = New System.Drawing.Size(136, 32)
            '
            '_mnuCheckNow
            '
            Me._mnuCheckNow.Name = "_mnuCheckNow"
            Me._mnuCheckNow.Size = New System.Drawing.Size(135, 22)
            Me._mnuCheckNow.Text = "Check Now"
            '
            '_mnuSeparator
            '
            Me._mnuSeparator.Name = "_mnuSeparator"
            Me._mnuSeparator.Size = New System.Drawing.Size(132, 6)
            '
            '_mnuExit
            '
            Me._mnuExit.Name = "_mnuExit"
            Me._mnuExit.Size = New System.Drawing.Size(32, 19)
            Me._mnuExit.Text = "Exit"
            '
            '_notifyIcon
            '
            Me._notifyIcon.ContextMenuStrip = Me._contextMenu
            Me._notifyIcon.Icon = CType(resources.GetObject("_notifyIcon.Icon"), System.Drawing.Icon)
            Me._notifyIcon.Text = "Auto Update"
            Me._notifyIcon.Visible = True
            '
            'MainForm
            '
            Me.ClientSize = New System.Drawing.Size(738, 408)
            Me.Name = "MainForm"
            Me.Opacity = 0.0R
            Me.ShowInTaskbar = False
            Me.Text = "Auto Update"
            Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
            Me._contextMenu.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            ' Hide the form
            Me.Visible = False

            ' Initialize the worker and scheduler
            _updateWorker = New Workers.UpdateWorker(Me)
            AddHandler _updateWorker.UpdateCompleted, AddressOf OnUpdateCompleted

            _scheduler = New Managers.SchedulerManager()
            AddHandler _scheduler.TickFired, AddressOf OnSchedulerTick
            _scheduler.Start()

            Managers.LogManager.Info("MainForm loaded. Scheduler started.")
        End Sub

        ' ── Scheduler tick → run update check ──
        Private Sub OnSchedulerTick(sender As Object, e As EventArgs)
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── Update completed ──
        Private Sub OnUpdateCompleted(sender As Object, e As Workers.UpdateCompletedEventArgs)
            ' You can add UI feedback here (e.g. balloon tooltip)
            ' _notifyIcon.ShowBalloonTip(3000, "Auto Update", e.Message, ToolTipIcon.Info)
        End Sub

        ' ── Menu: Check Now ──
        Private Sub MnuCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuCheckNow.Click
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user.")
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── Menu: Exit ──
        Private Sub MnuExit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuExit.Click
            CleanupAndExit()
        End Sub

        ''' <summary>
        ''' Properly disposes all resources and exits the application.
        ''' </summary>
        Private Sub CleanupAndExit()
            If _scheduler IsNot Nothing Then
                _scheduler.Dispose()
                _scheduler = Nothing
            End If

            If _updateWorker IsNot Nothing Then
                _updateWorker.Cancel()
                _updateWorker.Dispose()
                _updateWorker = Nothing
            End If

            If _notifyIcon IsNot Nothing Then
                _notifyIcon.Visible = False
                _notifyIcon.Dispose()
                _notifyIcon = Nothing
            End If

            Managers.LogManager.Info("Application exiting.")
            Application.Exit()
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            ' Minimize to tray instead of closing (unless exiting via menu)
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.Visible = False
            Else
                CleanupAndExit()
            End If
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _contextMenu IsNot Nothing Then
                    RemoveHandler _mnuCheckNow.Click, AddressOf MnuCheckNow_Click
                    RemoveHandler _mnuExit.Click, AddressOf MnuExit_Click
                    _contextMenu.Dispose()
                End If
                If _notifyIcon IsNot Nothing Then
                    _notifyIcon.Visible = False
                    _notifyIcon.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        End Sub
    End Class

End Namespace
