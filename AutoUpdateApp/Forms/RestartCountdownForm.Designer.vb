Namespace Forms
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RestartCountdownForm
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me._lblHeader = New System.Windows.Forms.Label()
        Me._lblCountdown = New System.Windows.Forms.Label()
        Me._btnCancel = New System.Windows.Forms.Button()
        Me._countdownTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        '_lblHeader
        '
        Me._lblHeader.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me._lblHeader.ForeColor = System.Drawing.Color.White
        Me._lblHeader.Location = New System.Drawing.Point(20, 30)
        Me._lblHeader.Name = "_lblHeader"
        Me._lblHeader.Size = New System.Drawing.Size(460, 40)
        Me._lblHeader.TabIndex = 0
        Me._lblHeader.Text = "[Header]"
        Me._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        '_lblCountdown
        '
        Me._lblCountdown.Font = New System.Drawing.Font("Segoe UI", 72.0!, System.Drawing.FontStyle.Bold)
        Me._lblCountdown.ForeColor = System.Drawing.Color.FromArgb(255, 70, 70)
        Me._lblCountdown.Location = New System.Drawing.Point(20, 75)
        Me._lblCountdown.Name = "_lblCountdown"
        Me._lblCountdown.Size = New System.Drawing.Size(460, 120)
        Me._lblCountdown.TabIndex = 1
        Me._lblCountdown.Text = "60"
        Me._lblCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        '_btnCancel
        '
        Me._btnCancel.BackColor = System.Drawing.Color.FromArgb(80, 80, 95)
        Me._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand
        Me._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(120, 120, 140)
        Me._btnCancel.FlatAppearance.BorderSize = 1
        Me._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me._btnCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me._btnCancel.ForeColor = System.Drawing.Color.White
        Me._btnCancel.Location = New System.Drawing.Point(140, 215)
        Me._btnCancel.Name = "_btnCancel"
        Me._btnCancel.Size = New System.Drawing.Size(220, 45)
        Me._btnCancel.TabIndex = 2
        Me._btnCancel.Text = "[Cancel]"
        Me._btnCancel.UseVisualStyleBackColor = False
        '
        '_countdownTimer
        '
        Me._countdownTimer.Interval = 1000
        '
        'RestartCountdownForm
        '
        Me.BackColor = System.Drawing.Color.FromArgb(20, 20, 25)
        Me.ClientSize = New System.Drawing.Size(500, 380)
        Me.Controls.Add(Me._btnCancel)
        Me.Controls.Add(Me._lblCountdown)
        Me.Controls.Add(Me._lblHeader)
        Me.ControlBox = False
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "RestartCountdownForm"
        Me.ShowInTaskbar = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Restart Countdown"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents _lblHeader As System.Windows.Forms.Label
    Friend WithEvents _lblCountdown As System.Windows.Forms.Label
    Friend WithEvents _btnCancel As System.Windows.Forms.Button
    Friend WithEvents _countdownTimer As System.Windows.Forms.Timer

End Class
End Namespace
