Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.Win32

Module NekoHelper
    ' ヘルパー関数
    Public Class Base64EncodeUtil

        ' Base64エンコード関数にゃ
        Public Shared Function Base64Encode(input As String) As String
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(input)
            Return Convert.ToBase64String(bytes)
        End Function

        ' Base64デコード関数にゃ
        Public Shared Function Base64Decode(base64 As String) As String
            Dim bytes As Byte() = Convert.FromBase64String(base64)
            Return Encoding.UTF8.GetString(bytes)
        End Function

    End Class

    Public Class UrlEncodeUtil

        ' URLエンコードを実装するにゃ
        Public Shared Function UrlEncodeSjis(ByVal input As String) As String
            Dim sjis As Encoding = Encoding.GetEncoding("Shift_JIS")
            Dim bytes() As Byte = sjis.GetBytes(input)
            Dim sb As New StringBuilder()

            For Each b As Byte In bytes
                ' 英数字や一部記号はそのまま、それ以外は %XX にゃ
                If (b >= &H30 AndAlso b <= &H39) OrElse ' 0-9
                   (b >= &H41 AndAlso b <= &H5A) OrElse ' A-Z
                   (b >= &H61 AndAlso b <= &H7A) OrElse ' a-z
                   b = &H2D OrElse b = &H5F OrElse b = &H2E OrElse b = &H7E Then ' - _ . ~
                    sb.Append(Chr(b))
                Else
                    sb.AppendFormat("%{0:X2}", b)
                End If
            Next

            Return sb.ToString()
        End Function

        Public Shared Function UrlEncodeUtf8(ByVal input As String) As String
            Dim sb As New StringBuilder()
            For Each c As Char In input
                If (AscW(c) >= 48 AndAlso AscW(c) <= 57) OrElse ' 0-9
                   (AscW(c) >= 65 AndAlso AscW(c) <= 90) OrElse ' A-Z
                   (AscW(c) >= 97 AndAlso AscW(c) <= 122) OrElse ' a-z
                   c = "-"c OrElse c = "_"c OrElse c = "."c OrElse c = "~"c Then
                    sb.Append(c)
                Else
                    Dim bytes() As Byte = Encoding.UTF8.GetBytes(c.ToString())
                    For Each b As Byte In bytes
                        sb.AppendFormat("%{0:X2}", b)
                    Next
                End If
            Next
            Return sb.ToString()
        End Function

    End Class


    Public Class HtmlParserUtil


        ' HTMLから特定のタグの属性値を取得するにゃ
        Public Shared Function GetHtmlValue(ByVal source As String, ByVal tag As String, ByVal attrKey As String, ByVal attrValue As String, ByVal returnAttr As String) As String
            ' タグの中で、attrKey=attrValue となっているものを探すにゃ
            ' attrKeyとreturnAttrの順番が入れ替わってもOKにゃ
            Dim pattern1 As String = $"<\s*{tag}[^>]*\b{attrKey}\s*=\s*""{Regex.Escape(attrValue)}""[^>]*?\b{returnAttr}\s*=\s*""([^""]*)"""
            Dim pattern2 As String = $"<\s*{tag}[^>]*\b{returnAttr}\s*=\s*""([^""]*)""[^>]*?\b{attrKey}\s*=\s*""{Regex.Escape(attrValue)}"""


            ' 両方試すにゃ
            Dim match = Regex.Match(source, pattern1, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If match.Success Then Return Net.WebUtility.HtmlDecode(match.Groups(1).Value)

            match = Regex.Match(source, pattern2, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If match.Success Then Return Net.WebUtility.HtmlDecode(match.Groups(1).Value)

            Return Nothing ' 見つからなかったにゃ

        End Function

        ' 特定のタグの中身を取得するにゃ
        Public Shared Function GetHtmlInnerText(ByVal source As String, ByVal tag As String, ByVal attrKey As String, ByVal attrValue As String) As String
            ' 正規表現でタグと属性を指定して中身を取得するにゃ
            Dim pattern As String = $"<{tag}[^>]*\b{attrKey}=""{Regex.Escape(attrValue)}""[^>]*>(.*?)</{tag}>"
            Dim match As Match = Regex.Match(source, pattern, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

            If match.Success Then Return match.Groups(1).Value

            Return Nothing ' 見つからなかったにゃ

        End Function


    End Class


    Public Class UrlParserUtil

        ' URLからベースURLを取得するにゃ
        Public Shared Function GetBaseDirUrl(ByVal fullUrl As String) As String
            Try
                ' Uriクラスを使ってパースするにゃ
                Dim uri As New Uri(fullUrl)
                Dim segments As String() = uri.AbsolutePath.Split("/"c)

                ' 最後がファイル名っぽかったら省くにゃ
                Dim basePath As String = "/"
                If segments.Length > 2 Then basePath = "/" & String.Join("/", segments.Take(segments.Length - 1)) & "/"


                ' scheme + host + パスを結合にゃ
                Return uri.Scheme & "://" & uri.Host & basePath
            Catch ex As Exception
                Return Nothing ' URLが変なときはNothing返すにゃ
            End Try
        End Function

        Public Shared Function GetBaseDomainUrl(ByVal fullUrl As String) As String
            Try
                Dim uri As New Uri(fullUrl)
                Return uri.Scheme & "://" & uri.Host & "/"
            Catch ex As UriFormatException
                ' URLが無効な場合は空文字列を返すにゃ
                Return ""
            End Try
        End Function

    End Class


    Public Class Win32Util

        ' レジストリの既定値を取得するにゃ
        Public Shared Function GetRegistryValue(fullPath As String, valueName As String) As String
            Dim rootKeyStr As String = ""
            Dim subKeyPath As String = ""

            ' ルートキーの判別にゃ
            If fullPath.StartsWith("HKEY_CLASSES_ROOT\") Then
                rootKeyStr = "HKEY_CLASSES_ROOT"
                subKeyPath = fullPath.Substring("HKEY_CLASSES_ROOT\".Length)
            ElseIf fullPath.StartsWith("HKEY_CURRENT_USER\") Then
                rootKeyStr = "HKEY_CURRENT_USER"
                subKeyPath = fullPath.Substring("HKEY_CURRENT_USER\".Length)
            ElseIf fullPath.StartsWith("HKEY_LOCAL_MACHINE\") Then
                rootKeyStr = "HKEY_LOCAL_MACHINE"
                subKeyPath = fullPath.Substring("HKEY_LOCAL_MACHINE\".Length)
            ElseIf fullPath.StartsWith("HKEY_USERS\") Then
                rootKeyStr = "HKEY_USERS"
                subKeyPath = fullPath.Substring("HKEY_USERS\".Length)
            ElseIf fullPath.StartsWith("HKEY_CURRENT_CONFIG\") Then
                rootKeyStr = "HKEY_CURRENT_CONFIG"
                subKeyPath = fullPath.Substring("HKEY_CURRENT_CONFIG\".Length)
            Else
                MessageBox.Show("不明なルートキーにゃ: " & fullPath)
                Return Nothing
            End If

            ' ルートキーを選ぶにゃ
            Dim rootKey As RegistryKey = Nothing
            Select Case rootKeyStr
                Case "HKEY_CLASSES_ROOT"
                    rootKey = Registry.ClassesRoot
                Case "HKEY_CURRENT_USER"
                    rootKey = Registry.CurrentUser
                Case "HKEY_LOCAL_MACHINE"
                    rootKey = Registry.LocalMachine
                Case "HKEY_USERS"
                    rootKey = Registry.Users
                Case "HKEY_CURRENT_CONFIG"
                    rootKey = Registry.CurrentConfig
            End Select

            ' サブキーを開いて値を取得するにゃ
            Try
                Using key As RegistryKey = rootKey.OpenSubKey(subKeyPath)
                    If key IsNot Nothing Then
                        Dim val = key.GetValue(If(String.IsNullOrEmpty(valueName), Nothing, valueName))
                        If val IsNot Nothing Then
                            Return val.ToString()
                        Else
                            MessageBox.Show($"値が見つからなかったにゃ!! : {valueName}")
                        End If
                    Else
                        MessageBox.Show($"サブキーが見つからないにゃ!! : {subKeyPath}")
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show($"エラーにゃ!! : {ex.Message}")
            End Try

            Return Nothing
        End Function


        Public Shared Sub ShellExecute(ByVal filePath As String, ByVal Optional argv As String = "")
            If File.Exists(filePath) OrElse Directory.Exists(filePath) Then
                Dim absPath As String = Path.GetFullPath(filePath)
                Process.Start(New ProcessStartInfo With {
                    .FileName = absPath,
                    .Arguments = argv,
                    .UseShellExecute = True
                })
            Else
                MessageBox.Show($"ファイルが見つかりませんにゃ!! : {filePath}")
            End If
        End Sub

    End Class



End Module
