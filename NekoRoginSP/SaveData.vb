Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Serialization

''' <summary>
''' アカウントデータの保存と読み込みを行うクラスにゃ
''' </summary>
Public Class AccountSaveData

    ''' <summary>設定ファイル名。省略時に使われるにゃ</summary>
    Public Property FileName As String

    ''' <summary>アカウント一覧</summary>
    Public Property Accounts As List(Of Account)

    ''' <summary>
    ''' コンストラクタ：ファイル名指定またはデフォルト。
    ''' New 時に自動で読み込みを実行するにゃ
    ''' </summary>
    Public Sub New(Optional fileName As String = "accounts.xml")
        Me.FileName = fileName
        Me.Accounts = New List(Of Account)()
        LoadAccountsFromXml()
    End Sub

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
        Public Property Password As String

        <XmlIgnore>
        Public Property Cookie As Dictionary(Of String, String)

        <XmlArray("CookieSerialized")>
        <XmlArrayItem("Cookie")>
        Public Property CookieSerialized As List(Of CookieEntry)

        Public Property CookieRenew As Long
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

    ''' <summary>アカウント保存用の準備 (パスワードBase64, Cookieシリアライズ)</summary>
    Private Sub PrepareForSave()
        For Each acc In Accounts
            If Not String.IsNullOrEmpty(acc.Password) Then
                acc.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(acc.Password))
            End If
            acc.CookieSerialized = acc.Cookie.Select(Function(kv) New CookieEntry With {.Key = kv.Key, .Value = kv.Value}).ToList()
        Next
    End Sub

    ''' <summary>読み込み後の復元 (パスワードデコード, Cookie復元)</summary>
    Private Sub RestoreFromLoad(list As List(Of Account))
        For Each acc In list
            If Not String.IsNullOrEmpty(acc.Password) Then
                Try
                    acc.Password = Encoding.UTF8.GetString(Convert.FromBase64String(acc.Password))
                Catch ex As FormatException
                    ' デコード失敗時は空文字に
                    acc.Password = String.Empty
                End Try
            End If
            acc.Cookie = acc.CookieSerialized.ToDictionary(Function(e) e.Key, Function(e) e.Value)
        Next
    End Sub

    ''' <summary>
    ''' XML形式でアカウントを保存。filePath省略時はFileNameを使用
    ''' </summary>
    Public Sub SaveAccountsToXml(Optional filePath As String = Nothing)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName
        PrepareForSave()
        Dim serializer = New XmlSerializer(GetType(AccountList))
        Using writer = New StreamWriter(filePath)
            serializer.Serialize(writer, New AccountList With {.Accounts = Accounts})
        End Using
        RestoreFromLoad(Accounts)
    End Sub

    ''' <summary>
    ''' XML形式でアカウントを読み込み。filePath省略時はFileNameを使用
    ''' </summary>
    Public Function LoadAccountsFromXml(Optional filePath As String = Nothing) As List(Of Account)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName
        If Not File.Exists(filePath) Then
            Accounts = New List(Of Account)()
            Return Accounts
        End If
        Try
            Dim serializer = New XmlSerializer(GetType(AccountList))
            Using reader = New StreamReader(filePath)
                Dim loaded = DirectCast(serializer.Deserialize(reader), AccountList)
                RestoreFromLoad(loaded.Accounts)
                Accounts = loaded.Accounts
                Return Accounts
            End Using
        Catch ex As Exception
            Accounts = New List(Of Account)()
            Return Accounts
        End Try
    End Function

    ''' <summary>
    ''' 暗号化したXMLをBase64で保存。filePath省略時はFileNameを使用
    ''' </summary>
    Public Sub SaveEncryptedXmlBase64(password As String, Optional filePath As String = Nothing)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName
        PrepareForSave()
        ' 通常XMLを作成
        Dim serializer = New XmlSerializer(GetType(AccountList))
        Dim sb As New StringBuilder()
        Using sw = New StringWriter(sb)
            serializer.Serialize(sw, New AccountList With {.Accounts = Accounts})
        End Using
        Dim plainXml = sb.ToString()
        ' 暗号化→Base64
        Dim encryptedBytes = EncryptString(plainXml, password)
        Dim base64 = Convert.ToBase64String(encryptedBytes)
        ' ラップして保存
        Dim wrapper = New EncryptedWrapper With {.EncryptData = base64}
        Dim wrapSerializer = New XmlSerializer(GetType(EncryptedWrapper))
        Using writer = New StreamWriter(filePath)
            wrapSerializer.Serialize(writer, wrapper)
        End Using
    End Sub

    ''' <summary>
    ''' 暗号化されたBase64 XMLを読み込み。filePath省略時はFileNameを使用
    ''' </summary>
    Public Function LoadEncryptedXmlBase64(password As String, Optional filePath As String = Nothing) As List(Of Account)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName
        If Not File.Exists(filePath) Then
            Accounts = New List(Of Account)()
            Return Accounts
        End If
        Try
            ' ラップ読み込み
            Dim wrapSerializer = New XmlSerializer(GetType(EncryptedWrapper))
            Dim wrapper As EncryptedWrapper
            Using reader = New StreamReader(filePath)
                wrapper = DirectCast(wrapSerializer.Deserialize(reader), EncryptedWrapper)
            End Using
            ' 復号
            Dim encryptedBytes = Convert.FromBase64String(wrapper.EncryptData)
            Dim plainXml = DecryptString(encryptedBytes, password)
            ' XML読み込み
            Dim serializer = New XmlSerializer(GetType(AccountList))
            Using sr = New StringReader(plainXml)
                Dim loaded = DirectCast(serializer.Deserialize(sr), AccountList)
                RestoreFromLoad(loaded.Accounts)
                Accounts = loaded.Accounts
                Return Accounts
            End Using
        Catch ex As Exception
            Accounts = New List(Of Account)()
            Return Accounts
        End Try
    End Function

    ''' <summary>文字列をAESで暗号化し、IVを先頭に付加するにゃ</summary>
    Public Function EncryptString(plainText As String, password As String) As Byte()
        Using crypt = Aes.Create()
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
        End Using
    End Function

    ''' <summary>AES暗号化データを復号するにゃ</summary>
    Public Function DecryptString(cipherData As Byte(), password As String) As String
        Using crypt = Aes.Create()
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
        End Using
    End Function

End Class



''' <summary>
''' 設定の保存と読み込みを行うクラスにゃ
''' </summary>
Public Class SettingSaveData

    ''' <summary>設定ファイル名。省略時に使われるにゃ</summary>
    Public Property FileName As String

    ''' <summary>ウィンドウ設定</summary>
    Public Property Window As WindowSettings

    ''' <summary>一般設定</summary>
    Public Property General As GeneralSettings

    ''' <summary>コンストラクタ：ファイル名指定またはデフォルト</summary>
    Public Sub New(Optional fileName As String = "settings.xml")
        Me.FileName = fileName
        Me.Window = New WindowSettings()
        Me.General = New GeneralSettings()
        ' 指定された FileName で設定を読み込む
        LoadSettingsFromXml()
    End Sub

    <Serializable>
    Public Class WindowSettings
        Public Property Left As Integer
        Public Property Top As Integer
        Public Property WindowState As Integer
        Public Property MinimizeToTray As Boolean
    End Class

    <Serializable>
    Public Class GeneralSettings
        <XmlElement("UseHotkey")>
        Public Property UseHotkey As Boolean
    End Class

    Public Class SettingsGroup
        <XmlElement("General")>
        Public Property General As GeneralSettings

        <XmlElement("Window")>
        Public Property Window As WindowSettings
    End Class

    <Serializable, XmlRoot("NekoRoginSP")>
    Public Class RootSettings
        <XmlElement("Settings")>
        Public Property Settings As SettingsGroup
    End Class

    ''' <summary>
    ''' 設定を XML に保存にゃ。filePath を省略すると FileName プロパティを使うにゃ
    ''' </summary>
    Public Sub SaveSettingsToXml(Optional filePath As String = Nothing)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName

        Dim root As New RootSettings With {
            .Settings = New SettingsGroup With {
                .General = General,
                .Window = Window
            }
        }
        Dim serializer As New XmlSerializer(GetType(RootSettings))
        Using writer As New StreamWriter(filePath)
            serializer.Serialize(writer, root)
        End Using
    End Sub

    ''' <summary>
    ''' 設定を XML から読み込みにゃ。filePath を省略すると FileName プロパティを使うにゃ
    ''' </summary>
    Public Sub LoadSettingsFromXml(Optional filePath As String = Nothing)
        If String.IsNullOrEmpty(filePath) Then filePath = FileName

        If File.Exists(filePath) Then
            Dim serializer As New XmlSerializer(GetType(RootSettings))
            Using reader As New StreamReader(filePath)
                Dim root As RootSettings = CType(serializer.Deserialize(reader), RootSettings)
                General = root.Settings.General
                Window = root.Settings.Window
            End Using
        Else
            ' ファイルがない時はデフォルト値にゃ
            General = New GeneralSettings With {.UseHotkey = False}
            Window = New WindowSettings With {
                .Left = 100,
                .Top = 100,
                .WindowState = 0,
                .MinimizeToTray = False
            }
        End If
    End Sub

End Class


