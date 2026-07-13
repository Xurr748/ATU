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

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me._contextMenu = New ContextMenuStrip(Me.components)
            Me._mnuCheckNow = New ToolStripMenuItem()
            Me._mnuSeparator = New ToolStripSeparator()
            Me._mnuExit = New ToolStripMenuItem()
            Me._notifyIcon = New NotifyIcon(Me.components)

            Me._contextMenu.SuspendLayout()
            Me.SuspendLayout()

            ' ── ContextMenu ──
            Me._contextMenu.Items.AddRange(New ToolStripItem() {Me._mnuCheckNow, Me._mnuSeparator, Me._mnuExit})
            Me._contextMenu.Name = "_contextMenu"
            Me._contextMenu.Size = New Size(150, 54)

            Me._mnuCheckNow.Name = "_mnuCheckNow"
            Me._mnuCheckNow.Text = "ตรวจสอบตอนนี้"

            Me._mnuSeparator.Name = "_mnuSeparator"

            Me._mnuExit.Name = "_mnuExit"
            Me._mnuExit.Text = "ออก"

            ' ── NotifyIcon (ใช้ System Icon แทน resource) ──
            Me._notifyIcon.ContextMenuStrip = Me._contextMenu
            Me._notifyIcon.Text = "Auto Update"
            Me._notifyIcon.Visible = True
            Try
                Me._notifyIcon.Icon = Drawing.SystemIcons.Application
            Catch
            End Try

            Dim fontNormal As New Font("Segoe UI", 9.0F, FontStyle.Regular)
            Dim fontBold As New Font("Segoe UI", 9.0F, FontStyle.Bold)
            Dim fontGroupTitle As New Font("Segoe UI", 9.5F, FontStyle.Bold)

            ' ══════════════════════════════════════════════
            ' กลุ่ม: ข้อมูลเครื่อง
            ' ══════════════════════════════════════════════
            Me._grpInfo = New GroupBox()
            Me._grpInfo.Text = " ข้อมูลเครื่อง "
            Me._grpInfo.Font = fontGroupTitle
            Me._grpInfo.Location = New Point(14, 14)
            Me._grpInfo.Size = New Size(370, 130)

            ' ComputerName
            Me._lblComNameLabel = New Label()
            Me._lblComNameLabel.Text = "ชื่อเครื่อง:"
            Me._lblComNameLabel.Location = New Point(16, 28)
            Me._lblComNameLabel.AutoSize = True
            Me._lblComNameLabel.Font = fontNormal

            Me._lblComNameValue = New Label()
            Me._lblComNameValue.Text = "..."
            Me._lblComNameValue.Location = New Point(130, 28)
            Me._lblComNameValue.AutoSize = True
            Me._lblComNameValue.Font = fontBold

            ' Type
            Me._lblTypeLabel = New Label()
            Me._lblTypeLabel.Text = "ประเภท:"
            Me._lblTypeLabel.Location = New Point(16, 52)
            Me._lblTypeLabel.AutoSize = True
            Me._lblTypeLabel.Font = fontNormal

            Me._lblTypeValue = New Label()
            Me._lblTypeValue.Text = "..."
            Me._lblTypeValue.Location = New Point(130, 52)
            Me._lblTypeValue.AutoSize = True
            Me._lblTypeValue.Font = fontBold

            ' Mode
            Me._lblModeLabel = New Label()
            Me._lblModeLabel.Text = "โหมด:"
            Me._lblModeLabel.Location = New Point(16, 76)
            Me._lblModeLabel.AutoSize = True
            Me._lblModeLabel.Font = fontNormal

            Me._lblModeValue = New Label()
            Me._lblModeValue.Text = "..."
            Me._lblModeValue.Location = New Point(130, 76)
            Me._lblModeValue.AutoSize = True
            Me._lblModeValue.Font = fontBold

            ' Time
            Me._lblTimeLabel = New Label()
            Me._lblTimeLabel.Text = "เวลาตรวจสอบ:"
            Me._lblTimeLabel.Location = New Point(16, 100)
            Me._lblTimeLabel.AutoSize = True
            Me._lblTimeLabel.Font = fontNormal

            Me._lblTimeValue = New Label()
            Me._lblTimeValue.Text = "..."
            Me._lblTimeValue.Location = New Point(130, 100)
            Me._lblTimeValue.AutoSize = True
            Me._lblTimeValue.Font = fontBold

            Me._grpInfo.Controls.AddRange(New Control() { _
                Me._lblComNameLabel, Me._lblComNameValue, _
                Me._lblTypeLabel, Me._lblTypeValue, _
                Me._lblModeLabel, Me._lblModeValue, _
                Me._lblTimeLabel, Me._lblTimeValue})

            ' ══════════════════════════════════════════════
            ' กลุ่ม: เวอร์ชัน
            ' ══════════════════════════════════════════════
            Me._grpVersion = New GroupBox()
            Me._grpVersion.Text = " เวอร์ชัน "
            Me._grpVersion.Font = fontGroupTitle
            Me._grpVersion.Location = New Point(14, 152)
            Me._grpVersion.Size = New Size(370, 104)

            ' Current Version
            Me._lblCurrentLabel = New Label()
            Me._lblCurrentLabel.Text = "เวอร์ชันปัจจุบัน:"
            Me._lblCurrentLabel.Location = New Point(16, 28)
            Me._lblCurrentLabel.AutoSize = True
            Me._lblCurrentLabel.Font = fontNormal

            Me._lblCurrentValue = New Label()
            Me._lblCurrentValue.Text = "..."
            Me._lblCurrentValue.Location = New Point(150, 28)
            Me._lblCurrentValue.AutoSize = True
            Me._lblCurrentValue.Font = fontBold

            ' Server Version
            Me._lblServerLabel = New Label()
            Me._lblServerLabel.Text = "เวอร์ชัน Server:"
            Me._lblServerLabel.Location = New Point(16, 52)
            Me._lblServerLabel.AutoSize = True
            Me._lblServerLabel.Font = fontNormal

            Me._lblServerValue = New Label()
            Me._lblServerValue.Text = "..."
            Me._lblServerValue.Location = New Point(150, 52)
            Me._lblServerValue.AutoSize = True
            Me._lblServerValue.Font = fontBold

            ' Status
            Me._lblStatusLabel = New Label()
            Me._lblStatusLabel.Text = "สถานะ:"
            Me._lblStatusLabel.Location = New Point(16, 76)
            Me._lblStatusLabel.AutoSize = True
            Me._lblStatusLabel.Font = fontNormal

            Me._lblStatusValue = New Label()
            Me._lblStatusValue.Text = "..."
            Me._lblStatusValue.Location = New Point(150, 76)
            Me._lblStatusValue.AutoSize = True
            Me._lblStatusValue.Font = fontBold

            Me._grpVersion.Controls.AddRange(New Control() { _
                Me._lblCurrentLabel, Me._lblCurrentValue, _
                Me._lblServerLabel, Me._lblServerValue, _
                Me._lblStatusLabel, Me._lblStatusValue})

            ' ══════════════════════════════════════════════
            ' ปุ่ม
            ' ══════════════════════════════════════════════
            Me._btnCheckNow = New Button()
            Me._btnCheckNow.Text = "ตรวจสอบอัปเดต"
            Me._btnCheckNow.Location = New Point(14, 268)
            Me._btnCheckNow.Size = New Size(120, 32)
            Me._btnCheckNow.Font = fontNormal

            Me._btnRefreshInfo = New Button()
            Me._btnRefreshInfo.Text = "รีเฟรชข้อมูล"
            Me._btnRefreshInfo.Location = New Point(144, 268)
            Me._btnRefreshInfo.Size = New Size(110, 32)
            Me._btnRefreshInfo.Font = fontNormal

            Me._btnExit = New Button()
            Me._btnExit.Text = "ออก"
            Me._btnExit.Location = New Point(314, 268)
            Me._btnExit.Size = New Size(70, 32)
            Me._btnExit.Font = fontNormal

            ' ══════════════════════════════════════════════
            ' Form
            ' ══════════════════════════════════════════════
            Me.Text = "Auto Update"
            Me.ClientSize = New Size(400, 316)
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.Font = New Font("Segoe UI", 9.0F)
            Me.ShowInTaskbar = False
            Me.WindowState = FormWindowState.Minimized
            Me.Opacity = 0.0R

            Me.Controls.Add(Me._grpInfo)
            Me.Controls.Add(Me._grpVersion)
            Me.Controls.Add(Me._btnCheckNow)
            Me.Controls.Add(Me._btnRefreshInfo)
            Me.Controls.Add(Me._btnExit)

            Me._contextMenu.ResumeLayout(False)
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
                ElseIf String.Equals(currentVer, serverVer, StringComparison.OrdinalIgnoreCase) Then
                    _lblStatusValue.Text = "✓ เวอร์ชันล่าสุดแล้ว"
                    _lblStatusValue.ForeColor = Color.Green
                Else
                    _lblStatusValue.Text = "✗ มีอัปเดตใหม่"
                    _lblStatusValue.ForeColor = Color.Red
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

        ' ── ตรวจสอบอัปเดตเสร็จแล้ว → รีเฟรช UI ──
        Private Sub OnUpdateCompleted(ByVal sender As Object, ByVal e As Workers.UpdateCompletedEventArgs)
            LoadInfo()
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
                _updateWorker.RunAsync()
            End If
        End Sub

        ' ── ปุ่ม: รีเฟรชข้อมูล ──
        Private Sub BtnRefreshInfo_Click(ByVal sender As Object, ByVal e As EventArgs)
            Managers.ConfigManager.InvalidateCache()
            LoadInfo()
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
