Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Windows.Forms

Namespace Workers

    ''' <summary>
    ''' Event Args สำหรับ Event UpdateCompleted
    ''' </summary>
    Public Class UpdateCompletedEventArgs
        Inherits EventArgs

        Public Property Result As Strategies.UpdateResult
        Public Property Message As String

        Public Sub New(result As Strategies.UpdateResult, message As String)
            Me.Result = result
            Me.Message = message
        End Sub
    End Class

    ''' <summary>
    ''' ตัวจัดการ BackgroundWorker ที่ดำเนินการตรวจสอบอัปเดตทั้งขั้นตอน:
    ''' 1. Get ComputerName
    ''' 2. Find in TesterType.csv
    ''' 3. Check scheduled time
    ''' 4. Read versions (registry + file)
    ''' 5. Check updateflag.txt for pending restart
    ''' 6. Execute mode-specific strategy
    ''' 
    ''' งานหนักทั้งหมดทำงานบน Background Thread
    ''' NormalStrategy ใช้ Control.Invoke สำหรับแสดงหน้าต่าง
    ''' </summary>
    Public Class UpdateWorker
        Implements IDisposable

        Private ReadOnly _worker As BackgroundWorker
        Private ReadOnly _invokeControl As Control
        Private _disposed As Boolean
        Private _lastRunDate As DateTime = DateTime.MinValue

        ''' <summary>เกิดขึ้นเมื่อการตรวจสอบอัปเดตเสร็จ (บน UI Thread)</summary>
        Public Event UpdateCompleted As EventHandler(Of UpdateCompletedEventArgs)

        ''' <summary>
        ''' Creates an UpdateWorker.
        ''' </summary>
        ''' <param name="invokeControl">Control for UI thread marshaling (passed to NormalStrategy).</param>
        Public Sub New(invokeControl As Control)
            _invokeControl = invokeControl
            _worker = New BackgroundWorker()
            _worker.WorkerSupportsCancellation = True
            AddHandler _worker.DoWork, AddressOf DoWork
            AddHandler _worker.RunWorkerCompleted, AddressOf WorkCompleted
        End Sub

        ''' <summary>True หากกำลังตรวจสอบอัปเดตอยู่</summary>
        Public ReadOnly Property IsBusy As Boolean
            Get
                Return _worker.IsBusy
            End Get
        End Property

        ''' <summary>
        ''' เริ่มตรวจสอบอัปเดตแบบ Async ไม่ทำอะไรหากกำลังทำงานอยู่
        ''' </summary>
        Public Sub RunAsync()
            If _worker.IsBusy Then
                Managers.LogManager.Warn("Update worker is already running. Skipping.")
                Return
            End If
            _worker.RunWorkerAsync()
        End Sub

        ''' <summary>ร้องขอยกเลิกการตรวจสอบอัปเดต</summary>
        Public Sub Cancel()
            If _worker.IsBusy Then
                _worker.CancelAsync()
            End If
        End Sub

        Private Sub DoWork(sender As Object, e As DoWorkEventArgs)
            Try
                Managers.LogManager.Info("═══ Update check started ═══")

                ' ตรวจสอบว่าถูกยกเลิกหรือไม่
                If _worker.CancellationPending Then
                    e.Cancel = True
                    Return
                End If

                ' ── ขั้นตอนที่ 1: ดึงชื่อเครื่อง ──
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Managers.LogManager.Info("Computer: " & computerName)

                ' ── ขั้นตอนที่ 2: ค้นหาใน TesterType.csv ──
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester Is Nothing Then
                    Managers.LogManager.Warn("Computer '" & computerName & "' not found in tester config. Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Not in config")
                    Return
                End If

                Managers.LogManager.Info("Type: " & tester.TesterType & ", Mode: " & tester.Mode & _
                                        ", ScheduledTime: " & tester.ScheduledTime.ToString())

                ' ── ขั้นตอนที่ 3: ตรวจสอบเวลาที่กำหนด ──
                Dim now As TimeSpan = DateTime.Now.TimeOfDay
                If now < tester.ScheduledTime Then
                    Managers.LogManager.Info("Scheduled time not reached (" & _
                        now.ToString("hh\:mm\:ss") & " < " & _
                        tester.ScheduledTime.ToString("hh\:mm\:ss") & "). Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Time not reached")
                    Return
                End If

                ' ── ป้องกันรันซ้ำในวันเดียวกัน ──
                If _lastRunDate.Date = DateTime.Now.Date Then
                    Managers.LogManager.Info("Already checked today. Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Already checked today")
                    Return
                End If
                _lastRunDate = DateTime.Now

                ' ── ขั้นตอนที่ 4 และ 5: อ่านเวอร์ชัน ──
                Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
                Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()
                Managers.LogManager.Info("Versions — Current: " & currentVersion & ", Latest: " & latestVersion)

                ' ── สร้าง Context ──
                Dim context As New Models.UpdateContext()
                context.Tester = tester
                context.CurrentVersion = currentVersion
                context.LatestVersion = latestVersion

                ' ── ขั้นตอนที่ 6: ตรวจสอบ updateflag.txt ก่อน (ตาม Spec) ──
                Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
                context.HasPendingRestartFlag = (flag.HasValue AndAlso flag.Value)

                ' จัดการ Flag รีสตาร์ทค้าง (มีความสำคัญสูงสุด)
                If context.HasPendingRestartFlag AndAlso context.NeedsUpdate Then
                    Managers.LogManager.Info("Pending restart update detected. Running installer.")
                    Dim success As Boolean = Managers.InstallerManager.RunInstaller(tester.TesterType)
                    If success Then
                        Managers.UpdateFlagManager.SetFlag(computerName, False)
                        e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.UpdateCompleted, _
                                                                "Restart update completed")
                    Else
                        e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.[Error], _
                                                                "Restart update failed")
                    End If
                    Return
                End If

                ' ── ขั้นตอนที่ 7: ตรวจสอบว่าต้องอัปเดตหรือไม่ ──
                If Not context.NeedsUpdate Then
                    Managers.LogManager.Info("Application is up to date.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Up to date")
                    Return
                End If

                ' ── ขั้นตอนที่ 8: เลือกและดำเนินการ Strategy ──
                Dim strategy As Strategies.IUpdateStrategy = _
                    Strategies.StrategyFactory.Create(tester.Mode, _invokeControl)
                Dim result As Strategies.UpdateResult = strategy.Execute(context)
                e.Result = New UpdateCompletedEventArgs(result, "Strategy executed: " & tester.Mode)

            Catch ex As Exception
                Managers.LogManager.[Error]("Update check failed.", ex)
                e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.[Error], ex.Message)
            End Try
        End Sub

        Private Sub WorkCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
            If e.Error IsNot Nothing Then
                Managers.LogManager.[Error]("Update worker error.", e.Error)
                RaiseEvent UpdateCompleted(Me, _
                    New UpdateCompletedEventArgs(Strategies.UpdateResult.[Error], e.Error.Message))
            ElseIf e.Cancelled Then
                Managers.LogManager.Info("Update check was cancelled.")
                RaiseEvent UpdateCompleted(Me, _
                    New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Cancelled"))
            ElseIf TypeOf e.Result Is UpdateCompletedEventArgs Then
                Dim args As UpdateCompletedEventArgs = DirectCast(e.Result, UpdateCompletedEventArgs)
                Managers.LogManager.Info("═══ Update check completed: " & args.Message & " ═══")
                RaiseEvent UpdateCompleted(Me, args)
            End If
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    If _worker IsNot Nothing Then
                        RemoveHandler _worker.DoWork, AddressOf DoWork
                        RemoveHandler _worker.RunWorkerCompleted, AddressOf WorkCompleted
                        _worker.Dispose()
                    End If
                End If
                _disposed = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
