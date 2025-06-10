Imports System.Net
Imports System.Net.Http
Imports System.Runtime.Remoting.Contexts
Imports System.Security.Policy
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
            Throw New HttpRequestException($"GETエラーにゃ!! : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
        End If

        Dim bytes = Await response.Content.ReadAsByteArrayAsync()
        Return responseEncoding.GetString(bytes)
    End Function

    ' POSTリクエスト
    Public Async Function PostAsync(ByVal url As String, ByVal data As Dictionary(Of String, String)) As Task(Of String)
        Dim formBody As New List(Of String)
        For Each kvp In data
            formBody.Add($"{kvp.Key}={kvp.Value}")
        Next
        Dim rawData = String.Join("&", formBody)
        Dim content = New StringContent(rawData, responseEncoding, "application/x-www-form-urlencoded")

        Dim response = Await client.PostAsync(url, content)
        lastUrl = response.RequestMessage.RequestUri.ToString()
        If Not response.IsSuccessStatusCode Then
            Throw New HttpRequestException($"POSTエラーにゃ!! : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
        End If

        Dim bytes = Await response.Content.ReadAsByteArrayAsync()
        Return responseEncoding.GetString(bytes)

    End Function

    ' 画像の取得（Imageオブジェクトとして）
    Public Async Function GetImageAsync(ByVal url As String) As Task(Of Image)
        Dim response = Await client.GetAsync(url)
        'lastUrl = response.RequestMessage.RequestUri.ToString()
        If Not response.IsSuccessStatusCode Then
            Throw New HttpRequestException($"画像取得エラーにゃ!! : {(CInt(response.StatusCode))} {response.ReasonPhrase}")
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

    ' ユーザーエージェントを設定するにゃ
    Public Sub SetUserAgent(ByVal userAgent As String)
        client.DefaultRequestHeaders.UserAgent.Clear()
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)
    End Sub

End Class
