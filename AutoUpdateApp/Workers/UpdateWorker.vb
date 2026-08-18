Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Windows.Forms

Namespace Workers

    Public Class UpdateCompletedEventArgs
        Inherits EventArgs

        Public Property Result As Strategies.UpdateResult
        Public Property Message As String

        Public Sub New(result As Strategies.UpdateResult, message As String)
            Me.Result = result
            Me.Message = message
        End Sub
    End Class

    Public Class UpdateWorker
        Implements IDisposable

        Private ReadOnly _worker As BackgroundWorker
        Private ReadOnly _invokeControl As Control
        Private _disposed As Boolean
        Private _lastRunDate As DateTime = DateTime.MinValue

        Public Event UpdateCompleted As EventHandler(Of UpdateCompletedEventArgs)

        Public Sub New(invokeControl As Control)
            _invokeControl = invokeControl
            _worker = New BackgroundWorker()
            _worker.WorkerSupportsCancellation = True
            AddHandler _worker.DoWork, AddressOf DoWork
            AddHandler _worker.RunWorkerCompleted, AddressOf WorkCompleted
        End Sub

        Public ReadOnly Property IsBusy As Boolean
            Get
                Return _worker.IsBusy
            End Get
        End Property

        Private _isManual As Boolean = False

        Public Sub RunAsync(Optional isManual As Boolean = False)
            If _worker.IsBusy Then
                Managers.LogManager.Warn("Update worker is already running. Skipping.")
                Return
            End If
            _isManual = isManual
            _worker.RunWorkerAsync()
        End Sub

        Public Sub Cancel()
            If _worker.IsBusy Then
                _worker.CancelAsync()
            End If
        End Sub

        Private Sub DoWork(sender As Object, e As DoWorkEventArgs)
            Try
                Managers.LogManager.Info("═══ Update check started ═══")

                If _worker.CancellationPending Then
                    e.Cancel = True
                    Return
                End If

                Managers.LogManager.LogIPAddress()

                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Managers.LogManager.Info("Computer: " & computerName)

                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester Is Nothing Then
                    Managers.LogManager.Warn("Computer '" & computerName & "' not found in tester config. Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Not in config")
                    Return
                End If

                Managers.LogManager.Info("Type: " & tester.TesterType & ", Mode: " & tester.Mode & _
                                        ", ScheduledTime: " & tester.ScheduledTime.ToString())

                Dim now As DateTime = DateTime.Now
                If Not _isManual Then
                    Dim scheduled As TimeSpan = tester.ScheduledTime
                    If now.Hour <> scheduled.Hours OrElse now.Minute < scheduled.Minutes Then
                        Managers.LogManager.Info(String.Format("Scheduled hour not matching current hour. Current hour: {0}, Scheduled: {1}. Skipping.", now.Hour, scheduled.Hours))
                        e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Hour not matching")
                        Return
                    End If

                    If _lastRunDate.Date = DateTime.Now.Date Then
                        Managers.LogManager.Info("Already checked today. Skipping.")
                        e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Already checked today")
                        Return
                    End If
                End If

                Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
                Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()
                Managers.LogManager.Info("Versions — Current: " & currentVersion & ", Latest: " & latestVersion)

                Dim context As New Models.UpdateContext()
                context.Tester = tester
                context.CurrentVersion = currentVersion
                context.LatestVersion = latestVersion

                Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
                context.HasPendingRestartFlag = (flag.HasValue AndAlso flag.Value)

                If context.HasPendingRestartFlag AndAlso context.NeedsUpdate Then
                    Managers.LogManager.Info("Pending restart update flag is already set. Waiting for restart.")
                    _lastRunDate = DateTime.Now
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.UpdateScheduledForRestart, _
                                                            "Pending restart update flag is already set. Waiting for restart.")
                    Return
                End If

                If Not context.NeedsUpdate Then
                    If context.HasPendingRestartFlag Then
                        Managers.LogManager.Info("App is up to date but restart flag is True. Clearing flag.")
                        Managers.UpdateFlagManager.SetFlag(computerName, False)
                    End If

                    Managers.LogManager.Info("Application is up to date.")
                    _lastRunDate = DateTime.Now
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "โปรแกรมเป็นเวอร์ชันล่าสุดแล้ว (Up to Date)")
                    Return
                End If

                Dim installerFolder As String = Managers.InstallerManager.GetInstallerPath(tester.TesterType)
                If String.IsNullOrEmpty(installerFolder) OrElse Not IO.Directory.Exists(installerFolder) Then
                    Managers.LogManager.Warn("Installer folder not found on server: " & installerFolder)
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.Error, "ไม่พบไฟล์อัปเดต")
                    Return
                End If

                Dim strategy As Strategies.IUpdateStrategy = _
                    Strategies.StrategyFactory.Create(tester.Mode, _invokeControl)
                Dim result As Strategies.UpdateResult = strategy.Execute(context)

                If result <> Strategies.UpdateResult.[Error] Then
                    _lastRunDate = DateTime.Now
                End If

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
                        If _worker.IsBusy Then _worker.CancelAsync()
                        Try
                            RemoveHandler _worker.DoWork, AddressOf DoWork
                            RemoveHandler _worker.RunWorkerCompleted, AddressOf WorkCompleted
                            _worker.Dispose()
                        Catch ex As InvalidOperationException
                        End Try
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
