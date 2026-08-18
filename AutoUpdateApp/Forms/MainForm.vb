Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Forms

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
        Private _btnUpdateNow As Button
        Private _btnDetails As Button
        Private _detailsMenu As ContextMenuStrip
        Private _btnConfigDebug As Button
        Private _btnLang As Button
        Private _btnGear As Button
        Private _gearMenu As ContextMenuStrip

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
        Private _flagSetTime As DateTime = DateTime.MinValue
        Private _restartPromptShown As Boolean = False
        Private _falseCount As Integer = 0
        Private WithEvents _restartCheckTimer As Timer

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
            Me._btnUpdateNow = New System.Windows.Forms.Button()
            Me._btnDetails = New System.Windows.Forms.Button()
            Me._btnConfigDebug = New System.Windows.Forms.Button()
            Me._btnLang = New System.Windows.Forms.Button()
            Me._detailsMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me._progressBar = New System.Windows.Forms.ProgressBar()
            Me._lblProgress = New System.Windows.Forms.Label()
            Me._fadeTimer = New System.Windows.Forms.Timer(Me.components)
            Me._gearMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me._btnGear = New System.Windows.Forms.Button()
            Me._contextMenu.SuspendLayout()
            Me._grpInfo.SuspendLayout()
            Me._grpVersion.SuspendLayout()
            Me.SuspendLayout()
            Me._contextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._mnuCheckNow, Me._mnuSeparator, Me._mnuExit})
            Me._contextMenu.Name = "_contextMenu"
            Me._contextMenu.Size = New System.Drawing.Size(144, 54)
            Me._mnuCheckNow.Name = "_mnuCheckNow"
            Me._mnuCheckNow.Size = New System.Drawing.Size(143, 22)
            Me._mnuCheckNow.Text = "ตรวจสอบตอนนี้"
            Me._mnuSeparator.Name = "_mnuSeparator"
            Me._mnuSeparator.Size = New System.Drawing.Size(140, 6)
            Me._mnuExit.Name = "_mnuExit"
            Me._mnuExit.Size = New System.Drawing.Size(143, 22)
            Me._mnuExit.Text = "ออก"
            Me._notifyIcon.ContextMenuStrip = Me._contextMenu
            Me._notifyIcon.Icon = CType(resources.GetObject("_notifyIcon.Icon"), System.Drawing.Icon)
            Me._notifyIcon.Text = "Auto Update"
            Me._notifyIcon.Visible = True
            Me._grpInfo.BackColor = System.Drawing.Color.White
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
            Me._lblInfoTitle.AutoSize = True
            Me._lblInfoTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
            Me._lblInfoTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
            Me._lblInfoTitle.Location = New System.Drawing.Point(16, 12)
            Me._lblInfoTitle.Name = "_lblInfoTitle"
            Me._lblInfoTitle.Size = New System.Drawing.Size(124, 19)
            Me._lblInfoTitle.TabIndex = 0
            Me._lblInfoTitle.Text = "ข้อมูลเครื่องทดสอบ"
            Me._lblComNameLabel.AutoSize = True
            Me._lblComNameLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblComNameLabel.Location = New System.Drawing.Point(16, 40)
            Me._lblComNameLabel.Name = "_lblComNameLabel"
            Me._lblComNameLabel.Size = New System.Drawing.Size(49, 15)
            Me._lblComNameLabel.TabIndex = 0
            Me._lblComNameLabel.Text = "ชื่อเครื่อง:"
            Me._lblComNameValue.AutoSize = True
            Me._lblComNameValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblComNameValue.Location = New System.Drawing.Point(130, 40)
            Me._lblComNameValue.Name = "_lblComNameValue"
            Me._lblComNameValue.Size = New System.Drawing.Size(16, 15)
            Me._lblComNameValue.TabIndex = 1
            Me._lblComNameValue.Text = "..."
            Me._lblTypeLabel.AutoSize = True
            Me._lblTypeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTypeLabel.Location = New System.Drawing.Point(16, 62)
            Me._lblTypeLabel.Name = "_lblTypeLabel"
            Me._lblTypeLabel.Size = New System.Drawing.Size(43, 15)
            Me._lblTypeLabel.TabIndex = 2
            Me._lblTypeLabel.Text = "ประเภท:"
            Me._lblTypeValue.AutoSize = True
            Me._lblTypeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTypeValue.Location = New System.Drawing.Point(130, 62)
            Me._lblTypeValue.Name = "_lblTypeValue"
            Me._lblTypeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTypeValue.TabIndex = 3
            Me._lblTypeValue.Text = "..."
            Me._lblModeLabel.AutoSize = True
            Me._lblModeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblModeLabel.Location = New System.Drawing.Point(16, 84)
            Me._lblModeLabel.Name = "_lblModeLabel"
            Me._lblModeLabel.Size = New System.Drawing.Size(36, 15)
            Me._lblModeLabel.TabIndex = 4
            Me._lblModeLabel.Text = "โหมด:"
            Me._lblModeValue.AutoSize = True
            Me._lblModeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblModeValue.Location = New System.Drawing.Point(130, 84)
            Me._lblModeValue.Name = "_lblModeValue"
            Me._lblModeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblModeValue.TabIndex = 5
            Me._lblModeValue.Text = "..."
            Me._lblTimeLabel.AutoSize = True
            Me._lblTimeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTimeLabel.Location = New System.Drawing.Point(16, 106)
            Me._lblTimeLabel.Name = "_lblTimeLabel"
            Me._lblTimeLabel.Size = New System.Drawing.Size(71, 15)
            Me._lblTimeLabel.TabIndex = 6
            Me._lblTimeLabel.Text = "เวลาตรวจสอบ:"
            Me._lblTimeValue.AutoSize = True
            Me._lblTimeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTimeValue.Location = New System.Drawing.Point(130, 106)
            Me._lblTimeValue.Name = "_lblTimeValue"
            Me._lblTimeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTimeValue.TabIndex = 7
            Me._lblTimeValue.Text = "..."
            Me._grpVersion.BackColor = System.Drawing.Color.White
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
            Me._lblVersionTitle.AutoSize = True
            Me._lblVersionTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
            Me._lblVersionTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
            Me._lblVersionTitle.Location = New System.Drawing.Point(16, 12)
            Me._lblVersionTitle.Name = "_lblVersionTitle"
            Me._lblVersionTitle.Size = New System.Drawing.Size(109, 19)
            Me._lblVersionTitle.TabIndex = 0
            Me._lblVersionTitle.Text = "สถานะซอฟต์แวร์"
            Me._lblCurrentLabel.AutoSize = True
            Me._lblCurrentLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblCurrentLabel.Location = New System.Drawing.Point(16, 40)
            Me._lblCurrentLabel.Name = "_lblCurrentLabel"
            Me._lblCurrentLabel.Size = New System.Drawing.Size(76, 15)
            Me._lblCurrentLabel.TabIndex = 0
            Me._lblCurrentLabel.Text = "เวอร์ชันปัจจุบัน:"
            Me._lblCurrentValue.AutoSize = True
            Me._lblCurrentValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblCurrentValue.Location = New System.Drawing.Point(130, 40)
            Me._lblCurrentValue.Name = "_lblCurrentValue"
            Me._lblCurrentValue.Size = New System.Drawing.Size(16, 15)
            Me._lblCurrentValue.TabIndex = 1
            Me._lblCurrentValue.Text = "..."
            Me._lblServerLabel.AutoSize = True
            Me._lblServerLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblServerLabel.Location = New System.Drawing.Point(16, 62)
            Me._lblServerLabel.Name = "_lblServerLabel"
            Me._lblServerLabel.Size = New System.Drawing.Size(78, 15)
            Me._lblServerLabel.TabIndex = 2
            Me._lblServerLabel.Text = "เวอร์ชัน Server:"
            Me._lblServerValue.AutoSize = True
            Me._lblServerValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblServerValue.Location = New System.Drawing.Point(130, 62)
            Me._lblServerValue.Name = "_lblServerValue"
            Me._lblServerValue.Size = New System.Drawing.Size(16, 15)
            Me._lblServerValue.TabIndex = 3
            Me._lblServerValue.Text = "..."
            Me._lblStatusLabel.AutoSize = True
            Me._lblStatusLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblStatusLabel.Location = New System.Drawing.Point(16, 84)
            Me._lblStatusLabel.Name = "_lblStatusLabel"
            Me._lblStatusLabel.Size = New System.Drawing.Size(39, 15)
            Me._lblStatusLabel.TabIndex = 4
            Me._lblStatusLabel.Text = "สถานะ:"
            Me._lblStatusValue.AutoSize = True
            Me._lblStatusValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblStatusValue.Location = New System.Drawing.Point(130, 84)
            Me._lblStatusValue.Name = "_lblStatusValue"
            Me._lblStatusValue.Size = New System.Drawing.Size(16, 15)
            Me._lblStatusValue.TabIndex = 5
            Me._lblStatusValue.Text = "..."
            Me._btnCheckNow.BackColor = System.Drawing.Color.White
            Me._btnCheckNow.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnCheckNow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(9, Byte), Integer), CType(CType(132, Byte), Integer), CType(CType(227, Byte), Integer))
            Me._btnCheckNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnCheckNow.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnCheckNow.ForeColor = System.Drawing.Color.FromArgb(CType(CType(9, Byte), Integer), CType(CType(132, Byte), Integer), CType(CType(227, Byte), Integer))
            Me._btnCheckNow.Location = New System.Drawing.Point(14, 310)
            Me._btnCheckNow.Name = "_btnCheckNow"
            Me._btnCheckNow.Size = New System.Drawing.Size(93, 32)
            Me._btnCheckNow.TabIndex = 4
            Me._btnCheckNow.Text = "ตรวจสอบ"
            Me._btnCheckNow.UseVisualStyleBackColor = False
            Me._btnRefreshInfo.BackColor = System.Drawing.Color.White
            Me._btnRefreshInfo.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnRefreshInfo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(92, Byte), Integer), CType(CType(231, Byte), Integer))
            Me._btnRefreshInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnRefreshInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnRefreshInfo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(108, Byte), Integer), CType(CType(92, Byte), Integer), CType(CType(231, Byte), Integer))
            Me._btnRefreshInfo.Location = New System.Drawing.Point(111, 310)
            Me._btnRefreshInfo.Name = "_btnRefreshInfo"
            Me._btnRefreshInfo.Size = New System.Drawing.Size(93, 32)
            Me._btnRefreshInfo.TabIndex = 5
            Me._btnRefreshInfo.Text = "รีเฟรช"
            Me._btnRefreshInfo.UseVisualStyleBackColor = False
            Me._btnUpdateNow.BackColor = System.Drawing.Color.FromArgb(CType(CType(9, Byte), Integer), CType(CType(132, Byte), Integer), CType(CType(227, Byte), Integer))
            Me._btnUpdateNow.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnUpdateNow.FlatAppearance.BorderSize = 0
            Me._btnUpdateNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnUpdateNow.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
            Me._btnUpdateNow.ForeColor = System.Drawing.Color.White
            Me._btnUpdateNow.Location = New System.Drawing.Point(14, 268)
            Me._btnUpdateNow.Name = "_btnUpdateNow"
            Me._btnUpdateNow.Size = New System.Drawing.Size(370, 34)
            Me._btnUpdateNow.TabIndex = 3
            Me._btnUpdateNow.Text = "อัปเดตทันที"
            Me._btnUpdateNow.UseVisualStyleBackColor = False
            Me._btnUpdateNow.Visible = False
            Me._btnDetails.BackColor = System.Drawing.Color.White
            Me._btnDetails.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnDetails.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
            Me._btnDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnDetails.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnDetails.ForeColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
            Me._btnDetails.Location = New System.Drawing.Point(208, 310)
            Me._btnDetails.Name = "_btnDetails"
            Me._btnDetails.Size = New System.Drawing.Size(93, 32)
            Me._btnDetails.TabIndex = 7
            Me._btnDetails.Text = "Details"
            Me._btnDetails.UseVisualStyleBackColor = False
            Me._btnConfigDebug.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(193, Byte), Integer), CType(CType(7, Byte), Integer))
            Me._btnConfigDebug.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnConfigDebug.FlatAppearance.BorderSize = 0
            Me._btnConfigDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnConfigDebug.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Bold)
            Me._btnConfigDebug.ForeColor = System.Drawing.Color.Black
            Me._btnConfigDebug.Location = New System.Drawing.Point(14, 350)
            Me._btnConfigDebug.Name = "_btnConfigDebug"
            Me._btnConfigDebug.Size = New System.Drawing.Size(370, 28)
            Me._btnConfigDebug.TabIndex = 8
            Me._btnConfigDebug.Text = "[Debug] ดู Config ที่โหลดแล้ว"
            Me._btnConfigDebug.UseVisualStyleBackColor = False
            Me._btnConfigDebug.Visible = False
            Me._btnLang.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(94, Byte), Integer))
            Me._btnLang.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnLang.FlatAppearance.BorderSize = 0
            Me._btnLang.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnLang.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnLang.ForeColor = System.Drawing.Color.White
            Me._btnLang.Location = New System.Drawing.Point(305, 310)
            Me._btnLang.Name = "_btnLang"
            Me._btnLang.Size = New System.Drawing.Size(79, 32)
            Me._btnLang.TabIndex = 6
            Me._btnLang.Text = "🌐 TH"
            Me._btnLang.UseVisualStyleBackColor = False
            Me._detailsMenu.Name = "_detailsMenu"
            Me._detailsMenu.Size = New System.Drawing.Size(61, 4)
            Me._progressBar.Location = New System.Drawing.Point(14, 350)
            Me._progressBar.MarqueeAnimationSpeed = 30
            Me._progressBar.Name = "_progressBar"
            Me._progressBar.Size = New System.Drawing.Size(370, 18)
            Me._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee
            Me._progressBar.TabIndex = 1
            Me._progressBar.Visible = False
            Me._lblProgress.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Italic)
            Me._lblProgress.ForeColor = System.Drawing.Color.FromArgb(CType(CType(100, Byte), Integer), CType(CType(100, Byte), Integer), CType(CType(100, Byte), Integer))
            Me._lblProgress.Location = New System.Drawing.Point(14, 370)
            Me._lblProgress.Name = "_lblProgress"
            Me._lblProgress.Size = New System.Drawing.Size(370, 18)
            Me._lblProgress.TabIndex = 2
            Me._lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me._lblProgress.Visible = False
            Me._fadeTimer.Interval = 30
            Me._gearMenu.Font = New System.Drawing.Font("Segoe UI", 9.5!)
            Me._gearMenu.Name = "_gearMenu"
            Me._gearMenu.Size = New System.Drawing.Size(61, 4)
            Me._btnGear.BackColor = System.Drawing.Color.Transparent
            Me._btnGear.Cursor = System.Windows.Forms.Cursors.Hand
            Me._btnGear.FlatAppearance.BorderSize = 0
            Me._btnGear.FlatStyle = System.Windows.Forms.FlatStyle.Flat
            Me._btnGear.Font = New System.Drawing.Font("Segoe UI Emoji", 14.0!)
            Me._btnGear.ForeColor = System.Drawing.Color.FromArgb(CType(CType(120, Byte), Integer), CType(CType(120, Byte), Integer), CType(CType(130, Byte), Integer))
            Me._btnGear.Location = New System.Drawing.Point(362, 0)
            Me._btnGear.Name = "_btnGear"
            Me._btnGear.Size = New System.Drawing.Size(36, 36)
            Me._btnGear.TabIndex = 10
            Me._btnGear.Text = "⚙"
            Me._btnGear.UseVisualStyleBackColor = False
            Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(245, Byte), Integer), CType(CType(250, Byte), Integer))
            Me.ClientSize = New System.Drawing.Size(400, 398)
            Me.Controls.Add(Me._btnGear)
            Me.Controls.Add(Me._btnConfigDebug)
            Me.Controls.Add(Me._progressBar)
            Me.Controls.Add(Me._lblProgress)
            Me.Controls.Add(Me._btnUpdateNow)
            Me.Controls.Add(Me._btnDetails)
            Me.Controls.Add(Me._grpInfo)
            Me.Controls.Add(Me._grpVersion)
            Me.Controls.Add(Me._btnCheckNow)
            Me.Controls.Add(Me._btnRefreshInfo)
            Me.Controls.Add(Me._btnLang)
            Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.Opacity = 0.0R
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "Auto Update"
            Me.TopMost = True
            Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
            Me._contextMenu.ResumeLayout(False)
            Me._grpInfo.ResumeLayout(False)
            Me._grpInfo.PerformLayout()
            Me._grpVersion.ResumeLayout(False)
            Me._grpVersion.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Private Sub LoadInfo()
            Try
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText

                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                _lblComNameValue.Text = computerName

                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester IsNot Nothing Then
                    _lblTypeValue.Text = tester.TesterType
                    _lblModeValue.Text = tester.Mode
                    _lblTimeValue.Text = tester.ScheduledTime.ToString("hh\:mm\:ss")
                Else
                    _lblTypeValue.Text = L("NotFoundInConfig")
                    _lblModeValue.Text = "-"
                    _lblTimeValue.Text = "-"
                End If

                Dim currentVer As String = Managers.VersionManager.ReadRegistryVersion()
                Dim serverVer As String = Managers.VersionManager.ReadLatestVersion()

                _lblCurrentValue.Text = If(String.IsNullOrEmpty(currentVer), L("VersionNotFound"), currentVer)
                _lblServerValue.Text = If(String.IsNullOrEmpty(serverVer), L("VersionReadError"), serverVer)

                Dim hasPendingUpdate As Boolean = Managers.UpdateFlagManager.GetFlag(computerName).GetValueOrDefault(False)

                If hasPendingUpdate Then
                    _lblStatusValue.Text = L("StatusPendingRestart")
                    _lblStatusValue.ForeColor = Color.FromArgb(230, 126, 34)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                ElseIf String.IsNullOrEmpty(currentVer) Then
                    _lblStatusValue.Text = L("StatusNotInstalled")
                    _lblStatusValue.ForeColor = Color.FromArgb(155, 89, 182)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = True
                ElseIf String.IsNullOrEmpty(serverVer) Then
                    _lblStatusValue.Text = L("StatusServerError")
                    _lblStatusValue.ForeColor = Color.FromArgb(149, 165, 166)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                ElseIf String.Equals(currentVer, serverVer, StringComparison.OrdinalIgnoreCase) Then
                    _lblStatusValue.Text = L("StatusUpToDate")
                    _lblStatusValue.ForeColor = Color.FromArgb(46, 204, 113)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                Else
                    _lblStatusValue.Text = L("StatusUpdateAvailable") & " (" & serverVer & ")"
                    _lblStatusValue.ForeColor = Color.FromArgb(41, 128, 185)
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = True
                End If

            Catch ex As Exception
                Managers.LogManager.[Error]("เกิดข้อผิดพลาดตอนโหลดข้อมูล UI", ex)
                _lblStatusValue.Text = "Error: " & ex.Message
                _lblStatusValue.ForeColor = Color.Red
            End Try
        End Sub

        Private Sub UpdateStatusBar()
            LoadInfo()
        End Sub

        Private Sub ApplyLanguage()
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText

            Me.Text = L("AppTitle")
            If _notifyIcon IsNot Nothing Then
                _notifyIcon.Text = L("AppTitle")
            End If

            _lblInfoTitle.Text = L("InfoTitle")
            _lblComNameLabel.Text = L("ComputerName")
            _lblTypeLabel.Text = L("Type")
            _lblModeLabel.Text = L("Mode")
            _lblTimeLabel.Text = L("ScheduleTime")

            _lblVersionTitle.Text = L("VersionTitle")
            _lblCurrentLabel.Text = L("CurrentVersion")
            _lblServerLabel.Text = L("ServerVersion")
            _lblStatusLabel.Text = L("Status")

            _btnUpdateNow.Text = L("BtnUpdateNow")
            _btnCheckNow.Text = L("BtnCheck")
            _btnRefreshInfo.Text = L("BtnRefresh")
            _btnDetails.Text = L("BtnDetails")
            _btnConfigDebug.Text = L("BtnDebugConfig")

            _mnuCheckNow.Text = L("MenuCheckNow")

            If _restartNoticeForm IsNot Nothing AndAlso Not _restartNoticeForm.IsDisposed Then
                _restartNoticeForm.UpdateLanguage()
            End If

            For Each frm As Form In Application.OpenForms
                If TypeOf frm Is RestartCountdownForm Then
                    DirectCast(frm, RestartCountdownForm).UpdateLanguage()
                End If
            Next
            _mnuExit.Text = L("MenuExit")

            Dim currentLang As String = Config.LanguageManager.CurrentLanguage
            If _btnLang IsNot Nothing Then
                _btnLang.Text = "🌐 " & currentLang.ToUpper()
            End If
        End Sub

        Private Sub SwitchLanguage(lang As String)
            Config.LanguageManager.CurrentLanguage = lang
            Config.AppSettings.UpdateLanguage(lang)
            ApplyLanguage()
            LoadInfo()
            Managers.LogManager.Info("Language switched and persisted: " & lang)
        End Sub



        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            MyBase.OnLoad(e)

            Me.DoubleBuffered = True
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
            Me.UpdateStyles()

            Try
                Dim doubleBufferProp As System.Reflection.PropertyInfo = GetType(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
                If doubleBufferProp IsNot Nothing Then
                    doubleBufferProp.SetValue(_grpInfo, True, Nothing)
                    doubleBufferProp.SetValue(_grpVersion, True, Nothing)
                End If
            Catch ex As Exception
            End Try

            Me.Visible = False

            AddHandler _grpInfo.Paint, AddressOf Panel_Paint
            AddHandler _grpVersion.Paint, AddressOf Panel_Paint
            AddHandler Me.Paint, AddressOf MainForm_Paint

            AddButtonAnimHandlers(_btnUpdateNow, Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219))
            AddButtonAnimHandlers(_btnCheckNow, Color.White, Color.FromArgb(235, 245, 253), Color.FromArgb(70, 130, 180), Color.FromArgb(41, 128, 185))
            AddButtonAnimHandlers(_btnRefreshInfo, Color.White, Color.FromArgb(245, 240, 255), Color.FromArgb(180, 180, 180), Color.FromArgb(108, 92, 231))
            AddButtonAnimHandlers(_btnDetails, Color.White, Color.FromArgb(232, 255, 240), Color.FromArgb(46, 204, 113), Color.FromArgb(39, 174, 96))

            AddHandler _btnCheckNow.Click, AddressOf BtnCheckNow_Click
            AddHandler _btnRefreshInfo.Click, AddressOf BtnRefreshInfo_Click
            AddHandler _btnUpdateNow.Click, AddressOf BtnUpdateNow_Click
            AddHandler _btnDetails.Click, AddressOf BtnDetails_Click
            AddHandler _btnConfigDebug.Click, AddressOf BtnConfigDebug_Click
            AddHandler _btnGear.Click, AddressOf BtnGear_Click

            SetupGearMenu()

            AddHandler _btnLang.Click, AddressOf BtnLang_Click

            _typewriteTimer = New System.Windows.Forms.Timer()
            _typewriteTimer.Interval = 35
            AddHandler _typewriteTimer.Tick, AddressOf TypewriteTimer_Tick

            _btnAnimTimer = New System.Windows.Forms.Timer()
            _btnAnimTimer.Interval = 15
            AddHandler _btnAnimTimer.Tick, AddressOf BtnAnimTimer_Tick

            _updateWorker = New Workers.UpdateWorker(Me)
            AddHandler _updateWorker.UpdateCompleted, AddressOf OnUpdateCompleted

            _scheduler = New Managers.SchedulerManager()
            AddHandler _scheduler.TickFired, AddressOf OnSchedulerTick
            _scheduler.Start()

            _restartCheckTimer = New System.Windows.Forms.Timer()
            _restartCheckTimer.Interval = 60000
            AddHandler _restartCheckTimer.Tick, AddressOf RestartCheckTimer_Tick
            _restartCheckTimer.Start()

            CheckAndTrackUpdateFlag()

            Config.LanguageManager.CurrentLanguage = Config.AppSettings.Language

            ApplyLanguage()
            LoadInfo()

            Managers.LogManager.Info("MainForm loaded. Scheduler started. Language=" & Config.LanguageManager.CurrentLanguage)
        End Sub

        Private _lastScheduledRunDate As DateTime = DateTime.MinValue

        Private Sub OnSchedulerTick(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester IsNot Nothing Then
                    Dim now As DateTime = DateTime.Now
                    Dim scheduled As TimeSpan = tester.ScheduledTime

                    If now.Hour = scheduled.Hours AndAlso now.Minute >= scheduled.Minutes Then
                        If _lastScheduledRunDate.Date <> now.Date Then
                            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                                _lastScheduledRunDate = now
                                Managers.LogManager.Info("Scheduler triggered update at: " & now.ToString("HH:mm:ss"))
                                _updateWorker.RunAsync()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Managers.LogManager.Error("Error during OnSchedulerTick scheduled check.", ex)
            End Try
        End Sub

        Public Sub UpdateProgressSafe(percent As Integer, statusText As String)
            If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Of Integer, String)(AddressOf UpdateProgressSafe), percent, statusText)
            Else
                If _progressBar IsNot Nothing Then
                    _progressBar.Style = ProgressBarStyle.Blocks
                    _progressBar.Value = Math.Max(0, Math.Min(100, percent))
                    _progressBar.Visible = True
                End If
                If _lblProgress IsNot Nothing Then
                    _lblProgress.Text = String.Format("{0} ({1}%)", statusText, percent)
                    _lblProgress.Visible = True
                End If
            End If
        End Sub

        Private Sub OnUpdateCompleted(ByVal sender As Object, ByVal e As Workers.UpdateCompletedEventArgs)
            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Of Object, Workers.UpdateCompletedEventArgs)(AddressOf OnUpdateCompleted), sender, e)
                Return
            End If
            LoadInfo()
            ShowProgress(False, "")
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            If _btnCheckNow IsNot Nothing Then
                _btnCheckNow.Enabled = True
                _btnCheckNow.Text = L("BtnCheck")
            End If
            If Me.Visible AndAlso Me.WindowState <> FormWindowState.Minimized Then
                Select Case e.Result
                    Case Strategies.UpdateResult.NoAction
                        Dim translatedMsg As String = TranslateMessage(e.Message)
                        MessageBox.Show(L("PromptCheckDone") & ": " & translatedMsg, L("TitleCheckResult"), MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case Strategies.UpdateResult.UpdateCompleted
                        MessageBox.Show(L("PromptSuccessCompleted"), L("TitleCheckResult"), MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case Strategies.UpdateResult.UpdateScheduledForRestart
                        ShowRestartNoticeForm()
                    Case Strategies.UpdateResult.[Error]
                        Dim translatedMsg As String = TranslateMessage(e.Message)
                        MessageBox.Show(L("TitleError") & ": " & translatedMsg, L("TitleCheckResult"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Select
            End If
            UpdateStatusBar()
        End Sub

        Private Function TranslateMessage(msg As String) As String
            If String.IsNullOrEmpty(msg) Then Return ""
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText

            Dim cleanKey As String = msg.Replace(" ", "").Replace("(", "").Replace(")", "").Replace(".", "")
            Dim translated As String = L("Msg" & cleanKey)
            If Not String.Equals(translated, "Msg" & cleanKey, StringComparison.OrdinalIgnoreCase) Then
                Return translated
            End If

            If msg.Contains("Not in config") Then Return L("MsgNotInConfig")
            If msg.Contains("Hour not matching") Then Return L("MsgHourNotMatching")
            If msg.Contains("Already checked today") Then Return L("MsgAlreadyCheckedToday")
            If msg.Contains("Up to Date") OrElse msg.Contains("เวอร์ชันล่าสุด") Then Return L("MsgUpToDate")
            If msg.Contains("Waiting for restart") OrElse msg.Contains("Pending restart") Then Return L("MsgPendingRestart")
            If msg.Contains("ไม่พบไฟล์อัปเดต") OrElse msg.Contains("Installer folder not found") Then Return L("MsgInstallerNotFound")
            If msg.Contains("Cancelled") Then Return L("MsgCancelled")

            Return msg
        End Function

        Private _restartNoticeForm As RestartNoticeForm = Nothing

        Private Sub ShowRestartNoticeForm()
            Try
                If _restartNoticeForm IsNot Nothing AndAlso Not _restartNoticeForm.IsDisposed Then
                    _restartNoticeForm.Show()
                    _restartNoticeForm.WindowState = FormWindowState.Normal
                    _restartNoticeForm.TopMost = True
                    _restartNoticeForm.BringToFront()
                    _restartNoticeForm.Activate()
                    Managers.LogManager.Info("RestartNoticeForm re-shown (existing instance).")
                    Return
                End If

                _restartNoticeForm = New RestartNoticeForm()
                _restartNoticeForm.Show()
                Managers.LogManager.Info("RestartNoticeForm displayed (new instance).")
            Catch ex As Exception
                Managers.LogManager.[Error]("Failed to show RestartNoticeForm: " & ex.Message)
            End Try
        End Sub

        Private Sub CheckAndTrackUpdateFlag()
            Try
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Dim flagResult As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)

                If flagResult.HasValue AndAlso flagResult.Value Then
                    _falseCount = 0
                    If _flagSetTime = DateTime.MinValue Then
                        _flagSetTime = DateTime.Now
                        Managers.LogManager.Info("Update flag detected. Tracking start: " & _flagSetTime.ToString("HH:mm:ss"))

                        Dim shortcutName As String = Config.AppSettings.StartupShortcutName
                        If String.IsNullOrEmpty(shortcutName) Then
                            shortcutName = Config.AppSettings.UninstallProductName
                        End If
                        If Not String.IsNullOrEmpty(shortcutName) Then
                            Managers.InstallerManager.RemoveStartupShortcut(shortcutName)
                        End If
                        Managers.LogManager.Info("Removed target app from Startup (flag is true)")
                    End If
                Else
                    _falseCount += 1
                    If _falseCount >= 3 Then
                        _flagSetTime = DateTime.MinValue
                        _restartPromptShown = False
                        _falseCount = 0
                    End If
                End If
            Catch ex As Exception
                Managers.LogManager.Warn("Error checking update flag: " & ex.Message)
            End Try
        End Sub

        Private Sub RestartCheckTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Try
                CheckAndTrackUpdateFlag()

                If _flagSetTime <> DateTime.MinValue AndAlso Not _restartPromptShown Then
                    Dim elapsed As TimeSpan = DateTime.Now - _flagSetTime
                    If elapsed.TotalMinutes >= 60 Then
                        _restartPromptShown = True
                        Managers.LogManager.Info("Update flag has been set for " & elapsed.TotalMinutes.ToString("F0") & " minutes. Showing RestartNoticeForm.")

                        If Me.InvokeRequired Then
                            Me.BeginInvoke(New Action(AddressOf ShowRestartNoticeForm))
                        Else
                            ShowRestartNoticeForm()
                        End If
                    End If
                End If
            Catch ex As Exception
                Managers.LogManager.Warn("RestartCheckTimer error: " & ex.Message)
            End Try
        End Sub

        Private Sub NotifyIcon_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles _notifyIcon.DoubleClick
            ShowForm()
        End Sub

        Private Sub ShowForm()
            LoadInfo()

            _tempComName = _lblComNameValue.Text
            _tempType = _lblTypeValue.Text
            _tempMode = _lblModeValue.Text
            _tempTime = _lblTimeValue.Text
            _tempCurrentVer = _lblCurrentValue.Text
            _tempServerVer = _lblServerValue.Text
            _tempStatus = _lblStatusValue.Text

            _lblComNameValue.Text = ""
            _lblTypeValue.Text = ""
            _lblModeValue.Text = ""
            _lblTimeValue.Text = ""
            _lblCurrentValue.Text = ""
            _lblServerValue.Text = ""
            _lblStatusValue.Text = ""

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

        Private Sub TriggerTypewriter()
            StartTypewriter(_lblComNameValue, _tempComName)
            StartTypewriter(_lblTypeValue, _tempType)
            StartTypewriter(_lblModeValue, _tempMode)
            StartTypewriter(_lblTimeValue, _tempTime)
            StartTypewriter(_lblCurrentValue, _tempCurrentVer)
            StartTypewriter(_lblServerValue, _tempServerVer)
            StartTypewriter(_lblStatusValue, _tempStatus)
        End Sub

        Private Sub FadeTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles _fadeTimer.Tick
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

            Dim isInfoDone As Boolean = False
            If _grpInfo.Top > infoTarget Then
                _grpInfo.Top = Math.Max(infoTarget, _grpInfo.Top - stepY)
            Else
                _grpInfo.Top = infoTarget
                isInfoDone = True
            End If

            Dim isVersionDone As Boolean = False
            If _grpInfo.Top <= 26 Then
                If _grpVersion.Top > versionTarget Then
                    _grpVersion.Top = Math.Max(versionTarget, _grpVersion.Top - stepY)
                Else
                    _grpVersion.Top = versionTarget
                    isVersionDone = True
                End If
            End If

            Me.Invalidate()

            If isOpacityDone AndAlso isInfoDone AndAlso isVersionDone Then
                Me._fadeTimer.Stop()
                TriggerTypewriter()
            End If
        End Sub

        Private Sub MnuCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuCheckNow.Click
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user.")
                _updateWorker.RunAsync(True)
            End If
        End Sub

        Private Sub MnuExit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuExit.Click
            CleanupAndExit()
        End Sub

        Private Sub BtnCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user (button).")
                _btnCheckNow.Enabled = False
                _btnCheckNow.Text = L("PromptChecking")
                ShowProgress(True, L("PromptCheckingUpdate"))
                _updateWorker.RunAsync(True)
            Else
                MessageBox.Show(L("PromptAlreadyChecking"), L("PromptNotice"), MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Sub

        Private Sub BtnRefreshInfo_Click(ByVal sender As Object, ByVal e As EventArgs)
            Managers.ConfigManager.InvalidateCache()
            LoadInfo()

            _tempComName = _lblComNameValue.Text
            _tempType = _lblTypeValue.Text
            _tempMode = _lblModeValue.Text
            _tempTime = _lblTimeValue.Text
            _tempCurrentVer = _lblCurrentValue.Text
            _tempServerVer = _lblServerValue.Text
            _tempStatus = _lblStatusValue.Text

            TriggerTypewriter()

        End Sub

        Private Sub BtnDetails_Click(ByVal sender As Object, ByVal e As EventArgs)
            _detailsMenu.Items.Clear()

            AddPdfFolderMenu(_detailsMenu, "Info", Config.AppSettings.DetailInfoPdfPath)

            _detailsMenu.Items.Add(New ToolStripSeparator())

            AddPdfFolderMenu(_detailsMenu, "Detail", Config.AppSettings.DetailPdfPath)

            _detailsMenu.Show(_btnDetails, New System.Drawing.Point(0, _btnDetails.Height))
        End Sub

        Private Sub AddPdfFolderMenu(menu As ContextMenuStrip, groupName As String, folderPath As String)
            If String.IsNullOrEmpty(folderPath) Then
                Dim mnu As New ToolStripMenuItem(groupName & "  (ยังไม่ได้ตั้ง path)")
                mnu.Enabled = False
                menu.Items.Add(mnu)
                Return
            End If

            If IO.File.Exists(folderPath) Then
                Dim filePath As String = folderPath
                Dim mnu As New ToolStripMenuItem(groupName & "  —  " & IO.Path.GetFileName(filePath))
                AddHandler mnu.Click, Sub(s, ev)
                                          OpenSinglePdf(filePath)
                                      End Sub
                menu.Items.Add(mnu)
                Return
            End If

            If Not IO.Directory.Exists(folderPath) Then
                Dim mnu As New ToolStripMenuItem(groupName & "  (หาโฟลเดอร์ไม่เจอ)")
                mnu.Enabled = False
                menu.Items.Add(mnu)
                Return
            End If

            Dim pdfFiles = New IO.DirectoryInfo(folderPath).GetFiles("*.pdf")
            If pdfFiles.Length = 0 Then
                Dim mnu As New ToolStripMenuItem(groupName & "  (ไม่มีไฟล์ PDF)")
                mnu.Enabled = False
                menu.Items.Add(mnu)
                Return
            End If

            Array.Sort(pdfFiles, Function(a, b) b.LastWriteTime.CompareTo(a.LastWriteTime))

            If pdfFiles.Length = 1 Then
                Dim f As IO.FileInfo = pdfFiles(0)
                Dim filePath As String = f.FullName
                Dim mnu As New ToolStripMenuItem(groupName & "  —  " & f.Name)
                AddHandler mnu.Click, Sub(s, ev)
                                          OpenSinglePdf(filePath)
                                      End Sub
                menu.Items.Add(mnu)
            Else
                Dim parent As New ToolStripMenuItem(groupName & "  (" & pdfFiles.Length & " ไฟล์)")
                For Each f As IO.FileInfo In pdfFiles
                    Dim filePath As String = f.FullName
                    Dim label As String = f.Name & "  [" & f.LastWriteTime.ToString("dd/MM/yy HH:mm") & "]"
                    Dim child As New ToolStripMenuItem(label)
                    AddHandler child.Click, Sub(s, ev)
                                                OpenSinglePdf(filePath)
                                            End Sub
                    parent.DropDownItems.Add(child)
                Next
                menu.Items.Add(parent)
            End If
        End Sub

        Private Sub OpenSinglePdf(filePath As String)
            Try
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                If Not IO.File.Exists(filePath) Then
                    MessageBox.Show(L("PromptFileNotFound") & filePath, L("PromptFileNotFoundTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                Managers.LogManager.Info("เปิด PDF: " & filePath)

                Try
                    Process.Start(filePath)
                    Return
                Catch ex As System.ComponentModel.Win32Exception
                    Managers.LogManager.Warn("No default PDF app: " & ex.Message & ". Trying fallback...")
                End Try

                Dim browsers As String() = {
                    "C:\Program Files\Internet Explorer\iexplore.exe",
                    "C:\Program Files (x86)\Internet Explorer\iexplore.exe",
                    "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                    "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
                }
                For Each browser As String In browsers
                    If IO.File.Exists(browser) Then
                        Process.Start(browser, """" & filePath & """")
                        Managers.LogManager.Info("Opened PDF with browser: " & browser)
                        Return
                    End If
                Next

                Try
                    Dim psi As New ProcessStartInfo()
                    psi.FileName = "rundll32.exe"
                    psi.Arguments = "shell32.dll,OpenAs_RunDLL " & filePath
                    Process.Start(psi)
                    Managers.LogManager.Info("Opened PDF with OpenAs dialog")
                    Return
                Catch ex2 As Exception
                    Managers.LogManager.Warn("OpenAs fallback failed: " & ex2.Message)
                End Try

                Try
                    Process.Start("explorer.exe", "/select,""" & filePath & """")
                    Managers.LogManager.Info("Opened containing folder for: " & filePath)
                Catch ex3 As Exception
                    Managers.LogManager.[Error]("All fallbacks failed for: " & filePath, ex3)
                End Try

            Catch ex As Exception
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Managers.LogManager.[Error]("Failed to open PDF: " & filePath, ex)
                MessageBox.Show(L("PromptCantOpenFile") & ex.Message, L("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End Sub

        Private Sub OpenPdfFile(pdfPath As String, displayName As String)
            Try
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                If String.IsNullOrEmpty(pdfPath) Then
                    Dim msg As String = String.Format(L("PromptPathNotConfigured"), displayName)
                    MessageBox.Show(msg, L("PromptPathNotFoundTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                Dim fileToOpen As String = pdfPath

                If IO.Directory.Exists(pdfPath) Then
                    Dim pdfFiles = New IO.DirectoryInfo(pdfPath).GetFiles("*.pdf")
                    If pdfFiles.Length = 0 Then
                        MessageBox.Show(L("PromptNoPdfInFolder") & pdfPath, L("PromptFileNotFoundTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                    Dim latestPdf As IO.FileInfo = pdfFiles(0)
                    For Each f In pdfFiles
                        If f.LastWriteTime > latestPdf.LastWriteTime Then
                            latestPdf = f
                        End If
                    Next
                    fileToOpen = latestPdf.FullName
                    Managers.LogManager.Info("เปิด PDF ล่าสุดจากโฟลเดอร์: " & fileToOpen)
                End If

                If Not IO.File.Exists(fileToOpen) Then
                    MessageBox.Show(L("PromptFileNotFound") & fileToOpen, L("PromptFileNotFoundTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                Process.Start(fileToOpen)
            Catch ex As Exception
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Managers.LogManager.[Error]("Failed to open " & displayName & ": " & pdfPath, ex)
                MessageBox.Show(L("PromptCantOpenFile") & ex.Message, L("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End Sub

        Private Sub SetupGearMenu()
            If _gearMenu Is Nothing Then Return
            _gearMenu.Items.Clear()
            _gearMenu.Items.Add("[Debug] ดู Config ที่โหลดแล้ว", Nothing, AddressOf BtnConfigDebug_Click)
            _gearMenu.Items.Add("[Test] แสดง RestartNoticeForm", Nothing, AddressOf BtnTestRestart_Click)
        End Sub

        Private Sub BtnGear_Click(ByVal sender As Object, ByVal e As EventArgs)
            If _gearMenu IsNot Nothing Then
                _gearMenu.Show(Cursor.Position)
            End If
        End Sub

        Private Sub BtnTestRestart_Click(ByVal sender As Object, ByVal e As EventArgs)
            ShowRestartNoticeForm()
        End Sub

        Private Sub BtnConfigDebug_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Dim sb As New System.Text.StringBuilder()

                sb.AppendLine(L("DebugExeLocation"))
                sb.AppendLine(System.Reflection.Assembly.GetExecutingAssembly().Location)
                sb.AppendLine()
                sb.AppendLine(L("DebugConfigStatus"))
                sb.AppendLine(Config.AppSettings.LoadStatus)
                sb.AppendLine()

                sb.AppendLine(L("DebugReadValues"))
                For Each issue As String In Config.AppSettings.ValidateConfig()
                    sb.AppendLine(issue)
                Next

                MessageBox.Show(sb.ToString(), L("DebugTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                MessageBox.Show("Error: " & ex.Message, L("DebugTitle"), MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End Sub

        Private Sub BtnUpdateNow_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)

                If tester Is Nothing Then
                    MessageBox.Show(L("MachineNotInSystem"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                Dim result = MessageBox.Show(L("ConfirmUpdate"), L("ConfirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    _btnUpdateNow.Enabled = False
                    _btnUpdateNow.Text = L("Updating")
                    ShowProgress(True, L("Updating"))

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
            Managers.InstallerManager.KillTargetProcess()
            Managers.InstallerManager.CloseProgramOfRegistryPath()
            e.Result = Managers.InstallerManager.RunInstaller(testerType, _
                Sub(percent, msg)
                    Me.UpdateProgressSafe(percent, msg)
                End Sub)
        End Sub

        Private Sub ManualUpdate_Completed(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs)
            ShowProgress(False, "")
            If e.Error IsNot Nothing Then
                Managers.LogManager.[Error]("Manual update error.", e.Error)
                MessageBox.Show("Error: " & e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ResetUpdateButton()
                Return
            End If

            Dim L As Func(Of String, String) = AddressOf Config.LanguageManager.GetText
            Dim success As Boolean = DirectCast(e.Result, Boolean)
            If success Then
                MessageBox.Show(L("PromptSuccess"), L("TitleSuccess"), MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Managers.UpdateFlagManager.SetFlag(computerName, False)
                LoadInfo()
            Else
                MessageBox.Show(L("PromptFailed"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            ResetUpdateButton()
        End Sub

        Private Sub ResetUpdateButton()
            If _btnUpdateNow IsNot Nothing Then
                _btnUpdateNow.Enabled = True
                _btnUpdateNow.Text = "อัปเดตทันที"
            End If
        End Sub

        Private Sub ShowProgress(show As Boolean, statusText As String)
            If _progressBar IsNot Nothing Then _progressBar.Visible = show
            If _lblProgress IsNot Nothing Then
                _lblProgress.Text = statusText
                _lblProgress.Visible = show
            End If
        End Sub


        Private Sub BtnLang_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim currentLang As String = Config.LanguageManager.CurrentLanguage.ToLower()
            Dim nextLang As String = "th"
            If currentLang = "th" Then
                nextLang = "en"
            ElseIf currentLang = "en" Then
                nextLang = "jp"
            ElseIf currentLang = "jp" Then
                nextLang = "th"
            End If

            SwitchLanguage(nextLang)
        End Sub


        Private Sub BtnExit_Click(ByVal sender As Object, ByVal e As EventArgs)
            CleanupAndExit()
        End Sub


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
                If _btnLang IsNot Nothing Then RemoveHandler _btnLang.Click, AddressOf BtnLang_Click
                If _btnUpdateNow IsNot Nothing Then RemoveHandler _btnUpdateNow.Click, AddressOf BtnUpdateNow_Click
                If _btnDetails IsNot Nothing Then RemoveHandler _btnDetails.Click, AddressOf BtnDetails_Click
                If _detailsMenu IsNot Nothing Then _detailsMenu.Dispose()
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

        Private Sub StartTypewriter(ByVal lbl As Label, ByVal text As String)
            If lbl Is Nothing Then Return
            lbl.Text = ""
            _typewriteTargets(lbl) = text
            _typewriteIndices(lbl) = 0
            If _typewriteTimer IsNot Nothing Then
                _typewriteTimer.Start()
            End If
        End Sub

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


        Private Sub MainForm_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
            Dim rect As New Rectangle(0, 0, Me.Width, Me.Height)
            Using brush As New LinearGradientBrush(rect, Color.FromArgb(245, 247, 250), Color.FromArgb(232, 236, 243), 90.0F)
                e.Graphics.FillRectangle(brush, rect)
            End Using


            DrawPanelShadow(e.Graphics, _grpInfo.Bounds)
            DrawPanelShadow(e.Graphics, _grpVersion.Bounds)
        End Sub


        Private Sub DrawPanelShadow(ByVal g As Graphics, ByVal rect As Rectangle)

            For i As Integer = 1 To 5
                Using pen As New Pen(Color.FromArgb(CInt(10 - (i * 2)), 0, 0, 0), i * 2)

                    g.DrawLine(pen, rect.Left + 4, rect.Bottom + i, rect.Right + i, rect.Bottom + i)
                    g.DrawLine(pen, rect.Right + i, rect.Top + 4, rect.Right + i, rect.Bottom + i)
                End Using
            Next
        End Sub


        Private Sub Panel_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
            Dim pnl As Panel = DirectCast(sender, Panel)
            Dim rect As New Rectangle(0, 0, pnl.Width, pnl.Height)
            Using brush As New LinearGradientBrush(rect, Color.White, Color.FromArgb(248, 250, 253), 45.0F)
                e.Graphics.FillRectangle(brush, rect)
            End Using

            Using borderPen As New Pen(Color.FromArgb(218, 224, 233), 1)
                e.Graphics.DrawRectangle(borderPen, 0, 0, pnl.Width - 1, pnl.Height - 1)
            End Using
        End Sub


        Private Sub AddButtonAnimHandlers(ByVal btn As Button, ByVal normalColor As Color, ByVal hoverColor As Color, ByVal normalBorder As Color, ByVal hoverBorder As Color)
            If btn Is Nothing Then Return

            btn.BackColor = normalColor
            btn.FlatAppearance.BorderColor = normalBorder
            btn.FlatAppearance.MouseOverBackColor = normalColor
            btn.FlatAppearance.MouseDownBackColor = normalColor

            _btnTargets(btn) = normalColor
            _btnBorders(btn) = normalBorder
            _btnTargetBorders(btn) = normalBorder

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
                                          btn.Padding = New Padding(0, 2, 0, 0)
                                      End Sub
            AddHandler btn.MouseUp, Sub(s, ev)
                                        btn.Padding = New Padding(0, 0, 0, 0)
                                    End Sub
        End Sub

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

        Private Sub BtnAnimTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Dim btns As New System.Collections.Generic.List(Of Button)(_btnTargets.Keys)
            Dim allColorsReached As Boolean = True
            Dim stepVal As Integer = 15

            For Each btn In btns
                Dim currentBack As Color = btn.BackColor
                Dim targetBack As Color = _btnTargets(btn)
                Dim currentBorder As Color = btn.FlatAppearance.BorderColor
                Dim targetBorder As Color = _btnTargetBorders(btn)

                If currentBack <> targetBack Then
                    Dim nextColor As Color = InterpolateColor(currentBack, targetBack, stepVal)
                    btn.BackColor = nextColor
                    btn.FlatAppearance.MouseOverBackColor = nextColor
                    btn.FlatAppearance.MouseDownBackColor = nextColor
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
