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

            ' ══════════════════════════════════════════════
            ' กลุ่ม: ข้อมูลเครื่อง
            ' ══════════════════════════════════════════════
            _grpInfo = New GroupBox()
            _grpInfo.Text = " ข้อมูลเครื่อง "
            _grpInfo.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            _grpInfo.Location = New Point(14, 14)
            _grpInfo.Size = New Size(370, 130)

            Dim fontNormal As New Font("Segoe UI", 9.0F, FontStyle.Regular)
            Dim fontValue As New Font("Segoe UI", 9.0F, FontStyle.Bold)

            ' ComputerName
            _lblComNameLabel = CreateLabel("ชื่อเครื่อง:", New Point(16, 28), fontNormal)
            _lblComNameValue = CreateLabel("...", New Point(130, 28), fontValue)

            ' Type
            _lblTypeLabel = CreateLabel("ประเภท:", New Point(16, 52), fontNormal)
            _lblTypeValue = CreateLabel("...", New Point(130, 52), fontValue)

            ' Mode
            _lblModeLabel = CreateLabel("โหมด:", New Point(16, 76), fontNormal)
            _lblModeValue = CreateLabel("...", New Point(130, 76), fontValue)

            ' Time
            _lblTimeLabel = CreateLabel("เวลาตรวจสอบ:", New Point(16, 100), fontNormal)
            _lblTimeValue = CreateLabel("...", New Point(130, 100), fontValue)

            _grpInfo.Controls.AddRange(New Control() { _
                _lblComNameLabel, _lblComNameValue, _
                _lblTypeLabel, _lblTypeValue, _
                _lblModeLabel, _lblModeValue, _
                _lblTimeLabel, _lblTimeValue})

            ' ══════════════════════════════════════════════
            ' กลุ่ม: เวอร์ชัน
            ' ══════════════════════════════════════════════
            _grpVersion = New GroupBox()
            _grpVersion.Text = " เวอร์ชัน "
            _grpVersion.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            _grpVersion.Location = New Point(14, 152)
            _grpVersion.Size = New Size(370, 104)

            ' Current Version
            _lblCurrentLabel = CreateLabel("เวอร์ชันปัจจุบัน:", New Point(16, 28), fontNormal)
            _lblCurrentValue = CreateLabel("...", New Point(150, 28), fontValue)

            ' Server Version
            _lblServerLabel = CreateLabel("เวอร์ชัน Server:", New Point(16, 52), fontNormal)
            _lblServerValue = CreateLabel("...", New Point(150, 52), fontValue)

            ' Status
            _lblStatusLabel = CreateLabel("สถานะ:", New Point(16, 76), fontNormal)
            _lblStatusValue = CreateLabel("...", New Point(150, 76), fontValue)

            _grpVersion.Controls.AddRange(New Control() { _
                _lblCurrentLabel, _lblCurrentValue, _
                _lblServerLabel, _lblServerValue, _
                _lblStatusLabel, _lblStatusValue})

            ' ══════════════════════════════════════════════
            ' ปุ่ม
            ' ══════════════════════════════════════════════
            _btnCheckNow = New Button()
            _btnCheckNow.Text = "ตรวจสอบอัปเดต"
            _btnCheckNow.Location = New Point(14, 268)
            _btnCheckNow.Size = New Size(120, 32)
            _btnCheckNow.Font = fontNormal
            AddHandler _btnCheckNow.Click, AddressOf BtnCheckNow_Click

            _btnRefreshInfo = New Button()
            _btnRefreshInfo.Text = "รีเฟรชข้อมูล"
            _btnRefreshInfo.Location = New Point(144, 268)
            _btnRefreshInfo.Size = New Size(110, 32)
            _btnRefreshInfo.Font = fontNormal
            AddHandler _btnRefreshInfo.Click, AddressOf BtnRefreshInfo_Click

            _btnExit = New Button()
            _btnExit.Text = "ออก"
            _btnExit.Location = New Point(314, 268)
            _btnExit.Size = New Size(70, 32)
            _btnExit.Font = fontNormal
            AddHandler _btnExit.Click, AddressOf BtnExit_Click

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

            Me.Controls.Add(_grpInfo)
            Me.Controls.Add(_grpVersion)
            Me.Controls.Add(_btnCheckNow)
            Me.Controls.Add(_btnRefreshInfo)
            Me.Controls.Add(_btnExit)

            Me._contextMenu.ResumeLayout(False)
            Me.ResumeLayout(False)
        End Sub

        Private Shared Function CreateLabel(text As String, loc As Point, fnt As Font) As Label
            Dim lbl As New Label()
            lbl.Text = text
            lbl.Location = loc
            lbl.AutoSize = True
            lbl.Font = fnt
            Return lbl
        End Function

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
