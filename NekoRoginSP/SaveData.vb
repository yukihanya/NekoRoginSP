Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Serialization

Public Module AccountSaveData

    <Serializable>
    Public Class CookieEntry
        Public Property Key As String
        Public Property Value As String
    End Class

    <Serializable>
    <XmlType("Account")>
    Public Class Account
        Public Sub New()
            Cookie = New Dictionary(Of String, String)()
            CookieSerialized = New List(Of CookieEntry)()
        End Sub

        Public Property Id As String
        Public Property Passwd As String

        <XmlIgnore>
        Public Property Cookie As Dictionary(Of String, String)

        <XmlArray("CookieSerialized")>
        <XmlArrayItem("Cookie")>
        Public Property CookieSerialized As List(Of CookieEntry)
    End Class

    <Serializable>
    <XmlRoot("NekoRoginSP")>
    Public Class AccountList
        Public Property DataType As String = "Base64"

        <XmlArray("Accounts")>
        <XmlArrayItem("Account")>
        Public Property Accounts As List(Of Account)
    End Class

    <Serializable>
    <XmlRoot("NekoRoginSP")>
    Public Class EncryptedWrapper
        Public Property DataType As String = "Encrypted"
        Public Property EncryptData As String
    End Class

    Public loginAccounts As New List(Of Account)

    Public Sub PrepareForSave(accounts As List(Of Account))
        For Each acc In accounts
            ' Base64 encode the password
            If Not String.IsNullOrEmpty(acc.Passwd) Then
                acc.Passwd = Convert.ToBase64String(Encoding.UTF8.GetBytes(acc.Passwd))
            End If

            ' Cookie をシリアライズ可能形式にするにゃ
            acc.CookieSerialized = acc.Cookie.Select(Function(kv) New CookieEntry With {
            .Key = kv.Key,
            .Value = kv.Value
        }).ToList()
        Next
    End Sub

    Public Sub RestoreFromLoad(accounts As List(Of Account))
        For Each acc In accounts
            ' パスワードをBase64デコードにゃ
            If Not String.IsNullOrEmpty(acc.Passwd) Then
                Try
                    acc.Passwd = Encoding.UTF8.GetString(Convert.FromBase64String(acc.Passwd))
                Catch ex As FormatException
                    MessageBox.Show($"パスワードのBase64デコードに失敗したにゃ!! : {ex.Message}")
                    acc.Passwd = ""
                End Try
            End If

            ' Cookie を復元するにゃ
            acc.Cookie = acc.CookieSerialized.ToDictionary(Function(e) e.Key, Function(e) e.Value)
        Next
    End Sub

    Public Sub SaveAccountsToXml(filePath As String, accounts As List(Of Account))
        PrepareForSave(accounts)
        Dim serializer As New XmlSerializer(GetType(AccountList))
        Using writer As New StreamWriter(filePath)
            serializer.Serialize(writer, New AccountList With {.Accounts = accounts})
        End Using
        RestoreFromLoad(accounts)
    End Sub

    Public Function LoadAccountsFromXml(filePath As String) As List(Of Account)
        If Not File.Exists(filePath) Then
            MessageBox.Show($"エラー : ファイルが見つからなかったにゃ!!{vbCrLf}{filePath}")
            Return New List(Of Account)() ' 空リストを返すにゃ
        End If

        Try
            Dim serializer As New XmlSerializer(GetType(AccountList))
            Using reader As New StreamReader(filePath)
                Dim loaded As AccountList = DirectCast(serializer.Deserialize(reader), AccountList)
                RestoreFromLoad(loaded.Accounts)
                Return loaded.Accounts
            End Using
        Catch ex As Exception
            MessageBox.Show($"エラー : 読み込みエラーにゃ!!{vbCrLf}{ex.Message}")
            Return New List(Of Account)()
        End Try
    End Function


    Public Sub SaveEncryptedXmlBase64(filePath As String, accounts As List(Of Account), password As String)
        ' 通常のXML文字列を作成
        PrepareForSave(accounts)
        Dim serializer As New XmlSerializer(GetType(AccountList))
        Dim sb As New StringBuilder()
        Using writer As New StringWriter(sb)
            serializer.Serialize(writer, New AccountList With {.Accounts = accounts})
        End Using
        Dim plainXml As String = sb.ToString()

        ' 暗号化 → Base64
        Dim encryptedBytes = EncryptString(plainXml, password)
        Dim base64 = Convert.ToBase64String(encryptedBytes)

        ' ラップして再度XMLとして保存
        Dim wrapper As New EncryptedWrapper With {.EncryptData = base64}
        Dim wrapSerializer As New XmlSerializer(GetType(EncryptedWrapper))
        Using writer As New StreamWriter(filePath)
            wrapSerializer.Serialize(writer, wrapper)
        End Using
    End Sub

    Public Function LoadEncryptedXmlBase64(filePath As String, password As String) As List(Of Account)
        If Not File.Exists(filePath) Then
            Console.WriteLine($"エラー : ファイルがにゃいにゃ!! : {filePath}")
            Return New List(Of Account)()
        End If

        Try
            ' XMLラップ読み込み
            Dim wrapSerializer As New XmlSerializer(GetType(EncryptedWrapper))
            Dim wrapper As EncryptedWrapper
            Using reader As New StreamReader(filePath)
                wrapper = DirectCast(wrapSerializer.Deserialize(reader), EncryptedWrapper)
            End Using

            ' Base64 → バイナリ → 復号 → XML
            Dim encryptedBytes = Convert.FromBase64String(wrapper.EncryptData)
            Dim plainXml = DecryptString(encryptedBytes, password)

            ' 元のデータを復元
            Dim serializer As New XmlSerializer(GetType(AccountList))
            Using reader As New StringReader(plainXml)
                Dim loaded = DirectCast(serializer.Deserialize(reader), AccountList)
                RestoreFromLoad(loaded.Accounts)
                Return loaded.Accounts
            End Using
        Catch ex As Exception
            Console.WriteLine($"エラー : 読み込みエラーにゃ : {ex.Message}")
            Return New List(Of Account)()
        End Try
    End Function

    Public Function EncryptString(plainText As String, password As String) As Byte()
        Dim crypt = Aes.Create()
        crypt.Key = New Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("NekoSalt123")).GetBytes(32)
        crypt.GenerateIV()

        Using ms As New MemoryStream()
            ms.Write(crypt.IV, 0, crypt.IV.Length)
            Using cs As New CryptoStream(ms, crypt.CreateEncryptor(), CryptoStreamMode.Write)
                Using sw As New StreamWriter(cs)
                    sw.Write(plainText)
                End Using
            End Using
            Return ms.ToArray()
        End Using
    End Function

    Public Function DecryptString(cipherData As Byte(), password As String) As String
        Dim crypt = Aes.Create()
        Dim iv(15) As Byte
        Array.Copy(cipherData, 0, iv, 0, iv.Length)
        crypt.Key = New Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("NekoSalt123")).GetBytes(32)
        crypt.IV = iv

        Using ms As New MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length)
            Using cs As New CryptoStream(ms, crypt.CreateDecryptor(), CryptoStreamMode.Read)
                Using sr As New StreamReader(cs)
                    Return sr.ReadToEnd()
                End Using
            End Using
        End Using
    End Function

