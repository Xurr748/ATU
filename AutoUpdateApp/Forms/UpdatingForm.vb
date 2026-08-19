Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel

Namespace Forms
    Public Class UpdatingForm
        Inherits Form

        Private _lblStatus As Label
        Private _progressBar As ProgressBar
        Private _worker As BackgroundWorker
        
        Public Property TesterType As String
        Public Property UpdateSuccess As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me._lblStatus = New Label()
            Me._progressBar = New ProgressBar()
            Me._worker = New BackgroundWorker()
            Me.SuspendLayout()
            
            Me._lblStatus.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
            Me._lblStatus.Location = New Point(20, 20)
            Me._lblStatus.Name = "lblStatus"
            Me._lblStatus.Size = New Size(460, 30)
            Me._lblStatus.TabIndex = 0
            Me._lblStatus.Text = "ระบบกำลังทำการอัปเดต กรุณารอสักครู่..."
            Me._lblStatus.TextAlign = ContentAlignment.MiddleCenter
            
            Me._progressBar.Location = New Point(20, 60)
            Me._progressBar.Name = "progressBar"
            Me._progressBar.Size = New Size(460, 30)
            Me._progressBar.Style = ProgressBarStyle.Continuous
            Me._progressBar.TabIndex = 1
            
            Me._worker.WorkerReportsProgress = True
            AddHandler Me._worker.DoWork, AddressOf Worker_DoWork
            AddHandler Me._worker.ProgressChanged, AddressOf Worker_ProgressChanged
            AddHandler Me._worker.RunWorkerCompleted, AddressOf Worker_Completed
            
            Me.AutoScaleDimensions = New SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(500, 120)
            Me.Controls.Add(Me._progressBar)
            Me.Controls.Add(Me._lblStatus)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "UpdatingForm"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.Text = "Auto Update"
            Me.TopMost = True
            
            AddHandler Me.Load, AddressOf UpdatingForm_Load
            Me.ResumeLayout(False)
        End Sub

        Private Sub UpdatingForm_Load(sender As Object, e As EventArgs)
            _worker.RunWorkerAsync()
        End Sub

        Private Sub Worker_DoWork(sender As Object, e As DoWorkEventArgs)
            Managers.InstallerManager.KillTargetProcess()
            e.Result = Managers.InstallerManager.RunInstaller(Me.TesterType, Sub(percent, msg)
                _worker.ReportProgress(percent, msg)
            End Sub)
        End Sub

        Private Sub Worker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs)
            Me._progressBar.Value = Math.Min(Math.Max(e.ProgressPercentage, 0), 100)
            If e.UserState IsNot Nothing Then
                Me._lblStatus.Text = e.UserState.ToString()
            End If
        End Sub

        Private Sub Worker_Completed(sender As Object, e As RunWorkerCompletedEventArgs)
            If e.Error Is Nothing AndAlso e.Result IsNot Nothing Then
                Me.UpdateSuccess = CBool(e.Result)
            Else
                Me.UpdateSuccess = False
            End If
            Me.Close()
        End Sub
    End Class
End Namespace
