Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Forms

    ''' <summary>
    ''' หน้าจอหลักของแอปพลิเคชัน
    ''' แสดงข้อมูล Version (Current/Server), ComputerName, Type, Mode, Time
    ''' มีไอคอนที่ System Tray พร้อมเมนู Check Now / Exit
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
        Private WithEvents _mnuExit As ToolStripMenuItem

        ' ── UI Controls ──
        Private _grpInfo As Panel
        Private _lblInfoTitle As Label
        Private _lblComNameLabel As Label
        Private _lblComNameValue As Label
        Private _lblTypeLabel As Label
        Private _lblTypeValue As Label
        Private _lblModeLabel As Label
        Private _lblModeValue As Label
        Private _lblTimeLabel As Label
        Private _lblTimeValue As Label

        Private _grpVersion As Panel
        Private _lblVersionTitle As Label
        Private _lblCurrentLabel As Label
        Private _lblCurrentValue As Label
        Private _lblServerLabel As Label
        Private _lblServerValue As Label
        Private _lblStatusLabel As Label
        Private _lblStatusValue As Label

        Private _btnCheckNow As Button
        Private _btnRefreshInfo As Button
        Private _btnExit As Button
        Private _btnUpdateNow As Button

        ' ── Progress Bar + Status ──
        Private _progressBar As ProgressBar
        Private _lblProgress As Label
        Private _manualUpdateWorker As System.ComponentModel.BackgroundWorker
        Private WithEvents _fadeTimer As Timer
        Private WithEvents _typewriteTimer As Timer
        Private _typewriteTargets As New System.Collections.Generic.Dictionary(Of Label, String)
        Private _typewriteIndices As New System.Collections.Generic.Dictionary(Of Label, Integer)
        Private WithEvents _btnAnimTimer As Timer
        Private _btnTargets As New System.Collections.Generic.Dictionary(Of Button, Color)
        Private _btnBorders As New System.Collections.Generic.Dictionary(Of Button, Color)
        Private _btnTargetBorders As New System.Collections.Generic.Dictionary(Of Button, Color)

        ' ── ตัวแปรเก็บค่าข้อความชั่วคราวระหว่างรอ Fade-in เสร็จ ──
        Private _tempComName As String = ""
        Private _tempType As String = ""
        Private _tempMode As String = ""
        Private _tempTime As String = ""
        Private _tempCurrentVer As String = ""
        Private _tempServerVer As String = ""
        Private _tempStatus As String = ""

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
            Me._grpInfo = New System.Windows.Forms.Panel()
            Me._lblInfoTitle = New System.Windows.Forms.Label()
            Me._lblComNameLabel = New System.Windows.Forms.Label()
            Me._lblComNameValue = New System.Windows.Forms.Label()
            Me._lblTypeLabel = New System.Windows.Forms.Label()
            Me._lblTypeValue = New System.Windows.Forms.Label()
            Me._lblModeLabel = New System.Windows.Forms.Label()
            Me._lblModeValue = New System.Windows.Forms.Label()
            Me._lblTimeLabel = New System.Windows.Forms.Label()
            Me._lblTimeValue = New System.Windows.Forms.Label()
            Me._grpVersion = New System.Windows.Forms.Panel()
            Me._lblVersionTitle = New System.Windows.Forms.Label()
            Me._lblCurrentLabel = New System.Windows.Forms.Label()
            Me._lblCurrentValue = New System.Windows.Forms.Label()
            Me._lblServerLabel = New System.Windows.Forms.Label()
            Me._lblServerValue = New System.Windows.Forms.Label()
            Me._lblStatusLabel = New System.Windows.Forms.Label()
            Me._lblStatusValue = New System.Windows.Forms.Label()
            Me._btnCheckNow = New System.Windows.Forms.Button()
            Me._btnRefreshInfo = New System.Windows.Forms.Button()
            Me._btnExit = New System.Windows.Forms.Button()
            Me._btnUpdateNow = New System.Windows.Forms.Button()
            Me._contextMenu.SuspendLayout()
            Me._grpInfo.SuspendLayout()
            Me._grpVersion.SuspendLayout()
            Me.SuspendLayout()
            '
            '_contextMenu
            '
            Me._contextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._mnuCheckNow, Me._mnuSeparator, Me._mnuExit})
            Me._contextMenu.Name = "_contextMenu"
            Me._contextMenu.Size = New System.Drawing.Size(144, 54)
            '
            '_mnuCheckNow
            '
            Me._mnuCheckNow.Name = "_mnuCheckNow"
            Me._mnuCheckNow.Size = New System.Drawing.Size(143, 22)
            Me._mnuCheckNow.Text = "ตรวจสอบตอนนี้"
            '
            '_mnuSeparator
            '
            Me._mnuSeparator.Name = "_mnuSeparator"
            Me._mnuSeparator.Size = New System.Drawing.Size(140, 6)
            '
            '_mnuExit
            '
            Me._mnuExit.Name = "_mnuExit"
            Me._mnuExit.Size = New System.Drawing.Size(143, 22)
            Me._mnuExit.Text = "ออก"
            '
            '_notifyIcon
            '
            Me._notifyIcon.ContextMenuStrip = Me._contextMenu
            Me._notifyIcon.Icon = CType(resources.GetObject("_notifyIcon.Icon"), System.Drawing.Icon)
            Me._notifyIcon.Text = "Auto Update"
            Me._notifyIcon.Visible = True
            '
            '_grpInfo
            '
            Me._grpInfo.Controls.Add(Me._lblInfoTitle)
            Me._grpInfo.Controls.Add(Me._lblComNameLabel)
            Me._grpInfo.Controls.Add(Me._lblComNameValue)
            Me._grpInfo.Controls.Add(Me._lblTypeLabel)
            Me._grpInfo.Controls.Add(Me._lblTypeValue)
            Me._grpInfo.Controls.Add(Me._lblModeLabel)
            Me._grpInfo.Controls.Add(Me._lblModeValue)
            Me._grpInfo.Controls.Add(Me._lblTimeLabel)
            Me._grpInfo.Controls.Add(Me._lblTimeValue)
            Me._grpInfo.Location = New System.Drawing.Point(14, 14)
            Me._grpInfo.Name = "_grpInfo"
            Me._grpInfo.Size = New System.Drawing.Size(370, 130)
            Me._grpInfo.TabIndex = 1
            Me._grpInfo.BackColor = System.Drawing.Color.White
            '
            '_lblInfoTitle
            '
            Me._lblInfoTitle.AutoSize = True
            Me._lblInfoTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
            Me._lblInfoTitle.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185)
            Me._lblInfoTitle.Location = New System.Drawing.Point(16, 12)
            Me._lblInfoTitle.Name = "_lblInfoTitle"
            Me._lblInfoTitle.Text = "ข้อมูลเครื่องทดสอบ"
            '
            '_lblComNameLabel
            '
            Me._lblComNameLabel.AutoSize = True
            Me._lblComNameLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblComNameLabel.Location = New System.Drawing.Point(16, 40)
            Me._lblComNameLabel.Name = "_lblComNameLabel"
            Me._lblComNameLabel.Size = New System.Drawing.Size(49, 15)
            Me._lblComNameLabel.TabIndex = 0
            Me._lblComNameLabel.Text = "ชื่อเครื่อง:"
            '
            '_lblComNameValue
            '
            Me._lblComNameValue.AutoSize = True
            Me._lblComNameValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblComNameValue.Location = New System.Drawing.Point(130, 40)
            Me._lblComNameValue.Name = "_lblComNameValue"
            Me._lblComNameValue.Size = New System.Drawing.Size(16, 15)
            Me._lblComNameValue.TabIndex = 1
            Me._lblComNameValue.Text = "..."
            '
            '_lblTypeLabel
            '
            Me._lblTypeLabel.AutoSize = True
            Me._lblTypeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTypeLabel.Location = New System.Drawing.Point(16, 62)
            Me._lblTypeLabel.Name = "_lblTypeLabel"
            Me._lblTypeLabel.Size = New System.Drawing.Size(43, 15)
            Me._lblTypeLabel.TabIndex = 2
            Me._lblTypeLabel.Text = "ประเภท:"
            '
            '_lblTypeValue
            '
            Me._lblTypeValue.AutoSize = True
            Me._lblTypeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTypeValue.Location = New System.Drawing.Point(130, 62)
            Me._lblTypeValue.Name = "_lblTypeValue"
            Me._lblTypeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTypeValue.TabIndex = 3
            Me._lblTypeValue.Text = "..."
            '
            '_lblModeLabel
            '
            Me._lblModeLabel.AutoSize = True
            Me._lblModeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblModeLabel.Location = New System.Drawing.Point(16, 84)
            Me._lblModeLabel.Name = "_lblModeLabel"
            Me._lblModeLabel.Size = New System.Drawing.Size(36, 15)
            Me._lblModeLabel.TabIndex = 4
            Me._lblModeLabel.Text = "โหมด:"
            '
            '_lblModeValue
            '
            Me._lblModeValue.AutoSize = True
            Me._lblModeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblModeValue.Location = New System.Drawing.Point(130, 84)
            Me._lblModeValue.Name = "_lblModeValue"
            Me._lblModeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblModeValue.TabIndex = 5
            Me._lblModeValue.Text = "..."
            '
            '_lblTimeLabel
            '
            Me._lblTimeLabel.AutoSize = True
            Me._lblTimeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTimeLabel.Location = New System.Drawing.Point(16, 106)
            Me._lblTimeLabel.Name = "_lblTimeLabel"
            Me._lblTimeLabel.Size = New System.Drawing.Size(71, 15)
            Me._lblTimeLabel.TabIndex = 6
            Me._lblTimeLabel.Text = "เวลาตรวจสอบ:"
            '
            '_lblTimeValue
            '
            Me._lblTimeValue.AutoSize = True
            Me._lblTimeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTimeValue.Location = New System.Drawing.Point(130, 106)
            Me._lblTimeValue.Name = "_lblTimeValue"
            Me._lblTimeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTimeValue.TabIndex = 7
            Me._lblTimeValue.Text = "..."
            '
            '_grpVersion
            '
            Me._grpVersion.Controls.Add(Me._lblVersionTitle)
            Me._grpVersion.Controls.Add(Me._lblCurrentLabel)
            Me._grpVersion.Controls.Add(Me._lblCurrentValue)
            Me._grpVersion.Controls.Add(Me._lblServerLabel)
            Me._grpVersion.Controls.Add(Me._lblServerValue)
            Me._grpVersion.Controls.Add(Me._lblStatusLabel)
            Me._grpVersion.Controls.Add(Me._lblStatusValue)
            Me._grpVersion.Location = New System.Drawing.Point(14, 152)
            Me._grpVersion.Name = "_grpVersion"
            Me._grpVersion.Size = New System.Drawing.Size(370, 104)
            Me._grpVersion.TabIndex = 2
            Me._grpVersion.BackColor = System.Drawing.Color.White
            '
            '_lblVersionTitle
            '
            Me._lblVersionTitle.AutoSize = True
            Me._lblVersionTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
            Me._lblVersionTitle.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185)
            Me._lblVersionTitle.Location = New System.Drawing.Point(16, 12)
            Me._lblVersionTitle.Name = "_lblVersionTitle"
            Me._lblVersionTitle.Text = "สถานะซอฟต์แวร์"
            '
            '_lblCurrentLabel
            '
            Me._lblCurrentLabel.AutoSize = True
            Me._lblCurrentLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblCurrentLabel.Location = New System.Drawing.Point(16, 40)
            Me._lblCurrentLabel.Name = "_lblCurrentLabel"
            Me._lblCurrentLabel.Size = New System.Drawing.Size(76, 15)
            Me._lblCurrentLabel.TabIndex = 0
            Me._lblCurrentLabel.Text = "เวอร์ชันปัจจุบัน:"
            '
            '_lblCurrentValue
            '
            Me._lblCurrentValue.AutoSize = True
            Me._lblCurrentValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblCurrentValue.Location = New System.Drawing.Point(150, 40)
            Me._lblCurrentValue.Name = "_lblCurrentValue"
            Me._lblCurrentValue.Size = New System.Drawing.Size(16, 15)
            Me._lblCurrentValue.TabIndex = 1
            Me._lblCurrentValue.Text = "..."
            '
            '_lblServerLabel
            '
            Me._lblServerLabel.AutoSize = True
            Me._lblServerLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblServerLabel.Location = New System.Drawing.Point(16, 62)
            Me._lblServerLabel.Name = "_lblServerLabel"
            Me._lblServerLabel.Size = New System.Drawing.Size(78, 15)
            Me._lblServerLabel.TabIndex = 2
            Me._lblServerLabel.Text = "เวอร์ชัน Server:"
            '
            '_lblServerValue
            '
            Me._lblServerValue.AutoSize = True
            Me._lblServerValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblServerValue.Location = New System.Drawing.Point(150, 62)
            Me._lblServerValue.Name = "_lblServerValue"
            Me._lblServerValue.Size = New System.Drawing.Size(16, 15)
            Me._lblServerValue.TabIndex = 3
            Me._lblServerValue.Text = "..."
            '
            '_lblStatusLabel
            '
            Me._lblStatusLabel.AutoSize = True
            Me._lblStatusLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblStatusLabel.Location = New System.Drawing.Point(16, 84)
            Me._lblStatusLabel.Name = "_lblStatusLabel"
            Me._lblStatusLabel.Size = New System.Drawing.Size(39, 15)
            Me._lblStatusLabel.TabIndex = 4
            Me._lblStatusLabel.Text = "สถานะ:"
            '
            '_lblStatusValue
            '
            Me._lblStatusValue.AutoSize = True
            Me._lblStatusValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblStatusValue.Location = New System.Drawing.Point(150, 84)
            Me._lblStatusValue.Name = "_lblStatusValue"
            Me._lblStatusValue.Size = New System.Drawing.Size(16, 15)
            Me._lblStatusValue.TabIndex = 5
            Me._lblStatusValue.Text = "..."
            '
            '
            '_btnUpdateNow
            '
            Me._btnUpdateNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnUpdateNow.FlatAppearance.BorderSize = 0
            Me._btnUpdateNow.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
            Me._btnUpdateNow.Location = New System.Drawing.Point(14, 268)
            Me._btnUpdateNow.Name = "_btnUpdateNow"
            Me._btnUpdateNow.Size = New System.Drawing.Size(370, 34)
            Me._btnUpdateNow.TabIndex = 3
            Me._btnUpdateNow.Text = "อัปเดตทันที"
            Me._btnUpdateNow.BackColor = System.Drawing.Color.FromArgb(9, 132, 227)
            Me._btnUpdateNow.ForeColor = System.Drawing.Color.White
            Me._btnUpdateNow.Cursor = System.Windows.Forms.Cursors.Hand
            '
            '_btnCheckNow
            '
            Me._btnCheckNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnCheckNow.FlatAppearance.BorderSize = 1
            Me._btnCheckNow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(9, 132, 227)
            Me._btnCheckNow.BackColor = System.Drawing.Color.White
            Me._btnCheckNow.ForeColor = System.Drawing.Color.FromArgb(9, 132, 227)
            Me._btnCheckNow.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnCheckNow.Location = New System.Drawing.Point(14, 310)
            Me._btnCheckNow.Name = "_btnCheckNow"
            Me._btnCheckNow.Size = New System.Drawing.Size(120, 32)
            Me._btnCheckNow.TabIndex = 4
            Me._btnCheckNow.Text = "ตรวจสอบอัปเดต"
            Me._btnCheckNow.Cursor = System.Windows.Forms.Cursors.Hand
            '
            '_btnRefreshInfo
            '
            Me._btnRefreshInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnRefreshInfo.FlatAppearance.BorderSize = 1
            Me._btnRefreshInfo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(108, 92, 231)
            Me._btnRefreshInfo.BackColor = System.Drawing.Color.White
            Me._btnRefreshInfo.ForeColor = System.Drawing.Color.FromArgb(108, 92, 231)
            Me._btnRefreshInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnRefreshInfo.Location = New System.Drawing.Point(144, 310)
            Me._btnRefreshInfo.Name = "_btnRefreshInfo"
            Me._btnRefreshInfo.Size = New System.Drawing.Size(110, 32)
            Me._btnRefreshInfo.TabIndex = 5
            Me._btnRefreshInfo.Text = "รีเฟรชข้อมูล"
            Me._btnRefreshInfo.Cursor = System.Windows.Forms.Cursors.Hand
            '
            '_btnExit
            '
            Me._btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnExit.FlatAppearance.BorderSize = 1
            Me._btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(235, 77, 75)
            Me._btnExit.BackColor = System.Drawing.Color.White
            Me._btnExit.ForeColor = System.Drawing.Color.FromArgb(235, 77, 75)
            Me._btnExit.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnExit.Location = New System.Drawing.Point(314, 310)
            Me._btnExit.Name = "_btnExit"
            Me._btnExit.Size = New System.Drawing.Size(70, 32)
            Me._btnExit.TabIndex = 6
            Me._btnExit.Text = "ออก"
            Me._btnExit.Cursor = System.Windows.Forms.Cursors.Hand
            '
            '_progressBar
            '
            Me._progressBar = New System.Windows.Forms.ProgressBar()
            Me._progressBar.Location = New System.Drawing.Point(14, 350)
            Me._progressBar.Name = "_progressBar"
            Me._progressBar.Size = New System.Drawing.Size(370, 18)
            Me._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee
            Me._progressBar.MarqueeAnimationSpeed = 30
            Me._progressBar.Visible = False
            '
            '_lblProgress
            '
            Me._lblProgress = New System.Windows.Forms.Label()
            Me._lblProgress.Location = New System.Drawing.Point(14, 370)
            Me._lblProgress.Name = "_lblProgress"
            Me._lblProgress.Size = New System.Drawing.Size(370, 18)
            Me._lblProgress.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Italic)
            Me._lblProgress.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100)
            Me._lblProgress.Text = ""
            Me._lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me._lblProgress.Visible = False
            '
            'MainForm
            '
            Me.ClientSize = New System.Drawing.Size(400, 398)
            Me.Controls.Add(Me._progressBar)
            Me.Controls.Add(Me._lblProgress)
            Me.Controls.Add(Me._btnUpdateNow)
            Me.Controls.Add(Me._grpInfo)
            Me.Controls.Add(Me._grpVersion)
            Me.Controls.Add(Me._btnCheckNow)
            Me.Controls.Add(Me._btnRefreshInfo)
            Me.Controls.Add(Me._btnExit)
            Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.Opacity = 0.0R
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "Auto Update"
            Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
            Me.BackColor = System.Drawing.Color.FromArgb(245, 245, 250)
            Me._fadeTimer = New System.Windows.Forms.Timer()
            Me._fadeTimer.Interval = 30
            AddHandler Me._fadeTimer.Tick, AddressOf FadeTimer_Tick
            Me._contextMenu.ResumeLayout(False)
            Me._grpInfo.ResumeLayout(False)
            Me._grpInfo.PerformLayout()
            Me._grpVersion.ResumeLayout(False)
            Me._grpVersion.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        ' ══════════════════════════════════════════════
        ' โหลดข้อมูลแสดงผลบน UI
        ' ══════════════════════════════════════════════
        Private Sub LoadInfo()
            Try
                ' ── ข้อมูลเครื่อง ──
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                _lblComNameValue.Text = computerName

                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester IsNot Nothing Then
                    _lblTypeValue.Text = tester.TesterType
                    _lblModeValue.Text = tester.Mode
                    _lblTimeValue.Text = tester.ScheduledTime.ToString("hh\:mm\:ss")
                Else
                    _lblTypeValue.Text = "(ไม่พบใน Config)"
                    _lblModeValue.Text = "-"
                    _lblTimeValue.Text = "-"
                End If

                ' ── เวอร์ชัน ──
                Dim currentVer As String = Managers.VersionManager.ReadRegistryVersion()
                Dim serverVer As String = Managers.VersionManager.ReadLatestVersion()

                _lblCurrentValue.Text = If(String.IsNullOrEmpty(currentVer), "(ไม่พบ)", currentVer)
                _lblServerValue.Text = If(String.IsNullOrEmpty(serverVer), "(อ่านไม่ได้)", serverVer)

                ' ── สถานะ ──
                If String.IsNullOrEmpty(currentVer) OrElse String.IsNullOrEmpty(serverVer) Then
                    _lblStatusValue.Text = "● ไม่สามารถตรวจสอบได้"
                    _lblStatusValue.ForeColor = Color.FromArgb(149, 165, 166)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                ElseIf String.Equals(currentVer, serverVer, StringComparison.OrdinalIgnoreCase) Then
                    _lblStatusValue.Text = "● เวอร์ชันล่าสุดแล้ว"
                    _lblStatusValue.ForeColor = Color.FromArgb(46, 204, 113)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                Else
                    _lblStatusValue.Text = "● มีอัปเดตใหม่"
                    _lblStatusValue.ForeColor = Color.FromArgb(231, 76, 60)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = True
                End If

            Catch ex As Exception
                Managers.LogManager.[Error]("เกิดข้อผิดพลาดตอนโหลดข้อมูล UI", ex)
                _lblStatusValue.Text = "Error: " & ex.Message
                _lblStatusValue.ForeColor = Color.Red
            End Try
        End Sub

        ' ══════════════════════════════════════════════
        ' Events
        ' ══════════════════════════════════════════════

        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            MyBase.OnLoad(e)

            ' ซ่อนหน้าต่าง
            Me.Visible = False

            ' ผูก Event วาดพื้นหลังแบบไล่เฉดสี (Gradient)
            AddHandler _grpInfo.Paint, AddressOf Panel_Paint
            AddHandler _grpVersion.Paint, AddressOf Panel_Paint
            AddHandler Me.Paint, AddressOf MainForm_Paint

            ' ผูก Event ปุ่มและอนิเมชั่นของปุ่ม
            AddButtonAnimHandlers(_btnUpdateNow, Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219))
            AddButtonAnimHandlers(_btnCheckNow, Color.White, Color.FromArgb(235, 245, 253), Color.FromArgb(70, 130, 180), Color.FromArgb(41, 128, 185))
            AddButtonAnimHandlers(_btnRefreshInfo, Color.White, Color.FromArgb(245, 240, 255), Color.FromArgb(180, 180, 180), Color.FromArgb(108, 92, 231))
            AddButtonAnimHandlers(_btnExit, Color.White, Color.FromArgb(255, 240, 240), Color.FromArgb(220, 80, 80), Color.FromArgb(180, 50, 50))

            AddHandler _btnCheckNow.Click, AddressOf BtnCheckNow_Click
            AddHandler _btnRefreshInfo.Click, AddressOf BtnRefreshInfo_Click
            AddHandler _btnExit.Click, AddressOf BtnExit_Click
            AddHandler _btnUpdateNow.Click, AddressOf BtnUpdateNow_Click

            ' เริ่มตัวนับเวลาของ Typewriter Effect
            _typewriteTimer = New System.Windows.Forms.Timer()
            _typewriteTimer.Interval = 35 ' ความเร็วพิมพ์ตัวอักษร 35ms
            AddHandler _typewriteTimer.Tick, AddressOf TypewriteTimer_Tick

            ' เริ่มตัวนับเวลาของ Button Hover Animation
            _btnAnimTimer = New System.Windows.Forms.Timer()
            _btnAnimTimer.Interval = 15 ' อัพเดตสีทุกๆ 15ms เพื่อความลื่นไหล
            AddHandler _btnAnimTimer.Tick, AddressOf BtnAnimTimer_Tick

            ' สร้าง Worker และตัวตั้งเวลา
            _updateWorker = New Workers.UpdateWorker(Me)
            AddHandler _updateWorker.UpdateCompleted, AddressOf OnUpdateCompleted

            _scheduler = New Managers.SchedulerManager()
            AddHandler _scheduler.TickFired, AddressOf OnSchedulerTick
            _scheduler.Start()

            ' โหลดข้อมูลครั้งแรก
            LoadInfo()

            Managers.LogManager.Info("MainForm loaded. Scheduler started.")
        End Sub

        ' ── Timer ครบรอบ → เริ่มตรวจสอบอัปเดต ──
        Private Sub OnSchedulerTick(ByVal sender As Object, ByVal e As EventArgs)
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── ตรวจสอบอัปเดตเสร็จแล้ว → รีเฟรช UI + แจ้งผล ──
        Private Sub OnUpdateCompleted(ByVal sender As Object, ByVal e As Workers.UpdateCompletedEventArgs)
            LoadInfo()
            ShowProgress(False, "")
            ' คืนค่าปุ่มตรวจสอบ
            If _btnCheckNow IsNot Nothing Then
                _btnCheckNow.Enabled = True
                _btnCheckNow.Text = "ตรวจสอบอัปเดต"
            End If
            ' แจ้งผลเฉพาะตอนหน้าต่างเปิดอยู่
            If Me.Visible AndAlso Me.WindowState <> FormWindowState.Minimized Then
                Select Case e.Result
                    Case Strategies.UpdateResult.NoAction
                        MessageBox.Show("ตรวจสอบเสร็จ: " & e.Message, "ผลการตรวจสอบ", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case Strategies.UpdateResult.UpdateCompleted
                        MessageBox.Show("อัปเดตสำเร็จเรียบร้อย!", "ผลการตรวจสอบ", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case Strategies.UpdateResult.[Error]
                        MessageBox.Show("เกิดข้อผิดพลาด: " & e.Message, "ผลการตรวจสอบ", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Select
            End If
        End Sub

        ' ── ไอคอน Tray: ดับเบิลคลิกเพื่อเปิดหน้าต่าง ──
        Private Sub NotifyIcon_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles _notifyIcon.DoubleClick
            ShowForm()
        End Sub

        ' ── แสดง/คืนสภาพหน้าต่างหลัก ──
        Private Sub ShowForm()
            LoadInfo()

            ' ดึงค่าที่โหลดขึ้นมาแล้ว เพื่อเตรียมทำเอฟเฟคพิมพ์ดีดหลังขยายหน้าต่างและสไลด์การ์ดเสร็จ
            _tempComName = _lblComNameValue.Text
            _tempType = _lblTypeValue.Text
            _tempMode = _lblModeValue.Text
            _tempTime = _lblTimeValue.Text
            _tempCurrentVer = _lblCurrentValue.Text
            _tempServerVer = _lblServerValue.Text
            _tempStatus = _lblStatusValue.Text

            ' เคลียร์ฟิลด์ชั่วคราวเพื่อให้เป็นช่องว่างระหว่างอนิเมชั่นค่อยๆ แสดงหน้าต่าง
            _lblComNameValue.Text = ""
            _lblTypeValue.Text = ""
            _lblModeValue.Text = ""
            _lblTimeValue.Text = ""
            _lblCurrentValue.Text = ""
            _lblServerValue.Text = ""
            _lblStatusValue.Text = ""

            ' ตั้งค่าพิกัดเริ่มต้นสำหรับการ์ดสไลด์เยื้องเวลา (Staggered Animation)
            _grpInfo.Top = 50
            _grpVersion.Top = 188

            Me.Opacity = 0.0R
            Me.Visible = True
            Me.ShowInTaskbar = True
            Me.WindowState = FormWindowState.Normal
            Me.BringToFront()
            Me.Activate()
            If Me._fadeTimer IsNot Nothing Then
                Me._fadeTimer.Start()
            Else
                _grpInfo.Top = 14
                _grpVersion.Top = 152
                Me.Opacity = 1.0R
                TriggerTypewriter()
            End If
        End Sub

        ' ── เรียกอนิเมชั่น Typewriter ให้ช่องข้อความเริ่มทำงานพร้อมกัน ──
        Private Sub TriggerTypewriter()
            StartTypewriter(_lblComNameValue, _tempComName)
            StartTypewriter(_lblTypeValue, _tempType)
            StartTypewriter(_lblModeValue, _tempMode)
            StartTypewriter(_lblTimeValue, _tempTime)
            StartTypewriter(_lblCurrentValue, _tempCurrentVer)
            StartTypewriter(_lblServerValue, _tempServerVer)
            StartTypewriter(_lblStatusValue, _tempStatus)
        End Sub

        ' ── ตัวนับเวลาสำหรับทำอนิเมชั่นค่อยๆ แสดงหน้าต่าง (Fade-in) และขยับเลื่อนการ์ดแบบเยื้องเวลา (Staggered Slide-in) ──
        Private Sub FadeTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Dim isOpacityDone As Boolean = False
            If Me.Opacity < 1.0R Then
                Me.Opacity += 0.08R
            Else
                Me.Opacity = 1.0R
                isOpacityDone = True
            End If

            Dim infoTarget As Integer = 14
            Dim versionTarget As Integer = 152
            Dim stepY As Integer = 3

            ' เลื่อนการ์ดแรกทันที
            Dim isInfoDone As Boolean = False
            If _grpInfo.Top > infoTarget Then
                _grpInfo.Top = Math.Max(infoTarget, _grpInfo.Top - stepY)
            Else
                _grpInfo.Top = infoTarget
                isInfoDone = True
            End If

            ' เลื่อนการ์ดสองเยื้องเวลา (จะเริ่มเมื่อการ์ดแรกเลื่อนเข้าใกล้เป้าหมายแล้ว)
            Dim isVersionDone As Boolean = False
            If _grpInfo.Top <= 26 Then
                If _grpVersion.Top > versionTarget Then
                    _grpVersion.Top = Math.Max(versionTarget, _grpVersion.Top - stepY)
                Else
                    _grpVersion.Top = versionTarget
                    isVersionDone = True
                End If
            End If

            ' บังคับวาดหน้าต่างใหม่เพื่อให้แสดงเงาการ์ดเคลื่อนตามตำแหน่งของการ์ดจริง
            Me.Invalidate()

            If isOpacityDone AndAlso isInfoDone AndAlso isVersionDone Then
                Me._fadeTimer.Stop()
                TriggerTypewriter()
            End If
        End Sub

        ' ── เมนู Tray: ตรวจสอบตอนนี้ ──
        Private Sub MnuCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuCheckNow.Click
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user.")
                _updateWorker.RunAsync(True)
            End If
        End Sub

        ' ── เมนู Tray: ออกจากโปรแกรม ──
        Private Sub MnuExit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuExit.Click
            CleanupAndExit()
        End Sub

        ' ── ปุ่ม: ตรวจสอบอัปเดต ──
        Private Sub BtnCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs)
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user (button).")
                _btnCheckNow.Enabled = False
                _btnCheckNow.Text = "กำลังตรวจสอบ..."
                ShowProgress(True, "กำลังตรวจสอบอัปเดต...")
                _updateWorker.RunAsync(True)
            Else
                MessageBox.Show("กำลังตรวจสอบอยู่แล้ว กรุณารอสักครู่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Sub

        ' ── ปุ่ม: รีเฟรชข้อมูล ──
        Private Sub BtnRefreshInfo_Click(ByVal sender As Object, ByVal e As EventArgs)
            Managers.ConfigManager.InvalidateCache()
            LoadInfo()

            ' ดึงค่าใหม่มาใส่ตัวแปรชั่วคราวเพื่อทำเอฟเฟคพิมพ์ดีดซ้ำอีกรอบ
            _tempComName = _lblComNameValue.Text
            _tempType = _lblTypeValue.Text
            _tempMode = _lblModeValue.Text
            _tempTime = _lblTimeValue.Text
            _tempCurrentVer = _lblCurrentValue.Text
            _tempServerVer = _lblServerValue.Text
            _tempStatus = _lblStatusValue.Text

            ' สั่งรันเอฟเฟค Typewriter พิมพ์ใหม่
            TriggerTypewriter()

            MessageBox.Show("รีเฟรชข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        ' ── ปุ่ม: อัปเดตทันที (รันบน BackgroundWorker) ──
        Private Sub BtnUpdateNow_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                
                If tester Is Nothing Then
                    MessageBox.Show("เครื่องนี้ไม่อยู่ในระบบ (TesterType.csv)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                Dim result = MessageBox.Show("ต้องการอัปเดตแอปพลิเคชันเดี๋ยวนี้หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    _btnUpdateNow.Enabled = False
                    _btnUpdateNow.Text = "กำลังอัปเดต..."
                    ShowProgress(True, "กำลังรัน uninstall / install ...")
                    
                    ' รันบน BackgroundWorker เพื่อไม่บล็อก UI
                    If _manualUpdateWorker IsNot Nothing Then
                        RemoveHandler _manualUpdateWorker.DoWork, AddressOf ManualUpdate_DoWork
                        RemoveHandler _manualUpdateWorker.RunWorkerCompleted, AddressOf ManualUpdate_Completed
                        _manualUpdateWorker.Dispose()
                    End If
                    _manualUpdateWorker = New System.ComponentModel.BackgroundWorker()
                    AddHandler _manualUpdateWorker.DoWork, AddressOf ManualUpdate_DoWork
                    AddHandler _manualUpdateWorker.RunWorkerCompleted, AddressOf ManualUpdate_Completed
                    _manualUpdateWorker.RunWorkerAsync(tester.TesterType)
                End If
            Catch ex As Exception
                Managers.LogManager.[Error]("Manual update failed.", ex)
                MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ResetUpdateButton()
            End Try
        End Sub

        Private Sub ManualUpdate_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs)
            Dim testerType As String = DirectCast(e.Argument, String)
            e.Result = Managers.InstallerManager.RunInstaller(testerType)
        End Sub

        Private Sub ManualUpdate_Completed(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs)
            ShowProgress(False, "")
            If e.Error IsNot Nothing Then
                Managers.LogManager.[Error]("Manual update error.", e.Error)
                MessageBox.Show("Error: " & e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ResetUpdateButton()
                Return
            End If

            Dim success As Boolean = DirectCast(e.Result, Boolean)
            If success Then
                MessageBox.Show("อัปเดตสำเร็จเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                LoadInfo()
            Else
                MessageBox.Show("อัปเดตไม่สำเร็จ กรุณาตรวจสอบ Log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            ResetUpdateButton()
        End Sub

        Private Sub ResetUpdateButton()
            If _btnUpdateNow IsNot Nothing Then
                _btnUpdateNow.Enabled = True
                _btnUpdateNow.Text = "อัปเดตทันที"
            End If
        End Sub

        ' ── แสดง/ซ่อน Progress Bar ──
        Private Sub ShowProgress(show As Boolean, statusText As String)
            If _progressBar IsNot Nothing Then _progressBar.Visible = show
            If _lblProgress IsNot Nothing Then
                _lblProgress.Text = statusText
                _lblProgress.Visible = show
            End If
        End Sub

        ' ── ปุ่ม: ออก ──
        Private Sub BtnExit_Click(ByVal sender As Object, ByVal e As EventArgs)
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
                If _btnCheckNow IsNot Nothing Then RemoveHandler _btnCheckNow.Click, AddressOf BtnCheckNow_Click
                If _btnRefreshInfo IsNot Nothing Then RemoveHandler _btnRefreshInfo.Click, AddressOf BtnRefreshInfo_Click
                If _btnExit IsNot Nothing Then RemoveHandler _btnExit.Click, AddressOf BtnExit_Click
                If _btnUpdateNow IsNot Nothing Then RemoveHandler _btnUpdateNow.Click, AddressOf BtnUpdateNow_Click
                If _manualUpdateWorker IsNot Nothing Then
                    RemoveHandler _manualUpdateWorker.DoWork, AddressOf ManualUpdate_DoWork
                    RemoveHandler _manualUpdateWorker.RunWorkerCompleted, AddressOf ManualUpdate_Completed
                    _manualUpdateWorker.Dispose()
                End If
                If _fadeTimer IsNot Nothing Then
                    RemoveHandler _fadeTimer.Tick, AddressOf FadeTimer_Tick
                    _fadeTimer.Dispose()
                    _fadeTimer = Nothing
                End If
                If _typewriteTimer IsNot Nothing Then
                    RemoveHandler _typewriteTimer.Tick, AddressOf TypewriteTimer_Tick
                    _typewriteTimer.Dispose()
                    _typewriteTimer = Nothing
                End If
                If _btnAnimTimer IsNot Nothing Then
                    RemoveHandler _btnAnimTimer.Tick, AddressOf BtnAnimTimer_Tick
                    _btnAnimTimer.Dispose()
                    _btnAnimTimer = Nothing
                End If
                If _contextMenu IsNot Nothing Then _contextMenu.Dispose()
                If _notifyIcon IsNot Nothing Then
                    _notifyIcon.Visible = False
                    _notifyIcon.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        ' ── เริ่มอนิเมชั่นพิมพ์ดีดสำหรับ Label ที่ระบุ ──
        Private Sub StartTypewriter(ByVal lbl As Label, ByVal text As String)
            If lbl Is Nothing Then Return
            lbl.Text = ""
            _typewriteTargets(lbl) = text
            _typewriteIndices(lbl) = 0
            If _typewriteTimer IsNot Nothing Then
                _typewriteTimer.Start()
            End If
        End Sub

        ' ── ตัวนับเวลาการวาดตัวอักษรทีละตัว ──
        Private Sub TypewriteTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Dim keys As New System.Collections.Generic.List(Of Label)(_typewriteTargets.Keys)
            Dim allDone As Boolean = True

            For Each lbl In keys
                Dim target As String = _typewriteTargets(lbl)
                Dim idx As Integer = _typewriteIndices(lbl)
                If idx < target.Length Then
                    lbl.Text = target.Substring(0, idx + 1)
                    _typewriteIndices(lbl) = idx + 1
                    allDone = False
                End If
            Next

            If allDone Then
                _typewriteTimer.Stop()
            End If
        End Sub

        ' ── วาดพื้นหลังหน้าต่างหลักแบบไล่เฉดสีนุ่มนวล (MainForm Gradient Background) ──
        Private Sub MainForm_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
            Dim rect As New Rectangle(0, 0, Me.Width, Me.Height)
            Using brush As New LinearGradientBrush(rect, Color.FromArgb(245, 247, 250), Color.FromArgb(232, 236, 243), 90.0F)
                e.Graphics.FillRectangle(brush, rect)
            End Using

            ' วาดเงาการ์ดข้อมูลแบบนุ่มนวล (Soft Panel Shadows)
            DrawPanelShadow(e.Graphics, _grpInfo.Bounds)
            DrawPanelShadow(e.Graphics, _grpVersion.Bounds)
        End Sub

        ' ── วาดเงาฟุ้งแบบมีมิติใต้ Panel การ์ดต่าง ๆ ──
        Private Sub DrawPanelShadow(ByVal g As Graphics, ByVal rect As Rectangle)
            ' วาดเงานุ่มๆ 5 ชั้นโดยลดความโปร่งแสงออกไปด้านนอก
            For i As Integer = 1 To 5
                Using pen As New Pen(Color.FromArgb(CInt(10 - (i * 2)), 0, 0, 0), i * 2)
                    ' วาดเส้นเงาด้านล่างและด้านขวา
                    g.DrawLine(pen, rect.Left + 4, rect.Bottom + i, rect.Right + i, rect.Bottom + i)
                    g.DrawLine(pen, rect.Right + i, rect.Top + 4, rect.Right + i, rect.Bottom + i)
                End Using
            Next
        End Sub

        ' ── วาดการ์ดข้อมูลแต่ละใบเป็นแบบไล่เฉดสีมุมทแยงพร้อมขอบบาง (Panel Card Gradient) ──
        Private Sub Panel_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
            Dim pnl As Panel = DirectCast(sender, Panel)
            Dim rect As New Rectangle(0, 0, pnl.Width, pnl.Height)
            Using brush As New LinearGradientBrush(rect, Color.White, Color.FromArgb(248, 250, 253), 45.0F)
                e.Graphics.FillRectangle(brush, rect)
            End Using
            ' วาดเส้นขอบแบบพรีเมียม
            Using borderPen As New Pen(Color.FromArgb(218, 224, 233), 1)
                e.Graphics.DrawRectangle(borderPen, 0, 0, pnl.Width - 1, pnl.Height - 1)
            End Using
        End Sub

        ' ── ระบบจัดการลงทะเบียน Event ให้ปุ่มรองรับ Hover และ Click Transitions ──
        Private Sub AddButtonAnimHandlers(ByVal btn As Button, ByVal normalColor As Color, ByVal hoverColor As Color, ByVal normalBorder As Color, ByVal hoverBorder As Color)
            If btn Is Nothing Then Return
            
            ' กำหนดสีเริ่มต้น
            btn.BackColor = normalColor
            btn.FlatAppearance.BorderColor = normalBorder
            
            _btnTargets(btn) = normalColor
            _btnBorders(btn) = normalBorder
            _btnTargetBorders(btn) = normalBorder

            ' ลงทะเบียน Mouse Events
            AddHandler btn.MouseEnter, Sub(s, ev)
                                           _btnTargets(btn) = hoverColor
                                           _btnTargetBorders(btn) = hoverBorder
                                           If _btnAnimTimer IsNot Nothing Then _btnAnimTimer.Start()
                                       End Sub
            AddHandler btn.MouseLeave, Sub(s, ev)
                                           _btnTargets(btn) = normalColor
                                           _btnTargetBorders(btn) = normalBorder
                                           If _btnAnimTimer IsNot Nothing Then _btnAnimTimer.Start()
                                       End Sub
            AddHandler btn.MouseDown, Sub(s, ev)
                                          ' Tactile click effect: ขยับเนื้อหาลงเล็กน้อย
                                          btn.Padding = New Padding(0, 2, 0, 0)
                                      End Sub
            AddHandler btn.MouseUp, Sub(s, ev)
                                        btn.Padding = New Padding(0, 0, 0, 0)
                                    End Sub
        End Sub

        ' ── ค่อยๆ ปรับ R, G, B ของปุ่มให้ลื่นไหล (Color Transitions Step) ──
        Private Function InterpolateColor(ByVal current As Color, ByVal target As Color, ByVal stepVal As Integer) As Color
            Dim r As Integer = current.R
            Dim g As Integer = current.G
            Dim b As Integer = current.B

            If r < target.R Then r = Math.Min(target.R, r + stepVal)
            If r > target.R Then r = Math.Max(target.R, r - stepVal)
            If g < target.G Then g = Math.Min(target.G, g + stepVal)
            If g > target.G Then g = Math.Max(target.G, g - stepVal)
            If b < target.B Then b = Math.Min(target.B, b + stepVal)
            If b > target.B Then b = Math.Max(target.B, b - stepVal)

            Return Color.FromArgb(r, g, b)
        End Function

        ' ── อัปเดตเฟรมอนิเมชั่นสีปุ่มทีละนิดจนกว่าจะถึงสีเป้าหมาย ──
        Private Sub BtnAnimTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Dim btns As New System.Collections.Generic.List(Of Button)(_btnTargets.Keys)
            Dim allColorsReached As Boolean = True
            Dim stepVal As Integer = 15 ' อัตราความเร็วในการเฟดสี

            For Each btn In btns
                Dim currentBack As Color = btn.BackColor
                Dim targetBack As Color = _btnTargets(btn)
                Dim currentBorder As Color = btn.FlatAppearance.BorderColor
                Dim targetBorder As Color = _btnTargetBorders(btn)

                If currentBack <> targetBack Then
                    btn.BackColor = InterpolateColor(currentBack, targetBack, stepVal)
                    allColorsReached = False
                End If

                If currentBorder <> targetBorder Then
                    btn.FlatAppearance.BorderColor = InterpolateColor(currentBorder, targetBorder, stepVal)
                    allColorsReached = False
                End If
            Next

            If allColorsReached Then
                _btnAnimTimer.Stop()
            End If
        End Sub

    End Class

End Namespace
