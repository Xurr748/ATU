Namespace Forms
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UpdatePromptForm
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.lblVersionInfo = New System.Windows.Forms.Label()
        Me.btnUpdateNow = New System.Windows.Forms.Button()
        Me.btnAfterRestart = New System.Windows.Forms.Button()
        Me.btnRemindLater = New System.Windows.Forms.Button()
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.pnlHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.BackColor = System.Drawing.Color.FromArgb(230, 230, 240)
        Me.pnlHeader.Controls.Add(Me.lblMessage)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(450, 45)
        Me.pnlHeader.TabIndex = 5
        '
        'lblMessage
        '
        Me.lblMessage.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblMessage.Location = New System.Drawing.Point(20, 10)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New System.Drawing.Size(410, 25)
        Me.lblMessage.TabIndex = 0
        Me.lblMessage.Text = "[Header]"
        '
        'lblVersionInfo
        '
        Me.lblVersionInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblVersionInfo.Location = New System.Drawing.Point(20, 65)
        Me.lblVersionInfo.Name = "lblVersionInfo"
        Me.lblVersionInfo.Size = New System.Drawing.Size(410, 20)
        Me.lblVersionInfo.TabIndex = 1
        Me.lblVersionInfo.Text = "Version Info"
        '
        'btnUpdateNow
        '
        Me.btnUpdateNow.BackColor = System.Drawing.Color.FromArgb(70, 130, 180)
        Me.btnUpdateNow.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnUpdateNow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(70, 130, 180)
        Me.btnUpdateNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUpdateNow.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnUpdateNow.ForeColor = System.Drawing.Color.White
        Me.btnUpdateNow.Location = New System.Drawing.Point(20, 110)
        Me.btnUpdateNow.Name = "btnUpdateNow"
        Me.btnUpdateNow.Size = New System.Drawing.Size(120, 35)
        Me.btnUpdateNow.TabIndex = 2
        Me.btnUpdateNow.Text = "Update Now"
        Me.btnUpdateNow.UseVisualStyleBackColor = False
        Me.btnUpdateNow.Visible = False
        '
        'btnAfterRestart
        '
        Me.btnAfterRestart.BackColor = System.Drawing.Color.White
        Me.btnAfterRestart.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnAfterRestart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200)
        Me.btnAfterRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAfterRestart.Location = New System.Drawing.Point(165, 110)
        Me.btnAfterRestart.Name = "btnAfterRestart"
        Me.btnAfterRestart.Size = New System.Drawing.Size(120, 35)
        Me.btnAfterRestart.TabIndex = 3
        Me.btnAfterRestart.Text = "After Restart"
        Me.btnAfterRestart.UseVisualStyleBackColor = False
        '
        'btnRemindLater
        '
        Me.btnRemindLater.BackColor = System.Drawing.Color.White
        Me.btnRemindLater.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnRemindLater.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200)
        Me.btnRemindLater.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRemindLater.Location = New System.Drawing.Point(310, 110)
        Me.btnRemindLater.Name = "btnRemindLater"
        Me.btnRemindLater.Size = New System.Drawing.Size(120, 35)
        Me.btnRemindLater.TabIndex = 4
        Me.btnRemindLater.Text = "Remind Later"
        Me.btnRemindLater.UseVisualStyleBackColor = False
        '
        'UpdatePromptForm
        '
        Me.BackColor = System.Drawing.Color.FromArgb(245, 245, 250)
        Me.ClientSize = New System.Drawing.Size(450, 180)
        Me.Controls.Add(Me.pnlHeader)
        Me.Controls.Add(Me.lblVersionInfo)
        Me.Controls.Add(Me.btnUpdateNow)
        Me.Controls.Add(Me.btnAfterRestart)
        Me.Controls.Add(Me.btnRemindLater)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "UpdatePromptForm"
        Me.ShowInTaskbar = True
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Update Prompt"
        Me.TopMost = True
        Me.pnlHeader.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents lblVersionInfo As System.Windows.Forms.Label
    Friend WithEvents btnUpdateNow As System.Windows.Forms.Button
    Friend WithEvents btnAfterRestart As System.Windows.Forms.Button
    Friend WithEvents btnRemindLater As System.Windows.Forms.Button
    Friend WithEvents pnlHeader As System.Windows.Forms.Panel

End Class
End Namespace
