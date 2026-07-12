Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Windows.Forms

Namespace Workers

    ''' <summary>
    ''' Event args for the UpdateCompleted event.
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
    ''' BackgroundWorker wrapper that orchestrates the full update check flow:
    ''' 1. Get ComputerName
    ''' 2. Find in TesterType.csv
    ''' 3. Check scheduled time
    ''' 4. Read versions (registry + file)
    ''' 5. Check updateflag.txt for pending restart
    ''' 6. Execute mode-specific strategy
    ''' 
    ''' All heavy work runs on a background thread.
    ''' NormalStrategy uses Control.Invoke for UI prompts.
    ''' </summary>
    Public Class UpdateWorker
        Implements IDisposable

        Private ReadOnly _worker As BackgroundWorker
        Private ReadOnly _invokeControl As Control
        Private _disposed As Boolean

        ''' <summary>Raised when the update check completes (on UI thread).</summary>
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

        ''' <summary>True if an update check is currently in progress.</summary>
        Public ReadOnly Property IsBusy As Boolean
            Get
                Return _worker.IsBusy
            End Get
        End Property

        ''' <summary>
        ''' Starts an asynchronous update check. No-op if already running.
        ''' </summary>
        Public Sub RunAsync()
            If _worker.IsBusy Then
                Managers.LogManager.Warn("Update worker is already running. Skipping.")
                Return
            End If
            _worker.RunWorkerAsync()
        End Sub

        ''' <summary>Requests cancellation of the current update check.</summary>
        Public Sub Cancel()
            If _worker.IsBusy Then
                _worker.CancelAsync()
            End If
        End Sub

        Private Sub DoWork(sender As Object, e As DoWorkEventArgs)
            Try
                Managers.LogManager.Info("═══ Update check started ═══")

                ' Check for cancellation
                If _worker.CancellationPending Then
                    e.Cancel = True
                    Return
                End If

                ' ── Step 1: Get computer name ──
                Dim computerName As String = Utilities.EnvironmentHelper.ComputerName
                Managers.LogManager.Info("Computer: " & computerName)

                ' ── Step 2: Find in TesterType.csv ──
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester Is Nothing Then
                    Managers.LogManager.Warn("Computer '" & computerName & "' not found in tester config. Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Not in config")
                    Return
                End If

                Managers.LogManager.Info("Type: " & tester.TesterType & ", Mode: " & tester.Mode & _
                                        ", ScheduledTime: " & tester.ScheduledTime.ToString())

                ' ── Step 3: Check scheduled time ──
                Dim now As TimeSpan = DateTime.Now.TimeOfDay
                If now < tester.ScheduledTime Then
                    Managers.LogManager.Info("Scheduled time not reached (" & _
                        now.ToString("hh\:mm\:ss") & " < " & _
                        tester.ScheduledTime.ToString("hh\:mm\:ss") & "). Skipping.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Time not reached")
                    Return
                End If

                ' ── Step 4 & 5: Read versions ──
                Dim currentVersion As String = Managers.VersionManager.ReadRegistryVersion()
                Dim latestVersion As String = Managers.VersionManager.ReadLatestVersion()
                Managers.LogManager.Info("Versions — Current: " & currentVersion & ", Latest: " & latestVersion)

                ' ── Build context ──
                Dim context As New Models.UpdateContext()
                context.Tester = tester
                context.CurrentVersion = currentVersion
                context.LatestVersion = latestVersion

                ' ── Step 6: Check update flag first (per spec) ──
                Dim flag As Boolean? = Managers.UpdateFlagManager.GetFlag(computerName)
                context.HasPendingRestartFlag = (flag.HasValue AndAlso flag.Value)

                ' Handle pending restart flag (takes priority)
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

                ' ── Step 7: Check if update needed ──
                If Not context.NeedsUpdate Then
                    Managers.LogManager.Info("Application is up to date.")
                    e.Result = New UpdateCompletedEventArgs(Strategies.UpdateResult.NoAction, "Up to date")
                    Return
                End If

                ' ── Step 8: Select and execute strategy ──
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
