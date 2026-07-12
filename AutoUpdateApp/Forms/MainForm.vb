Option Strict On
Option Explicit On

Imports System.Windows.Forms

Namespace Forms

    ''' <summary>
    ''' หน้าจอหลักของแอปพลิเคชัน — โครงสร้างขั้นต่ำ
    ''' เป็นที่อยู่ของ SchedulerManager และ UpdateWorker
    ''' แสดงไอคอนที่ System Tray พร้อมเมนู
    ''' 
    ''' ปรับแต่ง UI ในไฟล์นี้ได้ตามต้องการ
    ''' </summary>
    Public Class MainForm
        Inherits Form

        Private WithEvents _scheduler As Managers.SchedulerManager
        Private WithEvents _updateWorker As Workers.UpdateWorker

        Private WithEvents _notifyIcon As NotifyIcon
        Private _contextMenu As ContextMenuStrip
        Private WithEvents _mnuCheckNow As ToolStripMenuItem
        Private _mnuSeparator As ToolStripSeparator
        Private components As System.ComponentModel.IContainer
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents Button1 As System.Windows.Forms.Button
        Friend WithEvents Button2 As System.Windows.Forms.Button
        Friend WithEvents Button3 As System.Windows.Forms.Button
        Private WithEvents _mnuExit As ToolStripMenuItem

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
            Me.Label1 = New System.Windows.Forms.Label()
            Me.Button1 = New System.Windows.Forms.Button()
            Me.Button2 = New System.Windows.Forms.Button()
            Me.Button3 = New System.Windows.Forms.Button()
            Me._contextMenu.SuspendLayout()
            Me.SuspendLayout()
            '
            '_contextMenu
            '
            Me._contextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._mnuCheckNow, Me._mnuSeparator, Me._mnuExit})
            Me._contextMenu.Name = "_contextMenu"
            Me._contextMenu.Size = New System.Drawing.Size(136, 54)
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
            Me._mnuExit.Size = New System.Drawing.Size(135, 22)
            Me._mnuExit.Text = "Exit"
            '
            '_notifyIcon
            '
            Me._notifyIcon.ContextMenuStrip = Me._contextMenu
            Me._notifyIcon.Icon = CType(resources.GetObject("_notifyIcon.Icon"), System.Drawing.Icon)
            Me._notifyIcon.Text = "Auto Update"
            Me._notifyIcon.Visible = True
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(60, 28)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(39, 13)
            Me.Label1.TabIndex = 1
            Me.Label1.Text = "Label1"
            '
            'Button1
            '
            Me.Button1.Location = New System.Drawing.Point(27, 208)
            Me.Button1.Name = "Button1"
            Me.Button1.Size = New System.Drawing.Size(109, 33)
            Me.Button1.TabIndex = 2
            Me.Button1.Text = "Button1"
            Me.Button1.UseVisualStyleBackColor = True
            '
            'Button2
            '
            Me.Button2.Location = New System.Drawing.Point(175, 208)
            Me.Button2.Name = "Button2"
            Me.Button2.Size = New System.Drawing.Size(125, 33)
            Me.Button2.TabIndex = 3
            Me.Button2.Text = "Button2"
            Me.Button2.UseVisualStyleBackColor = True
            '
            'Button3
            '
            Me.Button3.Location = New System.Drawing.Point(369, 208)
            Me.Button3.Name = "Button3"
            Me.Button3.Size = New System.Drawing.Size(82, 33)
            Me.Button3.TabIndex = 4
            Me.Button3.Text = "Button3"
            Me.Button3.UseVisualStyleBackColor = True
            '
            'MainForm
            '
            Me.ClientSize = New System.Drawing.Size(548, 274)
            Me.Controls.Add(Me.Button3)
            Me.Controls.Add(Me.Button2)
            Me.Controls.Add(Me.Button1)
            Me.Controls.Add(Me.Label1)
            Me.Name = "MainForm"
            Me.Opacity = 0.0R
            Me.ShowInTaskbar = False
            Me.Text = "Auto Update"
            Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
            Me._contextMenu.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            MyBase.OnLoad(e)

            ' ซ่อนหน้าต่าง
            Me.Visible = False

            ' สร้าง Worker และตัวตั้งเวลา
            _updateWorker = New Workers.UpdateWorker(Me)
            AddHandler _updateWorker.UpdateCompleted, AddressOf OnUpdateCompleted

            _scheduler = New Managers.SchedulerManager()
            AddHandler _scheduler.TickFired, AddressOf OnSchedulerTick
            _scheduler.Start()

            Managers.LogManager.Info("MainForm loaded. Scheduler started.")
        End Sub

        ' ── Timer ครบรอบ → เริ่มตรวจสอบอัปเดต ──
        Private Sub OnSchedulerTick(ByVal sender As Object, ByVal e As EventArgs)
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── ตรวจสอบอัปเดตเสร็จแล้ว ──
        Private Sub OnUpdateCompleted(ByVal sender As Object, ByVal e As Workers.UpdateCompletedEventArgs)
            ' You can add UI feedback here (e.g. balloon tooltip)
            ' _notifyIcon.ShowBalloonTip(3000, "Auto Update", e.Message, ToolTipIcon.Info)
        End Sub

        ' ── ไอคอน Tray: ดับเบิลคลิกเพื่อเปิดหน้าต่าง ──
        Private Sub NotifyIcon_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles _notifyIcon.DoubleClick
            ShowForm()
        End Sub

        ' ── แสดง/คืนสภาพหน้าต่างหลัก ──
        Private Sub ShowForm()
            Me.Visible = True
            Me.ShowInTaskbar = True
            Me.Opacity = 1.0R
            Me.WindowState = FormWindowState.Normal
            Me.BringToFront()
            Me.Activate()
        End Sub

        ' ── เมนู: ตรวจสอบตอนนี้ ──
        Private Sub MnuCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuCheckNow.Click
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user.")
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── เมนู: ออกจากโปรแกรม ──
        Private Sub MnuExit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuExit.Click
            CleanupAndExit()
        End Sub

        ''' <summary>
        ''' ปล่อยทรัพยากรทั้งหมดและปิดโปรแกรมอย่างถูกต้อง
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

        Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
            ' ย่อลงไปที่ Tray แทนการปิด (ยกเว้นกดออกจากเมนู)
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.WindowState = FormWindowState.Minimized
                Me.ShowInTaskbar = False
                Me.Visible = False
            Else
                CleanupAndExit()
            End If
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
