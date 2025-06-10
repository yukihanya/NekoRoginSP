<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AuthForm
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AuthForm))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.Panel_IdLock = New System.Windows.Forms.Panel()
        Me.Label_Tel = New System.Windows.Forms.Label()
        Me.Label_IdLock = New System.Windows.Forms.Label()
        Me.Panel_Account = New System.Windows.Forms.Panel()
        Me.Text_Passwd = New System.Windows.Forms.TextBox()
        Me.Text_Id = New System.Windows.Forms.TextBox()
        Me.Label_Account = New System.Windows.Forms.Label()
        Me.Panel_Image = New System.Windows.Forms.Panel()
        Me.Text_Image = New System.Windows.Forms.TextBox()
        Me.Image_Auth = New System.Windows.Forms.PictureBox()
        Me.Label_Image = New System.Windows.Forms.Label()
        Me.Panel_OTP = New System.Windows.Forms.Panel()
        Me.Text_OTP = New System.Windows.Forms.TextBox()
        Me.Label_OTP = New System.Windows.Forms.Label()
        Me.Label_Error = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel_IdLock.SuspendLayout()
        Me.Panel_Account.SuspendLayout()
        Me.Panel_Image.SuspendLayout()
        CType(Me.Image_Auth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel_OTP.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(202, 165)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 27)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'OK_Button
        '
        Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OK_Button.Enabled = False
        Me.OK_Button.Location = New System.Drawing.Point(3, 3)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(67, 21)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(67, 21)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "キャンセル"
        '
        'Panel_IdLock
        '
        Me.Panel_IdLock.Controls.Add(Me.Label_Tel)
        Me.Panel_IdLock.Controls.Add(Me.Label_IdLock)
        Me.Panel_IdLock.Location = New System.Drawing.Point(12, 618)
        Me.Panel_IdLock.Name = "Panel_IdLock"
        Me.Panel_IdLock.Size = New System.Drawing.Size(315, 116)
        Me.Panel_IdLock.TabIndex = 1
        Me.Panel_IdLock.Visible = False
        '
        'Label_Tel
        '
        Me.Label_Tel.AutoSize = True
        Me.Label_Tel.Font = New System.Drawing.Font("ＭＳ ゴシック", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label_Tel.Location = New System.Drawing.Point(39, 68)
        Me.Label_Tel.Name = "Label_Tel"
        Me.Label_Tel.Size = New System.Drawing.Size(124, 16)
        Me.Label_Tel.TabIndex = 2
        Me.Label_Tel.Text = "0800-***-****"
        '
        'Label_IdLock
        '
        Me.Label_IdLock.AutoSize = True
        Me.Label_IdLock.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label_IdLock.Location = New System.Drawing.Point(15, 11)
        Me.Label_IdLock.Name = "Label_IdLock"
        Me.Label_IdLock.Size = New System.Drawing.Size(209, 24)
        Me.Label_IdLock.TabIndex = 0
        Me.Label_IdLock.Text = "■IDロック電話番号認証 (Step %step%/4)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "%message%" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'Panel_Account
        '
        Me.Panel_Account.Controls.Add(Me.Text_Passwd)
        Me.Panel_Account.Controls.Add(Me.Text_Id)
        Me.Panel_Account.Controls.Add(Me.Label_Account)
        Me.Panel_Account.Location = New System.Drawing.Point(12, 486)
        Me.Panel_Account.Name = "Panel_Account"
        Me.Panel_Account.Size = New System.Drawing.Size(315, 116)
        Me.Panel_Account.TabIndex = 2
        Me.Panel_Account.Visible = False
        '
        'Text_Passwd
        '
        Me.Text_Passwd.ForeColor = System.Drawing.SystemColors.GrayText
        Me.Text_Passwd.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.Text_Passwd.Location = New System.Drawing.Point(15, 59)
        Me.Text_Passwd.Name = "Text_Passwd"
        Me.Text_Passwd.Size = New System.Drawing.Size(209, 19)
        Me.Text_Passwd.TabIndex = 2
        Me.Text_Passwd.Tag = "パスワード"
        Me.Text_Passwd.Text = "パスワード"
        '
        'Text_Id
        '
        Me.Text_Id.ForeColor = System.Drawing.SystemColors.GrayText
        Me.Text_Id.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.Text_Id.Location = New System.Drawing.Point(15, 34)
        Me.Text_Id.Name = "Text_Id"
        Me.Text_Id.Size = New System.Drawing.Size(209, 19)
        Me.Text_Id.TabIndex = 1
        Me.Text_Id.Tag = "ガンホーID"
        Me.Text_Id.Text = "ガンホーID"
        '
        'Label_Account
        '
        Me.Label_Account.AutoSize = True
        Me.Label_Account.Location = New System.Drawing.Point(15, 10)
        Me.Label_Account.Name = "Label_Account"
        Me.Label_Account.Size = New System.Drawing.Size(100, 12)
        Me.Label_Account.TabIndex = 0
        Me.Label_Account.Text = "■アカウント%mode%"
        '
        'Panel_Image
        '
        Me.Panel_Image.Controls.Add(Me.Text_Image)
        Me.Panel_Image.Controls.Add(Me.Image_Auth)
        Me.Panel_Image.Controls.Add(Me.Label_Image)
        Me.Panel_Image.Location = New System.Drawing.Point(12, 352)
        Me.Panel_Image.Name = "Panel_Image"
        Me.Panel_Image.Size = New System.Drawing.Size(315, 116)
        Me.Panel_Image.TabIndex = 3
        Me.Panel_Image.Visible = False
        '
        'Text_Image
        '
        Me.Text_Image.ForeColor = System.Drawing.SystemColors.GrayText
        Me.Text_Image.ImeMode = System.Windows.Forms.ImeMode.Hiragana
        Me.Text_Image.Location = New System.Drawing.Point(17, 87)
        Me.Text_Image.MaxLength = 4
        Me.Text_Image.Name = "Text_Image"
        Me.Text_Image.Size = New System.Drawing.Size(207, 19)
        Me.Text_Image.TabIndex = 2
        Me.Text_Image.Tag = "認証文字"
        Me.Text_Image.Text = "認証文字"
        '
        'Image_Auth
        '
        Me.Image_Auth.Image = CType(resources.GetObject("Image_Auth.Image"), System.Drawing.Image)
        Me.Image_Auth.Location = New System.Drawing.Point(15, 30)
        Me.Image_Auth.Name = "Image_Auth"
        Me.Image_Auth.Size = New System.Drawing.Size(228, 51)
        Me.Image_Auth.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.Image_Auth.TabIndex = 1
        Me.Image_Auth.TabStop = False
        '
        'Label_Image
        '
        Me.Label_Image.AutoSize = True
        Me.Label_Image.Location = New System.Drawing.Point(15, 15)
        Me.Label_Image.Name = "Label_Image"
        Me.Label_Image.Size = New System.Drawing.Size(65, 12)
        Me.Label_Image.TabIndex = 0
        Me.Label_Image.Text = "■画像認証"
        '
        'Panel_OTP
        '
        Me.Panel_OTP.Controls.Add(Me.Text_OTP)
        Me.Panel_OTP.Controls.Add(Me.Label_OTP)
        Me.Panel_OTP.Location = New System.Drawing.Point(12, 216)
        Me.Panel_OTP.Name = "Panel_OTP"
        Me.Panel_OTP.Size = New System.Drawing.Size(315, 116)
        Me.Panel_OTP.TabIndex = 4
        Me.Panel_OTP.Visible = False
        '
        'Text_OTP
        '
        Me.Text_OTP.ForeColor = System.Drawing.SystemColors.GrayText
        Me.Text_OTP.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.Text_OTP.Location = New System.Drawing.Point(15, 37)
        Me.Text_OTP.MaxLength = 6
        Me.Text_OTP.Name = "Text_OTP"
        Me.Text_OTP.Size = New System.Drawing.Size(209, 19)
        Me.Text_OTP.TabIndex = 1
        Me.Text_OTP.Tag = "ワンタイムパスワード"
        Me.Text_OTP.Text = "ワンタイムパスワード"
        '
        'Label_OTP
        '
        Me.Label_OTP.AutoSize = True
        Me.Label_OTP.Location = New System.Drawing.Point(13, 11)
        Me.Label_OTP.Name = "Label_OTP"
        Me.Label_OTP.Size = New System.Drawing.Size(109, 12)
        Me.Label_OTP.TabIndex = 0
        Me.Label_OTP.Text = "■ワンタイムパスワード"
        '
        'Label_Error
        '
        Me.Label_Error.AutoSize = True
        Me.Label_Error.ForeColor = System.Drawing.Color.Red
        Me.Label_Error.Location = New System.Drawing.Point(25, 131)
        Me.Label_Error.Name = "Label_Error"
        Me.Label_Error.Size = New System.Drawing.Size(41, 12)
        Me.Label_Error.TabIndex = 5
        Me.Label_Error.Text = "%error%"
        Me.Label_Error.Visible = False
        '
        'AuthForm
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(360, 203)
        Me.Controls.Add(Me.Label_Error)
        Me.Controls.Add(Me.Panel_Account)
        Me.Controls.Add(Me.Panel_Image)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.Panel_OTP)
        Me.Controls.Add(Me.Panel_IdLock)
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.ImeMode = System.Windows.Forms.ImeMode.[On]
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AuthForm"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Caption"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel_IdLock.ResumeLayout(False)
        Me.Panel_IdLock.PerformLayout()
        Me.Panel_Account.ResumeLayout(False)
        Me.Panel_Account.PerformLayout()
        Me.Panel_Image.ResumeLayout(False)
        Me.Panel_Image.PerformLayout()
        CType(Me.Image_Auth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel_OTP.ResumeLayout(False)
        Me.Panel_OTP.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents Panel_IdLock As Panel
    Friend WithEvents Label_IdLock As Label
    Friend WithEvents Label_Tel As Label
    Friend WithEvents Panel_Account As Panel
    Friend WithEvents Label_Account As Label
    Friend WithEvents Text_Passwd As TextBox
    Friend WithEvents Text_Id As TextBox
    Friend WithEvents Panel_Image As Panel
    Friend WithEvents Label_Image As Label
    Friend WithEvents Image_Auth As PictureBox
    Friend WithEvents Text_Image As TextBox
    Friend WithEvents Panel_OTP As Panel
    Friend WithEvents Label_OTP As Label
    Friend WithEvents Text_OTP As TextBox
    Friend WithEvents Label_Error As Label
End Class
