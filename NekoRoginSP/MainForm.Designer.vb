<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.List_Account = New System.Windows.Forms.ComboBox()
        Me.List_Game = New System.Windows.Forms.ListBox()
        Me.Button_Account = New System.Windows.Forms.Button()
        Me.Button_Play = New System.Windows.Forms.Button()
        Me.Button_Info = New System.Windows.Forms.Button()
        Me.Button_Option = New System.Windows.Forms.Button()
        Me.MenuOption = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuOption_Setup = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuOption_Shell = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuOption_ShellChatLog = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuOption_ShellScreenShot = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuOption_ShellMusic = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuOption_MinimizeToTray = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuOption_About = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuOption_Quit = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuInfo = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuInfo_Character = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuInfo_Quest = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuAccount = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuAccount_Tool = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuAccount_Add = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuAccount_Remove = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuTasktray = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.TasktrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.MenuOption.SuspendLayout()
        Me.MenuInfo.SuspendLayout()
        Me.MenuAccount.SuspendLayout()
        Me.SuspendLayout()
        '
        'List_Account
        '
        Me.List_Account.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.List_Account.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.List_Account.FormattingEnabled = True
        Me.List_Account.Location = New System.Drawing.Point(12, 15)
        Me.List_Account.Name = "List_Account"
        Me.List_Account.Size = New System.Drawing.Size(240, 20)
        Me.List_Account.TabIndex = 0
        '
        'List_Game
        '
        Me.List_Game.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.List_Game.FormattingEnabled = True
        Me.List_Game.ItemHeight = 12
        Me.List_Game.Location = New System.Drawing.Point(12, 41)
        Me.List_Game.Name = "List_Game"
        Me.List_Game.Size = New System.Drawing.Size(240, 88)
        Me.List_Game.TabIndex = 1
        '
        'Button_Account
        '
        Me.Button_Account.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button_Account.Location = New System.Drawing.Point(263, 12)
        Me.Button_Account.Name = "Button_Account"
        Me.Button_Account.Size = New System.Drawing.Size(99, 23)
        Me.Button_Account.TabIndex = 2
        Me.Button_Account.Tag = ""
        Me.Button_Account.Text = "アカウント"
        Me.Button_Account.UseVisualStyleBackColor = True
        '
        'Button_Play
        '
        Me.Button_Play.Enabled = False
        Me.Button_Play.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button_Play.Location = New System.Drawing.Point(264, 41)
        Me.Button_Play.Name = "Button_Play"
        Me.Button_Play.Size = New System.Drawing.Size(99, 23)
        Me.Button_Play.TabIndex = 3
        Me.Button_Play.Text = "ゲーム起動"
        Me.Button_Play.UseVisualStyleBackColor = True
        '
        'Button_Info
        '
        Me.Button_Info.Enabled = False
        Me.Button_Info.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button_Info.Location = New System.Drawing.Point(264, 70)
        Me.Button_Info.Name = "Button_Info"
        Me.Button_Info.Size = New System.Drawing.Size(99, 23)
        Me.Button_Info.TabIndex = 5
        Me.Button_Info.Tag = ""
        Me.Button_Info.Text = "ゲーム情報"
        Me.Button_Info.UseVisualStyleBackColor = True
        '
        'Button_Option
        '
        Me.Button_Option.Font = New System.Drawing.Font("ＭＳ ゴシック", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button_Option.Location = New System.Drawing.Point(263, 99)
        Me.Button_Option.Name = "Button_Option"
        Me.Button_Option.Size = New System.Drawing.Size(99, 23)
        Me.Button_Option.TabIndex = 6
        Me.Button_Option.Tag = ""
        Me.Button_Option.Text = "オプション"
        Me.Button_Option.UseVisualStyleBackColor = True
        '
        'MenuOption
        '
        Me.MenuOption.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuOption_Setup, Me.MenuOption_Shell, Me.MenuOption_MinimizeToTray, Me.ToolStripSeparator2, Me.MenuOption_About, Me.ToolStripSeparator3, Me.MenuOption_Quit})
        Me.MenuOption.Name = "MenuOption"
        Me.MenuOption.Size = New System.Drawing.Size(237, 136)
        Me.MenuOption.Tag = ""
        '
        'MenuOption_Setup
        '
        Me.MenuOption_Setup.Name = "MenuOption_Setup"
        Me.MenuOption_Setup.Size = New System.Drawing.Size(236, 24)
        Me.MenuOption_Setup.Text = "ゲーム設定"
        '
        'MenuOption_Shell
        '
        Me.MenuOption_Shell.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuOption_ShellChatLog, Me.MenuOption_ShellScreenShot, Me.MenuOption_ShellMusic})
        Me.MenuOption_Shell.Name = "MenuOption_Shell"
        Me.MenuOption_Shell.Size = New System.Drawing.Size(236, 24)
        Me.MenuOption_Shell.Tag = ""
        Me.MenuOption_Shell.Text = "インストールフォルダを開く"
        '
        'MenuOption_ShellChatLog
        '
        Me.MenuOption_ShellChatLog.Name = "MenuOption_ShellChatLog"
        Me.MenuOption_ShellChatLog.Size = New System.Drawing.Size(150, 22)
        Me.MenuOption_ShellChatLog.Tag = "Chat"
        Me.MenuOption_ShellChatLog.Text = "チャットログ"
        '
        'MenuOption_ShellScreenShot
        '
        Me.MenuOption_ShellScreenShot.Name = "MenuOption_ShellScreenShot"
        Me.MenuOption_ShellScreenShot.Size = New System.Drawing.Size(150, 22)
        Me.MenuOption_ShellScreenShot.Tag = "ScreenShot"
        Me.MenuOption_ShellScreenShot.Text = "スクリーンショット"
        '
        'MenuOption_ShellMusic
        '
        Me.MenuOption_ShellMusic.Name = "MenuOption_ShellMusic"
        Me.MenuOption_ShellMusic.Size = New System.Drawing.Size(150, 22)
        Me.MenuOption_ShellMusic.Tag = "BGM"
        Me.MenuOption_ShellMusic.Text = "BGM"
        '
        'MenuOption_MinimizeToTray
        '
        Me.MenuOption_MinimizeToTray.Name = "MenuOption_MinimizeToTray"
        Me.MenuOption_MinimizeToTray.Size = New System.Drawing.Size(236, 24)
        Me.MenuOption_MinimizeToTray.Text = "最小化時にタスクトレイに格納する"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(233, 6)
        '
        'MenuOption_About
        '
        Me.MenuOption_About.Font = New System.Drawing.Font("Yu Gothic UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.MenuOption_About.ForeColor = System.Drawing.Color.SaddleBrown
        Me.MenuOption_About.Name = "MenuOption_About"
        Me.MenuOption_About.Size = New System.Drawing.Size(236, 24)
        Me.MenuOption_About.Text = "🐈️"
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(233, 6)
        '
        'MenuOption_Quit
        '
        Me.MenuOption_Quit.Name = "MenuOption_Quit"
        Me.MenuOption_Quit.Size = New System.Drawing.Size(236, 24)
        Me.MenuOption_Quit.Text = "アプリケーションの終了"
        '
        'MenuInfo
        '
        Me.MenuInfo.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuInfo_Character, Me.MenuInfo_Quest})
        Me.MenuInfo.Name = "MenuInfo"
        Me.MenuInfo.Size = New System.Drawing.Size(150, 48)
        Me.MenuInfo.Tag = ""
        '
        'MenuInfo_Character
        '
        Me.MenuInfo_Character.Name = "MenuInfo_Character"
        Me.MenuInfo_Character.Size = New System.Drawing.Size(149, 22)
        Me.MenuInfo_Character.Tag = ""
        Me.MenuInfo_Character.Text = "キャラクター連動"
        '
        'MenuInfo_Quest
        '
        Me.MenuInfo_Quest.Name = "MenuInfo_Quest"
        Me.MenuInfo_Quest.Size = New System.Drawing.Size(149, 22)
        Me.MenuInfo_Quest.Tag = ""
        Me.MenuInfo_Quest.Text = "クエスト連動"
        '
        'MenuAccount
        '
        Me.MenuAccount.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuAccount_Tool, Me.ToolStripSeparator1, Me.MenuAccount_Add, Me.MenuAccount_Remove})
        Me.MenuAccount.Name = "MenuAccount"
        Me.MenuAccount.Size = New System.Drawing.Size(162, 76)
        Me.MenuAccount.Tag = ""
        '
        'MenuAccount_Tool
        '
        Me.MenuAccount_Tool.Name = "MenuAccount_Tool"
        Me.MenuAccount_Tool.Size = New System.Drawing.Size(161, 22)
        Me.MenuAccount_Tool.Text = "アトラクションツール"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(158, 6)
        '
        'MenuAccount_Add
        '
        Me.MenuAccount_Add.Name = "MenuAccount_Add"
        Me.MenuAccount_Add.Size = New System.Drawing.Size(161, 22)
        Me.MenuAccount_Add.Text = "ガンホーID追加"
        '
        'MenuAccount_Remove
        '
        Me.MenuAccount_Remove.Enabled = False
        Me.MenuAccount_Remove.Name = "MenuAccount_Remove"
        Me.MenuAccount_Remove.Size = New System.Drawing.Size(161, 22)
        Me.MenuAccount_Remove.Text = "ガンホーID削除"
        '
        'MenuTasktray
        '
        Me.MenuTasktray.Name = "MenuTasktray"
        Me.MenuTasktray.Size = New System.Drawing.Size(61, 4)
        '
        'TasktrayIcon
        '
        Me.TasktrayIcon.ContextMenuStrip = Me.MenuTasktray
        Me.TasktrayIcon.Icon = CType(resources.GetObject("TasktrayIcon.Icon"), System.Drawing.Icon)
        Me.TasktrayIcon.Text = "NekoRoginSP"
        Me.TasktrayIcon.Visible = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(375, 139)
        Me.Controls.Add(Me.Button_Option)
        Me.Controls.Add(Me.Button_Info)
        Me.Controls.Add(Me.Button_Play)
        Me.Controls.Add(Me.Button_Account)
        Me.Controls.Add(Me.List_Game)
        Me.Controls.Add(Me.List_Account)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.Text = "NekoRoginSP"
        Me.MenuOption.ResumeLayout(False)
        Me.MenuInfo.ResumeLayout(False)
        Me.MenuAccount.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents List_Account As ComboBox
    Friend WithEvents List_Game As ListBox
    Friend WithEvents Button_Account As Button
    Friend WithEvents Button_Play As Button
    Friend WithEvents Button_Info As Button
    Friend WithEvents Button_Option As Button
    Friend WithEvents MenuOption As ContextMenuStrip
    Friend WithEvents MenuOption_MinimizeToTray As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents MenuOption_About As ToolStripMenuItem
    Friend WithEvents MenuOption_Shell As ToolStripMenuItem
    Friend WithEvents MenuInfo As ContextMenuStrip
    Friend WithEvents MenuAccount As ContextMenuStrip
    Friend WithEvents MenuAccount_Tool As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents MenuAccount_Add As ToolStripMenuItem
    Friend WithEvents MenuAccount_Remove As ToolStripMenuItem
    Friend WithEvents MenuInfo_Character As ToolStripMenuItem
    Friend WithEvents MenuInfo_Quest As ToolStripMenuItem
    Friend WithEvents MenuOption_ShellChatLog As ToolStripMenuItem
    Friend WithEvents MenuOption_ShellScreenShot As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator3 As ToolStripSeparator
    Friend WithEvents MenuOption_Quit As ToolStripMenuItem
    Friend WithEvents MenuOption_Setup As ToolStripMenuItem
    Friend WithEvents MenuOption_ShellMusic As ToolStripMenuItem
    Friend WithEvents MenuTasktray As ContextMenuStrip
    Friend WithEvents TasktrayIcon As NotifyIcon
End Class
