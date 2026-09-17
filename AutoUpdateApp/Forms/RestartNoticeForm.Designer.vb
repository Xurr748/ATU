Namespace Forms
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RestartNoticeForm
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me._picIcon = New System.Windows.Forms.PictureBox()
        Me._lblHeader = New System.Windows.Forms.Label()
        Me._lblBody = New System.Windows.Forms.Label()
        Me._btnRestart = New System.Windows.Forms.Button()
        Me._lblWarn = New System.Windows.Forms.Label()
        Me._popupTimer = New System.Windows.Forms.Timer(Me.components)
        CType(Me._picIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        '_picIcon
        '
        Me._picIcon.BackColor = System.Drawing.Color.Transparent
        Me._picIcon.Location = New System.Drawing.Point(20, 20)
        Me._picIcon.Name = "_picIcon"
        Me._picIcon.Size = New System.Drawing.Size(560, 80)
        Me._picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me._picIcon.TabIndex = 0
        Me._picIcon.TabStop = False
        '
        '_lblHeader
        '
        Me._lblHeader.Font = New System.Drawing.Font("Segoe UI", 24.0!, System.Drawing.FontStyle.Bold)
        Me._lblHeader.ForeColor = System.Drawing.Color.White
        Me._lblHeader.Location = New System.Drawing.Point(20, 105)
        Me._lblHeader.Name = "_lblHeader"
        Me._lblHeader.Size = New System.Drawing.Size(560, 60)
        Me._lblHeader.TabIndex = 1
        Me._lblHeader.Text = "[Header]"
        Me._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        '_lblBody
        '
        Me._lblBody.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me._lblBody.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210)
        Me._lblBody.Location = New System.Drawing.Point(30, 175)
        Me._lblBody.Name = "_lblBody"
        Me._lblBody.Size = New System.Drawing.Size(540, 100)
        Me._lblBody.TabIndex = 2
        Me._lblBody.Text = "[Body]"
        Me._lblBody.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        '_btnRestart
        '
        Me._btnRestart.BackColor = System.Drawing.Color.FromArgb(220, 53, 69)
        Me._btnRestart.Cursor = System.Windows.Forms.Cursors.Hand
        Me._btnRestart.FlatAppearance.BorderSize = 0
        Me._btnRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me._btnRestart.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me._btnRestart.ForeColor = System.Drawing.Color.White
        Me._btnRestart.Location = New System.Drawing.Point(170, 290)
        Me._btnRestart.Name = "_btnRestart"
        Me._btnRestart.Size = New System.Drawing.Size(260, 50)
        Me._btnRestart.TabIndex = 3
        Me._btnRestart.Text = "[Restart]"
        Me._btnRestart.UseVisualStyleBackColor = False
        '
        '_lblWarn
        '
        Me._lblWarn.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Italic)
        Me._lblWarn.ForeColor = System.Drawing.Color.FromArgb(150, 150, 160)
        Me._lblWarn.Location = New System.Drawing.Point(20, 350)
        Me._lblWarn.Name = "_lblWarn"
        Me._lblWarn.Size = New System.Drawing.Size(560, 25)
        Me._lblWarn.TabIndex = 4
        Me._lblWarn.Text = "[Warning]"
        Me._lblWarn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        '_popupTimer
        '
        Me._popupTimer.Interval = 20000
        '
        'RestartNoticeForm
        '
        Me.BackColor = System.Drawing.Color.FromArgb(25, 25, 35)
        Me.ClientSize = New System.Drawing.Size(600, 450)
        Me.Controls.Add(Me._lblWarn)
        Me.Controls.Add(Me._btnRestart)
        Me.Controls.Add(Me._lblBody)
        Me.Controls.Add(Me._lblHeader)
        Me.Controls.Add(Me._picIcon)
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = True
        Me.Name = "RestartNoticeForm"
        Me.ShowInTaskbar = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Restart Notice"
        Me.TopMost = True
        CType(Me._picIcon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents _picIcon As System.Windows.Forms.PictureBox
    Friend WithEvents _lblHeader As System.Windows.Forms.Label
    Friend WithEvents _lblBody As System.Windows.Forms.Label
    Friend WithEvents _btnRestart As System.Windows.Forms.Button
    Friend WithEvents _lblWarn As System.Windows.Forms.Label
    Friend WithEvents _popupTimer As System.Windows.Forms.Timer

End Class
End Namespace
