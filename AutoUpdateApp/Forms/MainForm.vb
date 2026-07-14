Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports System.Drawing

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
        Private _grpInfo As GroupBox
        Private _lblComNameLabel As Label
        Private _lblComNameValue As Label
        Private _lblTypeLabel As Label
        Private _lblTypeValue As Label
        Private _lblModeLabel As Label
        Private _lblModeValue As Label
        Private _lblTimeLabel As Label
        Private _lblTimeValue As Label

        Private _grpVersion As GroupBox
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
            Me._grpInfo = New System.Windows.Forms.GroupBox()
            Me._lblComNameLabel = New System.Windows.Forms.Label()
            Me._lblComNameValue = New System.Windows.Forms.Label()
            Me._lblTypeLabel = New System.Windows.Forms.Label()
            Me._lblTypeValue = New System.Windows.Forms.Label()
            Me._lblModeLabel = New System.Windows.Forms.Label()
            Me._lblModeValue = New System.Windows.Forms.Label()
            Me._lblTimeLabel = New System.Windows.Forms.Label()
            Me._lblTimeValue = New System.Windows.Forms.Label()
            Me._grpVersion = New System.Windows.Forms.GroupBox()
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
            Me._grpInfo.Controls.Add(Me._lblComNameLabel)
            Me._grpInfo.Controls.Add(Me._lblComNameValue)
            Me._grpInfo.Controls.Add(Me._lblTypeLabel)
            Me._grpInfo.Controls.Add(Me._lblTypeValue)
            Me._grpInfo.Controls.Add(Me._lblModeLabel)
            Me._grpInfo.Controls.Add(Me._lblModeValue)
            Me._grpInfo.Controls.Add(Me._lblTimeLabel)
            Me._grpInfo.Controls.Add(Me._lblTimeValue)
            Me._grpInfo.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
            Me._grpInfo.Location = New System.Drawing.Point(14, 14)
            Me._grpInfo.Name = "_grpInfo"
            Me._grpInfo.Size = New System.Drawing.Size(370, 130)
            Me._grpInfo.TabIndex = 1
            Me._grpInfo.TabStop = False
            Me._grpInfo.Text = " ข้อมูลเครื่อง "
            '
            '_lblComNameLabel
            '
            Me._lblComNameLabel.AutoSize = True
            Me._lblComNameLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblComNameLabel.Location = New System.Drawing.Point(16, 28)
            Me._lblComNameLabel.Name = "_lblComNameLabel"
            Me._lblComNameLabel.Size = New System.Drawing.Size(49, 15)
            Me._lblComNameLabel.TabIndex = 0
            Me._lblComNameLabel.Text = "ชื่อเครื่อง:"
            '
            '_lblComNameValue
            '
            Me._lblComNameValue.AutoSize = True
            Me._lblComNameValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblComNameValue.Location = New System.Drawing.Point(130, 28)
            Me._lblComNameValue.Name = "_lblComNameValue"
            Me._lblComNameValue.Size = New System.Drawing.Size(16, 15)
            Me._lblComNameValue.TabIndex = 1
            Me._lblComNameValue.Text = "..."
            '
            '_lblTypeLabel
            '
            Me._lblTypeLabel.AutoSize = True
            Me._lblTypeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTypeLabel.Location = New System.Drawing.Point(16, 52)
            Me._lblTypeLabel.Name = "_lblTypeLabel"
            Me._lblTypeLabel.Size = New System.Drawing.Size(43, 15)
            Me._lblTypeLabel.TabIndex = 2
            Me._lblTypeLabel.Text = "ประเภท:"
            '
            '_lblTypeValue
            '
            Me._lblTypeValue.AutoSize = True
            Me._lblTypeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTypeValue.Location = New System.Drawing.Point(130, 52)
            Me._lblTypeValue.Name = "_lblTypeValue"
            Me._lblTypeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTypeValue.TabIndex = 3
            Me._lblTypeValue.Text = "..."
            '
            '_lblModeLabel
            '
            Me._lblModeLabel.AutoSize = True
            Me._lblModeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblModeLabel.Location = New System.Drawing.Point(16, 76)
            Me._lblModeLabel.Name = "_lblModeLabel"
            Me._lblModeLabel.Size = New System.Drawing.Size(36, 15)
            Me._lblModeLabel.TabIndex = 4
            Me._lblModeLabel.Text = "โหมด:"
            '
            '_lblModeValue
            '
            Me._lblModeValue.AutoSize = True
            Me._lblModeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblModeValue.Location = New System.Drawing.Point(130, 76)
            Me._lblModeValue.Name = "_lblModeValue"
            Me._lblModeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblModeValue.TabIndex = 5
            Me._lblModeValue.Text = "..."
            '
            '_lblTimeLabel
            '
            Me._lblTimeLabel.AutoSize = True
            Me._lblTimeLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblTimeLabel.Location = New System.Drawing.Point(16, 100)
            Me._lblTimeLabel.Name = "_lblTimeLabel"
            Me._lblTimeLabel.Size = New System.Drawing.Size(71, 15)
            Me._lblTimeLabel.TabIndex = 6
            Me._lblTimeLabel.Text = "เวลาตรวจสอบ:"
            '
            '_lblTimeValue
            '
            Me._lblTimeValue.AutoSize = True
            Me._lblTimeValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblTimeValue.Location = New System.Drawing.Point(130, 100)
            Me._lblTimeValue.Name = "_lblTimeValue"
            Me._lblTimeValue.Size = New System.Drawing.Size(16, 15)
            Me._lblTimeValue.TabIndex = 7
            Me._lblTimeValue.Text = "..."
            '
            '_grpVersion
            '
            Me._grpVersion.Controls.Add(Me._lblCurrentLabel)
            Me._grpVersion.Controls.Add(Me._lblCurrentValue)
            Me._grpVersion.Controls.Add(Me._lblServerLabel)
            Me._grpVersion.Controls.Add(Me._lblServerValue)
            Me._grpVersion.Controls.Add(Me._lblStatusLabel)
            Me._grpVersion.Controls.Add(Me._lblStatusValue)
            Me._grpVersion.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
            Me._grpVersion.Location = New System.Drawing.Point(14, 152)
            Me._grpVersion.Name = "_grpVersion"
            Me._grpVersion.Size = New System.Drawing.Size(370, 104)
            Me._grpVersion.TabIndex = 2
            Me._grpVersion.TabStop = False
            Me._grpVersion.Text = " เวอร์ชัน "
            '
            '_lblCurrentLabel
            '
            Me._lblCurrentLabel.AutoSize = True
            Me._lblCurrentLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblCurrentLabel.Location = New System.Drawing.Point(16, 28)
            Me._lblCurrentLabel.Name = "_lblCurrentLabel"
            Me._lblCurrentLabel.Size = New System.Drawing.Size(76, 15)
            Me._lblCurrentLabel.TabIndex = 0
            Me._lblCurrentLabel.Text = "เวอร์ชันปัจจุบัน:"
            '
            '_lblCurrentValue
            '
            Me._lblCurrentValue.AutoSize = True
            Me._lblCurrentValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblCurrentValue.Location = New System.Drawing.Point(150, 28)
            Me._lblCurrentValue.Name = "_lblCurrentValue"
            Me._lblCurrentValue.Size = New System.Drawing.Size(16, 15)
            Me._lblCurrentValue.TabIndex = 1
            Me._lblCurrentValue.Text = "..."
            '
            '_lblServerLabel
            '
            Me._lblServerLabel.AutoSize = True
            Me._lblServerLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblServerLabel.Location = New System.Drawing.Point(16, 52)
            Me._lblServerLabel.Name = "_lblServerLabel"
            Me._lblServerLabel.Size = New System.Drawing.Size(78, 15)
            Me._lblServerLabel.TabIndex = 2
            Me._lblServerLabel.Text = "เวอร์ชัน Server:"
            '
            '_lblServerValue
            '
            Me._lblServerValue.AutoSize = True
            Me._lblServerValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblServerValue.Location = New System.Drawing.Point(150, 52)
            Me._lblServerValue.Name = "_lblServerValue"
            Me._lblServerValue.Size = New System.Drawing.Size(16, 15)
            Me._lblServerValue.TabIndex = 3
            Me._lblServerValue.Text = "..."
            '
            '_lblStatusLabel
            '
            Me._lblStatusLabel.AutoSize = True
            Me._lblStatusLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._lblStatusLabel.Location = New System.Drawing.Point(16, 76)
            Me._lblStatusLabel.Name = "_lblStatusLabel"
            Me._lblStatusLabel.Size = New System.Drawing.Size(39, 15)
            Me._lblStatusLabel.TabIndex = 4
            Me._lblStatusLabel.Text = "สถานะ:"
            '
            '_lblStatusValue
            '
            Me._lblStatusValue.AutoSize = True
            Me._lblStatusValue.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._lblStatusValue.Location = New System.Drawing.Point(150, 76)
            Me._lblStatusValue.Name = "_lblStatusValue"
            Me._lblStatusValue.Size = New System.Drawing.Size(16, 15)
            Me._lblStatusValue.TabIndex = 5
            Me._lblStatusValue.Text = "..."
            '
            '_btnUpdateNow
            '
            Me._btnUpdateNow.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
            Me._btnUpdateNow.Location = New System.Drawing.Point(14, 268)
            Me._btnUpdateNow.Name = "_btnUpdateNow"
            Me._btnUpdateNow.Size = New System.Drawing.Size(370, 32)
            Me._btnUpdateNow.TabIndex = 3
            Me._btnUpdateNow.Text = "อัปเดตทันที"
            Me._btnUpdateNow.BackColor = System.Drawing.Color.LightSteelBlue
            '
            '_btnCheckNow
            '
            Me._btnCheckNow.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._btnCheckNow.Location = New System.Drawing.Point(14, 310)
            Me._btnCheckNow.Name = "_btnCheckNow"
            Me._btnCheckNow.Size = New System.Drawing.Size(120, 32)
            Me._btnCheckNow.TabIndex = 4
            Me._btnCheckNow.Text = "ตรวจสอบอัปเดต"
            '
            '_btnRefreshInfo
            '
            Me._btnRefreshInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._btnRefreshInfo.Location = New System.Drawing.Point(144, 310)
            Me._btnRefreshInfo.Name = "_btnRefreshInfo"
            Me._btnRefreshInfo.Size = New System.Drawing.Size(110, 32)
            Me._btnRefreshInfo.TabIndex = 5
            Me._btnRefreshInfo.Text = "รีเฟรชข้อมูล"
            '
            '_btnExit
            '
            Me._btnExit.Font = New System.Drawing.Font("Segoe UI", 9.0!)
            Me._btnExit.Location = New System.Drawing.Point(314, 310)
            Me._btnExit.Name = "_btnExit"
            Me._btnExit.Size = New System.Drawing.Size(70, 32)
            Me._btnExit.TabIndex = 6
            Me._btnExit.Text = "ออก"
            '
            'MainForm
            '
            Me.ClientSize = New System.Drawing.Size(400, 356)
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
                    _lblStatusValue.Text = "ไม่สามารถตรวจสอบได้"
                    _lblStatusValue.ForeColor = Color.Gray
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                ElseIf String.Equals(currentVer, serverVer, StringComparison.OrdinalIgnoreCase) Then
                    _lblStatusValue.Text = "✓ เวอร์ชันล่าสุดแล้ว"
                    _lblStatusValue.ForeColor = Color.Green
                    If _btnUpdateNow IsNot Nothing Then _btnUpdateNow.Enabled = False
                Else
                    _lblStatusValue.Text = "✗ มีอัปเดตใหม่"
                    _lblStatusValue.ForeColor = Color.Red
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

            ' ผูก Event ปุ่ม
            AddHandler _btnCheckNow.Click, AddressOf BtnCheckNow_Click
            AddHandler _btnRefreshInfo.Click, AddressOf BtnRefreshInfo_Click
            AddHandler _btnExit.Click, AddressOf BtnExit_Click
            AddHandler _btnUpdateNow.Click, AddressOf BtnUpdateNow_Click

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
            Me.Visible = True
            Me.ShowInTaskbar = True
            Me.Opacity = 1.0R
            Me.WindowState = FormWindowState.Normal
            Me.BringToFront()
            Me.Activate()
        End Sub

        ' ── เมนู Tray: ตรวจสอบตอนนี้ ──
        Private Sub MnuCheckNow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _mnuCheckNow.Click
            If _updateWorker IsNot Nothing AndAlso Not _updateWorker.IsBusy Then
                Managers.LogManager.Info("Manual check triggered by user.")
                _updateWorker.RunAsync()
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
                _updateWorker.RunAsync()
            Else
                MessageBox.Show("กำลังตรวจสอบอยู่แล้ว กรุณารอสักครู่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Sub

        ' ── ปุ่ม: รีเฟรชข้อมูล ──
        Private Sub BtnRefreshInfo_Click(ByVal sender As Object, ByVal e As EventArgs)
            Managers.ConfigManager.InvalidateCache()
            LoadInfo()
            MessageBox.Show("รีเฟรชข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        ' ── ปุ่ม: อัปเดตทันที ──
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
                    Application.DoEvents()
                    
                    Dim success = Managers.InstallerManager.RunInstaller(tester.TesterType)
                    
                    If success Then
                        MessageBox.Show("อัปเดตสำเร็จเรียบร้อยแล้ว", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ' ล้าง Flag เผื่อมีค้างอยู่
                        Managers.UpdateFlagManager.SetFlag(computerName, False)
                        LoadInfo()
                    Else
                        MessageBox.Show("อัปเดตไม่สำเร็จ กรุณาตรวจสอบ Log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        _btnUpdateNow.Enabled = True
                        _btnUpdateNow.Text = "อัปเดตทันที"
                    End If
                End If
            Catch ex As Exception
                Managers.LogManager.[Error]("Manual update failed.", ex)
                MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                _btnUpdateNow.Enabled = True
                _btnUpdateNow.Text = "อัปเดตทันที"
            End Try
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
                If _contextMenu IsNot Nothing Then _contextMenu.Dispose()
                If _notifyIcon IsNot Nothing Then
                    _notifyIcon.Visible = False
                    _notifyIcon.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
