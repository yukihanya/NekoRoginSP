Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Security
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports NekoRoginSP

Module Program
    <STAThread()>
    Sub Main()
        Dim mutex As Mutex
        Dim createdNew As Boolean

        ' アプリ名などのユニークな名前を指定
        mutex = New Mutex(True, "NekoRoginSP_Mutex", createdNew)

        If Not createdNew Then
            ' すでに起動している場合
            MessageBox.Show("既にアプリが起動していますにゃ!!")
            Return
        End If

        ' アプリの通常起動処理
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())

        ' 終了時にmutexを解放
        mutex.ReleaseMutex()
    End Sub
End Module


Public Class MainForm

    Private Const URL_LOGIN As String = "https://member.gungho.jp/front/member/center.aspx"
    Private Const URL_GAMECODE As String = "https://member.gungho.jp/front/member/webgs/rocenter_old.aspx"
    Private Const URL_RO_LOGIN As String = "https://member.gungho.jp/front/ro/guest/login.aspx"
    Private Const URL_RO_TOOL As String = "https://ragnarokonline.gungho.jp/tool/"
    Private Const URL_RO_CHARACTER As String = "https://member.gungho.jp/front/member/sns/ro.aspx"
    Private Const URL_RO_QUEST As String = "https://member.gungho.jp/front/member/sns/ro_quest.aspx"

    Private appPath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)


    Private Structure Game
        Dim Id As String
        Dim Siid As String
        Dim Limit As String
    End Structure

    Public Shared selectedAccount As String
    Private selectedGame As String
    Private gameAccounts As New List(Of Game)
    Private httpClient As New HttpClientHelper()
    Public Shared tempOtp As String


    Private Async Function Login() As Task(Of Integer)

        Dim html As String
        Dim nextUrl As String = URL_LOGIN
        Dim parameter As New Dictionary(Of String, String)
        Dim account = loginAccounts.FirstOrDefault(Function(n) n.Id = selectedAccount)
        Dim success As Boolean

        gameAccounts.Clear()

        httpClient = New HttpClientHelper()

        httpClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.6422.142 Safari/537.36")
        httpClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        account.Cookie.Remove("goessosst")

        httpClient.SetCookies(UrlParserHelper.GetBaseDomainUrl(URL_LOGIN), account.Cookie)

        ' 一定数のページ切り替えがある時はログイン不能と見なして接続試行ループを抜けるにゃ
        For i As Integer = 0 To 9

            ' URLに含まれるGETクエリが変換されるのを防ぐにゃ
            'nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")

            Try
                ' パラメータがあればPOST、なければGETにゃ
                If parameter.Count Then
                    html = Await httpClient.PostAsync(nextUrl, parameter)
                Else
                    html = Await httpClient.GetAsync(nextUrl)
                End If

            Catch ex As HttpRequestException

                MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
                Return 0
            End Try

            ' 一度送信したパラメータはクリアするにゃ
            parameter.Clear()

            ' ページ内の文字列のマッチングを行うにゃ
            Select Case True
            ' アトラクションセンター
                Case html.Contains("でログイン中です")
                    success = True
                    Exit For

                Case html.Contains("ログインＩＤ、パスワード、画像認証文字のいずれかに誤りがあります。")

                    nextUrl = URL_LOGIN

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "アカウント修正"}
                        .Panel_Account.Top = 12
                        .Panel_Account.Visible = True
                        .Label_Account.Text = .Label_Account.Text.Replace("%mode%", "修正")
                        .Text_Id.Text = account.Id
                        .Text_Passwd.Text = account.Password
                        .Label_Error.Text = .Label_Error.Text.Replace("%error%", "ガンホーID、パスワード、認証文字のいずれかに誤りがありますにゃ!!")
                        .Label_Error.Visible = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


            ' 画像認証
                Case html.Contains("入力内容に不備があります")

                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$chbSave") = "on"
                    parameter("ctl00$ctl00$MainContent$TopContent$txt") = ""
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim imageUrl = UrlParserHelper.GetBaseDomainUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "img", "alt", "ひらがな認証", "src")
                    Dim captchaImage As Image

                    Try
                        captchaImage = Await httpClient.GetImageAsync(imageUrl)
                    Catch ex As Exception
                        MessageBox.Show($"エラー : 認証画像の取得に失敗しましたにゃ!!{vbCrLf}GetLastUrl={imageUrl}{vbCrLf}HttpRequestException={ex.Message}")
                        Return 0
                    End Try


                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "画像認証"}
                        .Panel_Image.Top = 12
                        .Panel_Image.Visible = True
                        .Label_Account.Text = .Label_Account.Text.Replace("%mode%", "修正")
                        .Label_Error.Text = .Label_Error.Text.Replace("%error%", "入力内容が間違っていますにゃ!!")
                        .Label_Error.Visible = True
                        .Image_Auth.Image = captchaImage
                        .Text_Image.Focus()
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    parameter("ctl00$ctl00$MainContent$TopContent$captchaControlAjax$txtCaptcha") = tempOtp



                Case html.Contains("ログイン画像認証")
                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$chbSave") = "on"
                    parameter("ctl00$ctl00$MainContent$TopContent$txt") = ""
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim imageUrl = UrlParserHelper.GetBaseDomainUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "img", "alt", "ひらがな認証", "src")
                    Dim captchaImage As Image

                    Try
                        captchaImage = Await httpClient.GetImageAsync(imageUrl)
                    Catch ex As Exception
                        MessageBox.Show($"エラー : 認証画像の取得に失敗しましたにゃ!!{vbCrLf}GetLastUrl={imageUrl}{vbCrLf}HttpRequestException={ex.Message}")
                        Return 0
                    End Try

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "画像認証"}
                        .Panel_Image.Top = 12
                        .Panel_Image.Visible = True
                        .Image_Auth.Image = captchaImage
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    parameter("ctl00$ctl00$MainContent$TopContent$captchaControlAjax$txtCaptcha") = tempOtp

            ' ワンタイムパスワード
                Case html.Contains("ワンタイムパスワードの認証に失敗しました")
                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Password
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")


                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "ワンタイムパスワード"}
                        .Panel_OTP.Top = 12
                        .Panel_OTP.Visible = True
                        .Label_Error.Text = .Label_Error.Text.Replace("%error%", "入力内容が間違っていますにゃ!!")
                        .Label_Error.Visible = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    parameter("ctl00$MainContent$OTPControl$inputOTP") = tempOtp


                Case html.Contains("ワンタイムパスワードを入力してください")

                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Password
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")


                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "ワンタイムパスワード"}
                        .Panel_OTP.Top = 12
                        .Panel_OTP.Visible = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    parameter("ctl00$MainContent$OTPControl$inputOTP") = tempOtp


            ' 電話認証
                Case html.Contains("STEP1")

                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "1").Replace("%message%", $"IDロック電話番号認証が必要ですにゃ!!{vbCrLf}認証を開始しますにゃ")
                        .Label_Tel.Visible = False
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


                Case html.Contains("STEP2")

                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")


                    Dim tel As String = HtmlParserHelper.GetHtmlInnerText(html, "span", "id", "MainContent_TopContent_labTelNumber")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "2").Replace("%message%", $"電話の準備が出来たらOKボタンを押してにゃ!!{vbCrLf}電話番号の末尾 [{tel}]")
                        .Label_Tel.Visible = False
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


                Case html.Contains("STEP3")

                    'parameter("__EVENTTARGET") = "ctl00$ctl00$MainContent$TopContent$ibtNext"
                    'parameter("__EVENTARGUMENT") = ""
                    'parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    'parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")

                    'nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")
                    nextUrl = HtmlParserHelper.GetHtmlValue(html, "META", "HTTP-EQUIV", "Refresh", "CONTENT")
                    nextUrl = nextUrl.Replace("10;URL=", "")
                    nextUrl = UrlParserHelper.GetBaseDomainUrl(httpClient.GetLastUrl()) & nextUrl

                    Dim tel As String = HtmlParserHelper.GetHtmlInnerText(html, "td", "class", "tbr authcode")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "3").Replace("%message%", $"下記の番号に発信してにゃ!!{vbCrLf}終わったらOKボタンを押してにゃ!!")
                        .Label_Tel.Text = tel
                        .Label_Tel.Visible = True
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                Case html.Contains("STEP<span id=""MainContent_TopContent_labStep"">4")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "4").Replace("%message%", $"電話番号認証に成功しましたにゃ!!")
                        .Label_Tel.Visible = False
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    nextUrl = URL_LOGIN

                Case html.Contains("電話番号認証に失敗しました。")

                    Return 0

                                ' ログインフォーム
                Case html.Contains("ログインが必要です")

                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Password
                    parameter("ctl00$MainContent$OTPControl$inputOTP") = ""
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserHelper.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                Case Else
                    Clipboard.SetText(html)
                    MessageBox.Show($"エラー : ページの解析に失敗しましたにゃ!!{vbCrLf}解析に失敗したページをクリップボードに出力しましたにゃ!!{vbCrLf}GetLastUrl={httpClient.GetLastUrl()}")
                    Return 0
            End Select

            Await Task.Delay(200)

        Next

        If success = False Then Return 0

        ' 正規表現パターンにゃ
        Dim pattern As String =
            "<span id=""(?<siid>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})"">[^<]*?</span>.*?" &
            "<span class=""details01"">(?<id>[^<]+)</span>.*?" &
            "<div[^>]*class=""list003[^""]*""[^>]*>(?<limit>.*?)</div>"

        Dim matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        For Each match As Match In matches
            Dim id As String = match.Groups("id").Value.Trim()
            Dim siid As String = match.Groups("siid").Value.Trim()
            Dim rawLimit As String = match.Groups("limit").Value
            Dim limit As String = Regex.Replace(rawLimit, "<.*?>", "") ' タグを全部消すにゃ
            limit = limit.Trim()
            gameAccounts.Add(New Game With {.Id = id, .Siid = siid, .Limit = limit})
        Next

        account.Cookie = httpClient.GetCookies(UrlParserHelper.GetBaseDomainUrl(URL_LOGIN))

        SaveAccountsToXml(Path.Combine(appPath, "accounts.xml"), loginAccounts)

        Return 1

    End Function

    Private Async Function GetGameCode() As Task(Of String)

        Dim html As String
        Dim roid = gameAccounts.FirstOrDefault(Function(n) n.Id.ToString = selectedGame)
        Dim nextUrl As String = $"{URL_GAMECODE}?SIID={roid.Siid}"

        Try
            html = Await httpClient.GetAsync(nextUrl)

        Catch ex As HttpRequestException
            MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
            Return Nothing
        End Try

        Dim pattern As String = "GameStartAsync\('([^']+)'\)"
        Dim match As Match = Regex.Match(html, pattern, RegexOptions.IgnoreCase)

        If Not match.Success Then Return Nothing

        Return match.Groups(1).Value

    End Function


    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        httpClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        httpClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        appSettings = LoadSettingsFromXml(Path.Combine(appPath, "settings.xml"))

        With appSettings
            Me.Left = .Left
            Me.Top = .Top
            Me.WindowState = CInt(.WindowState)
            MenuOption_MinimizeToTray.Checked = .MinimizeToTray
        End With

        loginAccounts = LoadAccountsFromXml(Path.Combine(appPath, "accounts.xml"))

        For Each account In loginAccounts
            List_Account.Items.Add(account.Id)
        Next


    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        If List_Account.Items.Count Then List_Account.SelectedIndex = 0

    End Sub
    Private Sub MainForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        With appSettings
            .Left = Me.Left
            .Top = Me.Top
            .WindowState = CInt(Me.WindowState)
            .MinimizeToTray = MenuOption_MinimizeToTray.Checked
        End With

        SaveSettingsToXml(Path.Combine(appPath, "settings.xml"), appSettings)
        SaveAccountsToXml(Path.Combine(appPath, "accounts.xml"), loginAccounts)

    End Sub

    Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize

        If Me.WindowState = FormWindowState.Minimized AndAlso MenuOption_MinimizeToTray.Checked = True Then Me.ShowInTaskbar = False

    End Sub

    Private Sub Button_Menu_Click(sender As Object, e As EventArgs) Handles Button_Account.Click, Button_Info.Click, Button_Option.Click

        Dim button = DirectCast(sender, Button)
        Dim menu As ContextMenuStrip

        Select Case button.Name
            Case "Button_Account" : menu = MenuAccount
            Case "Button_Info" : menu = MenuInfo
            Case "Button_Option" : menu = MenuOption
        End Select

        button.Enabled = False
        menu.Show(button, New Point(0, button.Height), ToolStripDropDownDirection.BelowRight)

    End Sub

    Private Async Sub Button_Play_Click(sender As Object, e As EventArgs) Handles Button_Play.Click

        Await List_Account_SelectedIndexChanged(List_Account, EventArgs.Empty)

        Dim code As String = Await GetGameCode()

        If code Is Nothing Then
            MessageBox.Show("エラー : ゲームコードの取得に失敗したにゃ!!")
            Exit Sub
        End If

        Try
            Process.Start($"ROEXEURI://-w&""{code}""")
        Catch ex As Exception
            MessageBox.Show($"エラー : アプリ起動に失敗したにゃ!!{vbCrLf}{ex.Message}")
        End Try


    End Sub

    Private Sub Button_Menu_Paint(sender As Object, e As PaintEventArgs) Handles Button_Account.Paint, Button_Info.Paint, Button_Option.Paint

        Dim button = DirectCast(sender, Button)
        Dim font As New Font("Marlett", button.Font.Size)
        Dim size As String = "6" 'Marlettの「6」が下向きの三角マークになるにゃ
        Dim sizef As SizeF = e.Graphics.MeasureString(size, font)
        Using brsh As New SolidBrush(button.ForeColor)
            e.Graphics.DrawString(size, font, brsh, button.ClientRectangle.Width - sizef.Width - 3, (button.ClientRectangle.Height - sizef.Height) / 2.0F)
        End Using

    End Sub


    Private Sub Menu_Button_Closing(sender As Object, e As ToolStripDropDownClosingEventArgs) Handles MenuAccount.Closing, MenuInfo.Closing, MenuOption.Closing

        Select Case DirectCast(sender, ContextMenuStrip).Name
            Case "MenuAccount" : Button_Account.Enabled = True
            Case "MenuInfo" : Button_Info.Enabled = True
            Case "MenuOption" : Button_Option.Enabled = True
        End Select

    End Sub

    Private Async Function List_Account_SelectedIndexChanged(sender As Object, e As EventArgs) As Task Handles List_Account.SelectedIndexChanged

        List_Game.Items.Clear()

        Button_Play.Enabled = False
        Button_Info.Enabled = False

        If List_Account.SelectedIndex = -1 Then
            MenuAccount_Remove.Enabled = False
            Exit Function
        End If

        MenuAccount_Remove.Enabled = True

        selectedAccount = List_Account.SelectedItem.ToString()

        If Await Login() = 0 Then Exit Function

        For Each item In gameAccounts
            List_Game.Items.Add($"{item.Id} [{item.Limit}]")
        Next

        If List_Game.Items.Count Then
            List_Game.SelectedIndex = Math.Max(List_Game.FindString(selectedGame), 0)
            selectedGame = List_Game.SelectedItem.ToString().Split(" ")(0)
            Button_Play.Enabled = True
            Button_Info.Enabled = True
        Else
            selectedGame = ""
        End If

    End Function



    Private Sub List_Game_SelectedIndexChanged(sender As Object, e As EventArgs) Handles List_Game.SelectedIndexChanged

        If List_Game.SelectedIndex = -1 Then
            selectedGame = ""
            Exit Sub
        End If

        selectedGame = List_Game.SelectedItem.ToString.Split(" ")(0)

    End Sub
    Private Sub List_Game_DoubleClick(sender As Object, e As EventArgs) Handles List_Game.DoubleClick

        Button_Play.PerformClick()

    End Sub

    Private Sub MenuAccount_Add_Click(sender As Object, e As EventArgs) Handles MenuAccount_Add.Click

        With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "アカウント追加"}
            .Label_Account.Text = .Label_Account.Text.Replace("%mode%", "追加")
            .Panel_Account.Top = 12
            .Panel_Account.Visible = True
            If .ShowDialog() <> DialogResult.OK Then Exit Sub
            List_Account.Items.Add(loginAccounts(loginAccounts.Count - 1).Id)

        End With

    End Sub


    Private Async Sub MenuAccount_Tool_Click(sender As Object, e As EventArgs) Handles MenuAccount_Tool.Click

        If List_Account.SelectedIndex = -1 Then
            System.Diagnostics.Process.Start(URL_RO_TOOL)
            Exit Sub
        End If

        Dim webClient As New HttpClientHelper()
        Dim html As String

        webClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        webClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        Dim nextUrl As String = $"{URL_RO_LOGIN}?ReturnUrl={URL_RO_TOOL}"

        Try
            html = Await webClient.GetAsync(nextUrl)
        Catch ex As HttpRequestException
            MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
            Exit Sub
        End Try

        nextUrl = UrlParserHelper.GetBaseDirUrl(webClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "ragnarockmaster", "action")
        nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")


        Dim temp As String = $"
