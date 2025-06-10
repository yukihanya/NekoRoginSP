Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.WebRequestMethods
Imports System.Security.Policy
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Xml
Imports System.Xml.Serialization
Imports NekoRoginSP.AccountSaveData

Public Class MainForm

    Private Const URL_LOGIN As String = "https://member.gungho.jp/front/member/center.aspx"
    Private Const URL_GAMECODE As String = "https://member.gungho.jp/front/member/webgs/rocenter_old.aspx"
    Private Const URL_RO_TOOL As String = "https://member.gungho.jp/front/ro/guest/login.aspx?ReturnUrl=https://ragnarokonline.gungho.jp/tool/"
    Private Const URL_RO_CHARACTER As String = "https://member.gungho.jp/front/member/sns/ro.aspx"
    Private Const URL_RO_QUEST As String = "https://member.gungho.jp/front/member/sns/ro_quest.aspx"

    Private appPath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)


    Private Structure Game
        Dim Id As String
        Dim Siid As String
        Dim Limit As String
    End Structure

    Public selectedAccountId As String
    Private selectedGameId As String
    Private gameAccounts As New List(Of Game)
    Private httpClient As New HttpClientHelper()
    Public tempOtp As String


    Private Sub BuildContextMenu()

        Dim menu = MenuTasktray

        menu.Items.Clear()

        ' IDList_A を追加
        For Each acc In loginAccounts
            Dim item As New ToolStripMenuItem(acc.Id)
            AddHandler item.Click, AddressOf MenuItemClicked
            menu.Items.Add(item)
        Next

        ' 区切り線
        menu.Items.Add(New ToolStripSeparator())

        ' IDList_B を追加
        For Each acc In gameAccounts
            Dim item As New ToolStripMenuItem(acc.Id)
            AddHandler item.Click, AddressOf MenuItemClicked
            menu.Items.Add(item)
        Next

        ' 最後のセパレータ
        menu.Items.Add(New ToolStripSeparator())
    End Sub

    Private Sub MenuItemClicked(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        MessageBox.Show("選択された: " & item.Text)
    End Sub

    Private Async Function Login() As Task(Of Integer)

        Dim html As String
        Dim nextUrl As String = URL_LOGIN
        Dim parameter As New Dictionary(Of String, String)
        Dim account = loginAccounts.FirstOrDefault(Function(n) n.Id = selectedAccountId)
        Dim success As Boolean

        httpClient = New HttpClientHelper()

        httpClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        httpClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        account.Cookie.Remove("goessosst")

        httpClient.SetCookies(UrlParserUtil.GetBaseDomainUrl(URL_LOGIN), account.Cookie)

        ' 一定数のページ切り替えがある時はログイン不能と見なして接続試行ループを抜けるにゃ
        For i As Integer = 0 To 9

            ' URLに含まれるGETクエリが変換されるのを防ぐにゃ
            nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")

            ' ログインをするにゃ
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
                        .Text_Passwd.Text = account.Passwd
                        .Label_Error.Text = .Label_Error.Text.Replace("%error%", "ガンホーID、パスワード、認証文字のいずれかに誤りがありますにゃ!!")
                        .Label_Error.Visible = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


            ' 画像認証
                Case html.Contains("入力内容に不備があります")

                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$chbSave") = "on"
                    parameter("ctl00$ctl00$MainContent$TopContent$txt") = ""
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim imageUrl = UrlParserUtil.GetBaseDomainUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "img", "alt", "ひらがな認証", "src")
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

                    parameter("ctl00$ctl00$MainContent$TopContent$captchaControlAjax$txtCaptcha") = UrlEncodeUtil.UrlEncodeSjis(tempOtp)

                Case html.Contains("ログイン画像認証")
                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$chbSave") = "on"
                    parameter("ctl00$ctl00$MainContent$TopContent$txt") = ""
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim imageUrl = UrlParserUtil.GetBaseDomainUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "img", "alt", "ひらがな認証", "src")
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

                    parameter("ctl00$ctl00$MainContent$TopContent$captchaControlAjax$txtCaptcha") = UrlEncodeUtil.UrlEncodeSjis(tempOtp)


            ' ワンタイムパスワード
                Case html.Contains("ワンタイムパスワードの認証に失敗しました")
                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Passwd
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")


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
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Passwd
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")


                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "ワンタイムパスワード"}
                        .Panel_OTP.Top = 12
                        .Panel_OTP.Visible = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With

                    parameter("ctl00$MainContent$OTPControl$inputOTP") = tempOtp


            ' ログインフォーム
                Case html.Contains("ログインが必要です")

                    parameter("__LASTFOCUS") = ""
                    parameter("__EVENTTARGET") = ""
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$MainContent$loginNameControl$txtLoginName") = account.Id
                    parameter("ctl00$MainContent$passwordControl$txtPassword") = account.Passwd
                    parameter("ctl00$MainContent$OTPControl$inputOTP") = ""
                    parameter("ctl00$MainContent$btNext1") = ""

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Clipboard.SetText(html)
                    For Each kvp As KeyValuePair(Of String, String) In parameter
                        Console.WriteLine(kvp.Key & "=" & kvp.Value)
                    Next



            ' 電話認証
                Case html.Contains("STEP1")

                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "1").Replace("%message%", $"IDロック電話番号認証が必要ですにゃ!!{vbCrLf}認証を開始しますにゃ")
                        .Label_Tel.Visible = False
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


                Case html.Contains("STEP2")

                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.x") = "0"
                    parameter("ctl00$ctl00$MainContent$TopContent$ibtNext.y") = "0"

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim tel As String = HtmlParserUtil.GetHtmlInnerText(html, "span", "id", "MainContent_TopContent_labTelNumber")

                    With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "IDロック電話番号認証"}
                        .Panel_IdLock.Top = 12
                        .Panel_IdLock.Visible = True
                        .Label_IdLock.Text = .Label_IdLock.Text.Replace("%step%", "2").Replace("%message%", $"電話の準備が出来たらOKボタンを押してにゃ!!{vbCrLf}電話番号の末尾 [{tel}]")
                        .OK_Button.Enabled = True
                        If .ShowDialog() <> DialogResult.OK Then Return 0
                    End With


                Case html.Contains("STEP3")

                    parameter("__EVENTTARGET") = "ctl00$ctl00$MainContent$TopContent$ibtNext"
                    parameter("__EVENTARGUMENT") = ""
                    parameter("__VIEWSTATE") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")
                    parameter("__VIEWSTATEGENERATOR") = HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")

                    nextUrl = UrlParserUtil.GetBaseDirUrl(httpClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

                    Dim tel As String = HtmlParserUtil.GetHtmlInnerText(html, "td", "class", "tbr authcode")

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

                Case Else
                    Clipboard.SetText(html)
                    MessageBox.Show($"エラー : ページの解析に失敗しましたにゃ!!{vbCrLf}解析に失敗したページをクリップボードに出力しましたにゃ!!{vbCrLf}GetLastUrl={httpClient.GetLastUrl()}")
                    Return 0
            End Select

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

        account.Cookie = httpClient.GetCookies(UrlParserUtil.GetBaseDomainUrl(URL_LOGIN))

        SaveAccountsToXml(Path.Combine(appPath, "accounts.xml"), loginAccounts)

        Return 1

    End Function

    Private Async Function GetGameCode() As Task(Of String)

        Dim html As String
        Dim roid = gameAccounts.FirstOrDefault(Function(n) n.Id.ToString = selectedGameId)
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

        ' ゲーム起動
        If List_Game.SelectedIndex = -1 Then Exit Sub

        selectedGameId = List_Game.SelectedItem.ToString.Split(" ")(0)

        gameAccounts.Clear()
        List_Game.Items.Clear()

        If Await Login() = 0 Then Exit Sub

        For Each item In gameAccounts
            List_Game.Items.Add($"{item.Id} [{item.Limit}]")
        Next

        List_Game.SelectedIndex = List_Game.FindString(selectedGameId)

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

        Dim menu = DirectCast(sender, ContextMenuStrip)
        Dim button As Button
        Select Case menu.Name
            Case "MenuAccount" : button = Button_Account
            Case "MenuInfo" : button = Button_Info
            Case "MenuOption" : button = Button_Option
        End Select

        button.Enabled = True

    End Sub


    Private Async Sub List_Account_SelectedIndexChanged(sender As Object, e As EventArgs) Handles List_Account.SelectedIndexChanged

        Dim c = DirectCast(sender, ComboBox)

        If c.SelectedIndex = -1 Then Exit Sub

        gameAccounts.Clear()
        List_Game.Items.Clear()

        selectedAccountId = c.SelectedItem.ToString()
        selectedGameId = ""

        If Await Login() = 0 Then Exit Sub

        For Each item In gameAccounts
            List_Game.Items.Add($"{item.Id} [{item.Limit}]")
        Next

        BuildContextMenu()

        If List_Game.Items.Count Then List_Game.SelectedIndex = 0

    End Sub



    Private Sub List_Game_DoubleClick(sender As Object, e As EventArgs) Handles List_Game.DoubleClick

        Button_Play.PerformClick()

    End Sub

    Private Sub MenuAccount_Add_Click(sender As Object, e As EventArgs) Handles MenuAccount_Add.Click

        With New AuthForm() With {.Left = Me.Left, .Top = Me.Top, .Text = "アカウント追加"}
            .Label_Account.Text = .Label_Account.Text.Replace("%mode%", "追加")
            .Panel_Account.Top = 12
            .Panel_Account.Visible = True
            If .ShowDialog() = DialogResult.OK Then Exit Sub
            Dim index As Integer = List_Account.SelectedIndex
            List_Account.Items(index) = loginAccounts(index).Id
        End With

    End Sub


    Private Async Sub MenuAccount_Tool_Click(sender As Object, e As EventArgs) Handles MenuAccount_Tool.Click

        Dim webClient As New HttpClientHelper()
        Dim html As String

        webClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        webClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        Dim nextUrl As String = URL_RO_TOOL

        Try
            html = Await webClient.GetAsync(nextUrl)
        Catch ex As HttpRequestException
            MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
            Exit Sub
        End Try

        nextUrl = UrlParserUtil.GetBaseDirUrl(webClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "ragnarockmaster", "action")
        nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")


        Dim temp As String = $"
<!-- saved from url=(0014)about:internet -->
<html>
    <body onLoad=""setTimeout('document.NekoRoginHtml.submit()', 2000);"">
        {""} にログインしています。ちょっと待ってにゃ!!<br />
        <form name=""NekoRoginHtml"" method=""post"" action=""{nextUrl}"">
            <input type=""hidden"" name=""__EVENTTARGET"" value="""" />
            <input type=""hidden"" name=""__EVENTARGUMENT"" value="""" />
            <input type=""hidden"" name=""__VIEWSTATE"" value=""{HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")}""/>
            <input type=""hidden"" name=""__VIEWSTATEGENERATOR"" value=""{HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$loginNameControl$txtLoginName"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccountId).Id}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$passwordControl$txtPassword"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccountId).Passwd}""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$OTPControl$inputOTP"" value=""""/>
            <input type=""hidden"" name=""ctl00$ctl00$MainContent$TopContent$login"" value="""" />
        </form>
    </body>
</html>"

        Using writer As New StreamWriter(Path.Combine(appPath, "temp.html"))
            writer.WriteLine(temp)
        End Using

        Await Task.Delay(1000)

        Win32Util.ShellExecute(Path.Combine(appPath, "temp.html"))

    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        If List_Account.Items.Count Then List_Account.SelectedIndex = 0

    End Sub

    Private Sub MenuOption_Shell_Click(sender As Object, e As EventArgs) Handles MenuOption_Shell.Click, MenuOption_ShellChatLog.Click, MenuOption_ShellScreenShot.Click, MenuOption_ShellMusic.Click

        Dim dirName As String = DirectCast(sender, ToolStripItem).Tag.ToString()

        Dim roPath As String = Win32Util.GetRegistryValue("HKEY_CLASSES_ROOT\ROEXEURI\shell\open\command", Nothing)
        If String.IsNullOrEmpty(roPath) Then
            MessageBox.Show($"エラー : ROEXEURIのレジストリが見つからないにゃ!!{vbCrLf}RagnarokOnlineのインストールを確認してにゃ!!")
            Exit Sub
        End If

        roPath = Path.GetDirectoryName(roPath.Replace("""%1""", "").Replace("""", ""))

        Win32Util.ShellExecute(Path.Combine(roPath, dirName))

    End Sub

    Private Sub MenuOption_Setup_Click_1(sender As Object, e As EventArgs) Handles MenuOption_Setup.Click

        Dim roPath As String = Win32Util.GetRegistryValue("HKEY_CLASSES_ROOT\ROEXEURI\shell\open\command", Nothing)
        If String.IsNullOrEmpty(roPath) Then
            MessageBox.Show($"エラー : ROEXEURIのレジストリが見つからないにゃ!!{vbCrLf}RagnarokOnlineのインストールを確認してにゃ!!")
            Exit Sub
        End If

        roPath = Path.GetDirectoryName(roPath.Replace("""%1""", "").Replace("""", ""))

        Win32Util.ShellExecute(Path.Combine(roPath, "Setup.exe"))
    End Sub

    Private Sub MenuOption_About_Click(sender As Object, e As EventArgs) Handles MenuOption_About.Click

        MessageBox.Show($"{My.Application.Info.StackTrace}")

    End Sub

    Private Sub MenuAccount_Remove_Click(sender As Object, e As EventArgs) Handles MenuAccount_Remove.Click

        Dim index As Integer = List_Account.SelectedIndex

        If index = -1 Then Exit Sub

        If MessageBox.Show($"ガンホーID[{List_Account.SelectedItem}]を削除しますにゃ？", "", MessageBoxButtons.YesNo) <> DialogResult.OK Then Exit Sub

        List_Account.Items.Remove(index)
        loginAccounts.RemoveAt(index)

    End Sub

    Private Async Sub MenuInfo_Character_Click(sender As Object, e As EventArgs) Handles MenuInfo_Character.Click, MenuInfo_Quest.Click

        If List_Game.SelectedIndex = -1 Then Exit Sub

        selectedGameId = List_Game.SelectedItem.ToString.Split(" ")(0)

        Dim webClient As New HttpClientHelper()
        Dim html As String

        webClient.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) NekoBrowser/1.0")
        webClient.SetEncoding(Encoding.GetEncoding("Shift_JIS"))

        Dim roid = gameAccounts.FirstOrDefault(Function(n) n.Id.ToString = selectedGameId)

        Dim nextUrl As String

        Select Case DirectCast(sender, ToolStripMenuItem).Name
            Case "MenuInfo_Character"
                nextUrl = $"{URL_RO_CHARACTER}?SIID={roid.Siid}"
            Case "MenuInfo_Quest"
                nextUrl = $"{URL_RO_QUEST}?SIID={roid.Siid}"
        End Select


        Try
            html = Await webClient.GetAsync(nextUrl)
        Catch ex As HttpRequestException
            MessageBox.Show($"エラー : HTTPエラーですにゃ!!{vbCrLf}GetLastUrl={nextUrl}{vbCrLf}HttpRequestException={ex.Message}")
            Exit Sub
        End Try


        nextUrl = UrlParserUtil.GetBaseDirUrl(webClient.GetLastUrl()) & HtmlParserUtil.GetHtmlValue(html, "form", "id", "sitemaster", "action")

        nextUrl = nextUrl.Replace("./", "").Replace("//front", "/front").Replace("&amp;", "&")


        Dim temp As String = $"
<!-- saved from url=(0014)about:internet -->
<html>
    <body onLoad=""setTimeout('document.NekoRoginHtml.submit()', 2000);"">
        {""} にログインしています。ちょっと待ってにゃ!!<br />
        <form name=""NekoRoginHtml"" method=""post"" action=""{nextUrl}"">
            <input type=""hidden"" name=""__EVENTTARGET"" value="""" />
            <input type=""hidden"" name=""__EVENTARGUMENT"" value="""" />
            <input type=""hidden"" name=""__VIEWSTATE"" value=""{HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATE", "value")}""/>
            <input type=""hidden"" name=""__VIEWSTATEGENERATOR"" value=""{HtmlParserUtil.GetHtmlValue(html, "input", "name", "__VIEWSTATEGENERATOR", "value")}""/>
            <input type=""hidden"" name=""ctl00$MainContent$loginNameControl$txtLoginName"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccountId).Id}""/>
            <input type=""hidden"" name=""ctl00$MainContent$passwordControl$txtPassword"" value=""{loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = selectedAccountId).Passwd}""/>
            <input type=""hidden"" name=""ctl00$MainContent$OTPControl$inputOTP"" value=""""/>
            <input type=""hidden"" name=""ctl00$MainContent$btNext1"" value="""" />
        </form>
    </body>
</html>"

        Using writer As New StreamWriter(Path.Combine(appPath, "temp.html"))
            writer.WriteLine(temp)
        End Using

        Await Task.Delay(1000)

        Win32Util.ShellExecute(Path.Combine(appPath, "temp.html"))

    End Sub

    Private Sub TasktrayIcon_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles TasktrayIcon.MouseDoubleClick

        With Me
            .ShowInTaskbar = True
            .WindowState = 0
            .Show()
        End With

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

        If Me.WindowState = FormWindowState.Minimized Then Me.ShowInTaskbar = False

    End Sub

    Private Sub MenuOption_MinimizeToTray_Click(sender As Object, e As EventArgs) Handles MenuOption_MinimizeToTray.Click

        MenuOption_MinimizeToTray.Checked = Not MenuOption_MinimizeToTray.Checked

    End Sub
End Class