End Module


Public Module SettingSaveData


    <Serializable>
    Public Class WindowSettings
        Public Property Left As Integer
        Public Property Top As Integer
        Public Property WindowState As Integer
        Public Property MinimizeToTray As Boolean

    End Class

    <Serializable>
    Public Class SettingsGroup
        <XmlElement("Window")>
        Public Property Window As WindowSettings
    End Class

    <Serializable, XmlRoot("NekoRoginSP")>
    Public Class RootSettings
        <XmlElement("Settings")>
        Public Property Settings As SettingsGroup
    End Class


    Public appSettings As New WindowSettings

    ' 保存にゃ
    Public Sub SaveSettingsToXml(ByVal filePath As String, ByVal settings As WindowSettings)
        Dim root As New RootSettings With {
            .Settings = New SettingsGroup With {
                .Window = settings
            }
        }

        Dim serializer As New XmlSerializer(GetType(RootSettings))
        Using writer As New StreamWriter(filePath)
            serializer.Serialize(writer, root)
        End Using
    End Sub

    ' 読み込みにゃ
    Public Function LoadSettingsFromXml(ByVal filePath As String) As WindowSettings
        If File.Exists(filePath) Then
            Dim serializer As New XmlSerializer(GetType(RootSettings))
            Using reader As New StreamReader(filePath)
                Dim root As RootSettings = CType(serializer.Deserialize(reader), RootSettings)
                Return root.Settings.Window
            End Using
        Else
            ' デフォルト値にゃ
            Return New WindowSettings With {
                .Left = 100,
                .Top = 100,
                .MinimizeToTray = False,
                .WindowState = 0
            }
        End If
    End Function

End Module