<!-- saved from url=(0014)about:internet -->
<html>
    <body onLoad=""setTimeout('document.NekoRoginHtml.submit()', 2000);"">
        アトラクションツール にログインしています。ちょっと待ってにゃ!!<br />
        <form name=""NekoRoginHtml"" method=""post"" action=""{nextUrl}"">
            <input type=""hidden"" name=""__EVENTTARGET"" value="""" />
            <input type=""hidden"" name=""__EVENTARGUMENT"" value="""" />
            <input type=""hidden"" name=""__VIEWSTATE"" value=""{HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")}""/>
            <input type=""hidden"" name=""__VIEWSTATEGENERATOR"" value=""{HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$loginNameControl$txtLoginName"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccount).Id}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$passwordControl$txtPassword"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccount).Password}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$OTPControl$inputOTP"" value=""""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$login"" value="""" />
        </form>
    </body>
</html>"

        Using writer As New StreamWriter(Path.Combine(appPath, "temp.html"))
            writer.WriteLine(temp)
        End Using

        Win32Helper.ShellExecute(Path.Combine(appPath, "temp.html"))

        Await Task.Delay(2000)

        System.IO.File.Delete(Path.Combine(appPath, "temp.html"))

    End Sub

    Private Sub MenuOption_Shell_Click(sender As Object, e As EventArgs) Handles MenuOption_Shell.Click, MenuOption_ShellChatLog.Click, MenuOption_ShellScreenShot.Click, MenuOption_ShellMusic.Click

        Dim dirName As String = DirectCast(sender, ToolStripItem).Tag.ToString()

        Dim roPath As String = Win32Helper.GetRegistryValue("HKEY_CLASSES_ROOT\ROEXEURI\shell\open\command", Nothing)
        If String.IsNullOrEmpty(roPath) Then
            MessageBox.Show($"エラー : ROEXEURIのレジストリが見つからないにゃ!!{vbCrLf}RagnarokOnlineのインストールを確認してにゃ!!")
            Exit Sub
        End If

        roPath = Path.GetDirectoryName(roPath.Replace("""%1""", "").Replace("""", ""))

        Win32Helper.ShellExecute(Path.Combine(roPath, dirName))

    End Sub

    Private Sub MenuOption_Setup_Click_1(sender As Object, e As EventArgs) Handles MenuOption_Setup.Click

        Dim roPath As String = Win32Helper.GetRegistryValue("HKEY_CLASSES_ROOT\ROEXEURI\shell\open\command", Nothing)
        If String.IsNullOrEmpty(roPath) Then
            MessageBox.Show($"エラー : ROEXEURIのレジストリが見つからないにゃ!!{vbCrLf}RagnarokOnlineのインストールを確認してにゃ!!")
            Exit Sub
        End If

        roPath = Path.GetDirectoryName(roPath.Replace("""%1""", "").Replace("""", ""))

        Win32Helper.ShellExecute(Path.Combine(roPath, "Setup.exe"))
    End Sub

    Private Sub MenuOption_About_Click(sender As Object, e As EventArgs) Handles MenuOption_About.Click

        MessageBox.Show($"NekoRoginSP {My.Application.Info.Version}{vbCrLf}{My.Application.Info.Copyright} / {My.Application.Info.CompanyName}")

    End Sub

    Private Sub MenuAccount_Remove_Click(sender As Object, e As EventArgs) Handles MenuAccount_Remove.Click

        Dim index As Integer = List_Account.SelectedIndex

        If index = -1 Then Exit Sub

        If MessageBox.Show($"ガンホーID[{List_Account.SelectedItem.ToString()}]を削除しますにゃ？", "", MessageBoxButtons.YesNo) <> DialogResult.Yes Then Exit Sub

        List_Account.Items.RemoveAt(index)
        loginAccounts.RemoveAt(index)

        List_Account.SelectedIndex = -1

    End Sub

    Private Async Sub MenuInfo_Character_Click(sender As Object, e As EventArgs) Handles MenuInfo_Character.Click, MenuInfo_Quest.Click

        Dim webClient As New HttpClientHelper()
        Dim html As String

        webClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        webClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        Dim roid = gameAccounts.FirstOrDefault(Function(n) n.Id.ToString = selectedGame)

        Dim menu = DirectCast(sender, ToolStripMenuItem)

        Dim nextUrl As String = IIf(DirectCast(sender, ToolStripMenuItem).Name = "MenuInfo_Character", $"{URL_RO_CHARACTER}?SIID={roid.Siid}", $"{URL_RO_QUEST}?SIID={roid.Siid}")

        Try
            html = Await webClient.GetAsync(nextUrl)
        Catch ex As HttpRequestException
            MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
            Exit Sub
        End Try


        nextUrl = UrlParserHelper.GetBaseDirUrl(webClient.GetLastUrl()) & HtmlParserHelper.GetHtmlValue(html, "form", "id", "sitemaster", "action")
        nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")


        Dim temp As String = $"
<!-- saved from url=(0014)about:internet -->
<html>
    <body onLoad=""setTimeout('document.NekoRoginHtml.submit()', 2000);"">
        {menu.Text} にログインしています。ちょっと待ってにゃ!!<br />
        <form name=""NekoRoginHtml"" method=""post"" action=""{nextUrl}"">
            <input type=""hidden"" name=""__EVENTTARGET"" value="""" />
            <input type=""hidden"" name=""__EVENTARGUMENT"" value="""" />
            <input type=""hidden"" name=""__VIEWSTATE"" value=""{HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")}""/>
            <input type=""hidden"" name=""__VIEWSTATEGENERATOR"" value=""{HtmlParserHelper.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")}""/>
            <input type=""hidden"" name=""ctl00$MainContent$loginNameControl$txtLoginName"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccount).Id}""/>
            <input type=""hidden"" name=""ctl00$MainContent$passwordControl$txtPassword"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccount).Password}""/>
            <input type=""hidden"" name=""ctl00$MainContent$OTPControl$inputOTP"" value=""""/>
            <input type=""hidden"" name=""ctl00$MainContent$btNext1"" value="""" />
        </form>
    </body>
</html>"

        Using writer As New StreamWriter(Path.Combine(appPath, "temp.html"))
            writer.WriteLine(temp)
        End Using

        Win32Helper.ShellExecute(Path.Combine(appPath, "temp.html"))

        Await Task.Delay(2000)

        System.IO.File.Delete(Path.Combine(appPath, "temp.html"))

    End Sub

    Private Sub MenuOption_MinimizeToTray_Click(sender As Object, e As EventArgs) Handles MenuOption_MinimizeToTray.Click

        MenuOption_MinimizeToTray.Checked = Not MenuOption_MinimizeToTray.Checked

    End Sub

    Private Sub TasktrayIcon_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles TasktrayIcon.MouseDoubleClick

        With Me
            .ShowInTaskbar = True
            .WindowState = 0
            .Show()
        End With

    End Sub

    Private Sub TasktrayIcon_MouseUp(sender As Object, e As MouseEventArgs) Handles TasktrayIcon.MouseUp


        If e.Button = MouseButtons.Right Then
            ' メニュー
            Dim menu = MenuTasktray

            menu.Items.Clear()

            For Each acc In loginAccounts
                Dim item As New ToolStripMenuItem(acc.Id)
                item.Name = $"TasktrayAccountsMenuItem_{acc.Id}"
                If acc.Id = selectedAccount Then item.Checked = True
                AddHandler item.Click, AddressOf TasktrayAccountsMenuItem_Clicked
                menu.Items.Add(item)
            Next

            menu.Items.Add(New ToolStripSeparator())

            For Each acc In gameAccounts
                Dim item As New ToolStripMenuItem($"{acc.Id} [{acc.Limit}]")
                item.Name = $"TasktrayGamesMenuItem_{acc.Id}"
                If acc.Id = selectedGame Then item.Checked = True
                AddHandler item.Click, AddressOf TasktrayGamesMenuItem_Clicked
                menu.Items.Add(item)
            Next

            menu.Items.Add(New ToolStripSeparator())

            ' 既存メニューを追加
            MergeMenusWithHandler(menu, MenuOption, AddressOf CommonClickHandler)

            ' カーソルの絶対位置（スクリーン座標）
            Dim screen As Screen = Screen.FromPoint(Cursor.Position)
            Dim estimatedSize As New Size(200, menu.Items.Count * 24)

            Dim showPos As Point = Cursor.Position

            If showPos.X + estimatedSize.Width > screen.WorkingArea.Right Then showPos.X = screen.WorkingArea.Right - estimatedSize.Width
            If showPos.Y + estimatedSize.Height > screen.WorkingArea.Bottom Then showPos.Y = screen.WorkingArea.Bottom - estimatedSize.Height

            If showPos.X < screen.WorkingArea.Left Then showPos.X = screen.WorkingArea.Left
            If showPos.Y < screen.WorkingArea.Top Then showPos.Y = screen.WorkingArea.Top

            menu.Show(showPos)


        End If

    End Sub

    Private Function CloneMenuItemWithHandler(original As ToolStripItem, handler As EventHandler) As ToolStripItem
        If TypeOf original Is ToolStripMenuItem Then
            Dim orig = DirectCast(original, ToolStripMenuItem)
            Dim clone As New ToolStripMenuItem(orig.Text)
            clone.Name = $"{orig.Name}_Clone"
            clone.Checked = orig.Checked
            AddHandler clone.Click, handler

            ' サブメニューも再帰コピー
            For Each subItem As ToolStripItem In orig.DropDownItems
                clone.DropDownItems.Add(CloneMenuItemWithHandler(subItem, handler))
            Next
            Return clone
        ElseIf TypeOf original Is ToolStripSeparator Then
            Return New ToolStripSeparator()
        End If
        Return Nothing
    End Function


    Private Sub MergeMenusWithHandler(target As ContextMenuStrip, fromMenu As ContextMenuStrip, handler As EventHandler)
        For Each item As ToolStripItem In fromMenu.Items
            Dim cloned = CloneMenuItemWithHandler(item, handler)
            If cloned IsNot Nothing Then
                target.Items.Add(cloned)
            End If
        Next
    End Sub


    Private Sub CommonClickHandler(sender As Object, e As EventArgs)
        Dim item = DirectCast(sender, ToolStripMenuItem)

        Select Case item.Text
            Case "ゲーム設定" : MenuOption_Setup.PerformClick()
            Case "インストールフォルダを開く" : MenuOption_Shell.PerformClick()
            Case "チャットログ" : MenuOption_ShellChatLog.PerformClick()
            Case "スクリーンショット" : MenuOption_ShellScreenShot.PerformClick()
            Case "BGM" : MenuOption_ShellMusic.PerformClick()
            Case "最小化時にタスクトレイに格納する" : MenuOption_MinimizeToTray.PerformClick()
            Case "🐈️" : MenuOption_About.PerformClick()
            Case "アプリケーションの終了" : MenuOption_Quit.PerformClick()
        End Select


    End Sub
    Private Sub TasktrayAccountsMenuItem_Clicked(sender As Object, e As EventArgs)

        Dim id As String = DirectCast(sender, ToolStripMenuItem).Text

        List_Account.SelectedIndex = Math.Max(List_Account.FindString(id), 0)
    End Sub


    Private Sub TasktrayGamesMenuItem_Clicked(sender As Object, e As EventArgs)

        Dim id As String = DirectCast(sender, ToolStripMenuItem).Text.Split(" ")(0)

        List_Game.SelectedIndex = Math.Max(List_Game.FindString(id), 0)

        Button_Play.PerformClick()

    End Sub

    Private Sub MenuOption_Quit_Click(sender As Object, e As EventArgs) Handles MenuOption_Quit.Click

        Me.Close()

    End Sub
End Class
