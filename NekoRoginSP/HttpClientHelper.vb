Imports System.Net
Imports System.Net.Http
Imports System.Text

' HTTPリクエストを簡単に扱うためのヘルパークラスにゃ
Public Class HttpClientHelper

    Private handler As HttpClientHandler
    Private client As HttpClient
    Private cookieContainer As CookieContainer
    Private lastUrl As String
    Private responseEncoding As Encoding = Encoding.UTF8

    Public Sub New()
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls

        cookieContainer = New CookieContainer()
        handler = New HttpClientHandler() With {
            .CookieContainer = cookieContainer,
            .UseCookies = True,
            .AllowAutoRedirect = True,
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }
        client = New HttpClient(handler)
    End Sub

    ' エンコーディングを設定するにゃ
    Public Sub SetEncoding(ByVal enc As Encoding)
        responseEncoding = enc
    End Sub
    ' 最後のURLを取得するにゃ
    Public Function GetLastUrl() As String
        Return lastUrl
    End Function
    ' GETリクエスト
    Public Async Function GetAsync(ByVal url As String) As Task(Of String)
        Dim response = Await client.GetAsync(url)
        lastUrl = response.RequestMessage.RequestUri.ToString()
        If Not response.IsSuccessStatusCode Then
            Throw New HttpRequestException($"GETエラー : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
        End If

        Dim bytes = Await response.Content.ReadAsByteArrayAsync()
        Return responseEncoding.GetString(bytes)
    End Function

    ' POSTリクエスト
    Public Async Function PostAsync(ByVal url As String, ByVal data As Dictionary(Of String, String)) As Task(Of String)
        Dim enc As Encoding = responseEncoding

        Dim formBody As New List(Of String)
        For Each kvp In data
            Dim encodedKey = EncodeSJIS(kvp.Key, enc)
            Dim encodedValue = EncodeSJIS(kvp.Value, enc)
            formBody.Add($"{encodedKey}={encodedValue}")
        Next

        Dim rawData = String.Join("&", formBody)
        Dim content = New ByteArrayContent(enc.GetBytes(rawData))
        content.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded")
        content.Headers.ContentType.CharSet = enc.WebName

        Dim response = Await client.PostAsync(url, content)
        lastUrl = response.RequestMessage.RequestUri.ToString()
        If Not response.IsSuccessStatusCode Then
            Throw New HttpRequestException($"POSTエラー : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
        End If

        Dim bytes = Await response.Content.ReadAsByteArrayAsync()
        Return responseEncoding.GetString(bytes)
    End Function


    ' 画像の取得（Imageオブジェクトとして）
    Public Async Function GetImageAsync(ByVal url As String) As Task(Of Image)
        Dim response = Await client.GetAsync(url)
        'lastUrl = response.RequestMessage.RequestUri.ToString()
        If Not response.IsSuccessStatusCode Then
            Throw New HttpRequestException($"画像取得エラー : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
        End If
        Using stream As IO.Stream = Await response.Content.ReadAsStreamAsync()
            Return Image.FromStream(stream)
        End Using
    End Function

    ' Cookieをセットするにゃ
    Public Sub SetCookies(ByVal url As String, ByVal cookies As Dictionary(Of String, String))
        Dim uri = New Uri(url)
        For Each kvp In cookies
            Dim cookie = New Cookie(kvp.Key, kvp.Value)
            cookieContainer.Add(uri, cookie)
        Next
    End Sub

    ' Cookieを取得するにゃ
    Public Function GetCookies(ByVal url As String) As Dictionary(Of String, String)
        Dim uri = New Uri(url)
        Dim cookieCol = cookieContainer.GetCookies(uri)
        Dim dict As New Dictionary(Of String, String)
        For Each cookie In cookieCol
            dict(cookie.Name) = cookie.Value
        Next
        Return dict
    End Function

    Public Sub ClearCookies()
        cookieContainer = New CookieContainer()
        handler.CookieContainer = cookieContainer
    End Sub

    Public Sub RemoveExpiredCookies()
        Dim newContainer As New CookieContainer()

        ' CookieContainer からすべての Cookie を取得してコピーするにゃ（有効なものだけ）
        Dim domainField = GetType(CookieContainer).GetField("m_domainTable", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        Dim domains = CType(domainField.GetValue(cookieContainer), IDictionary)

        For Each key As String In domains.Keys
            Dim domain = key.TrimStart("."c)
            Dim uri As New Uri("http://" & domain)

            For Each cookie As Cookie In cookieContainer.GetCookies(uri)
                If cookie.Expires = DateTime.MinValue OrElse cookie.Expires > DateTime.Now Then
                    newContainer.Add(uri, cookie)
                End If
            Next
        Next

        cookieContainer = newContainer
        handler.CookieContainer = cookieContainer
    End Sub

    ' ユーザーエージェントを設定するにゃ
    Public Sub SetUserAgent(ByVal userAgent As String)
        client.DefaultRequestHeaders.UserAgent.Clear()
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)
    End Sub

    Public Sub SetHeader(name As String, value As String)
        If client.DefaultRequestHeaders.Contains(name) Then
            client.DefaultRequestHeaders.Remove(name)
        End If
        client.DefaultRequestHeaders.Add(name, value)
    End Sub

    Private Function EncodeSJIS(value As String, enc As Encoding) As String
        Return String.Join("", enc.GetBytes(value).Select(Function(b) "%" & b.ToString("X2")))
    End Function

End Class
