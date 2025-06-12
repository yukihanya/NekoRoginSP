Imports System.Windows.Forms

Public Class AuthForm

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        Dim id As String
        Dim passwd As String

        Select Case Me.Text
            Case "アカウント追加"
                id = Text_Id.Text
                passwd = Text_Passwd.Text

                If loginAccounts.Any(Function(n) n.Id = id) Then
                    MessageBox.Show($"エラー : 既に登録されているガンホーIDですにゃ!!{vbCrLf}id={id}")
                    Exit Select
                End If

                loginAccounts.Add(New Account With {.Id = id, .Passwd = passwd})

            Case "アカウント修正"
                id = Text_Id.Text
                passwd = Text_Passwd.Text

                If MainForm.selectedAccount <> id And loginAccounts.Any(Function(n) n.Id = id) Then
                    MessageBox.Show($"エラー : 既に登録されているガンホーIDですにゃ!!{vbCrLf}id={id}")
                    Exit Select
                End If

                With loginAccounts.FirstOrDefault(Function(n) n.Id.ToString() = MainForm.selectedAccount)
                    .Id = id
                    .Passwd = passwd
                End With

            Case "ワンタイムパスワード"
                MainForm.tempOtp = Text_OTP.Text
            Case "画像認証"
                MainForm.tempOtp = Text_Image.Text
        End Select

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub AuthForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub TextBoxes_GotFocus(sender As Object, e As EventArgs) Handles Text_Id.GotFocus, Text_Image.GotFocus

        With DirectCast(sender, TextBox)
            If .Text = .Tag Then
                .Text = ""
                .ForeColor = System.Drawing.Color.Black
            End If
        End With

    End Sub

    Private Sub TextBoxes_LosttFocus(sender As Object, e As EventArgs) Handles Text_Id.LostFocus, Text_Image.LostFocus


        With DirectCast(sender, TextBox)
            If .Text.Length = 0 Then
                .Text = .Tag
                .ForeColor = System.Drawing.Color.Gray
            End If
        End With

    End Sub

    Private Sub PasswdBoxes_GotFocus(sender As Object, e As EventArgs) Handles Text_Passwd.GotFocus, Text_OTP.GotFocus

        With DirectCast(sender, TextBox)
            If .Text = .Tag Then
                .Text = ""
                .ForeColor = System.Drawing.Color.Black
                .PasswordChar = "*"
            End If
        End With

    End Sub

    Private Sub PasswdBoxes_LostFocus(sender As Object, e As EventArgs) Handles Text_Passwd.LostFocus, Text_OTP.LostFocus

        With DirectCast(sender, TextBox)
            If .Text.Length = 0 Then
                .Text = .Tag
                .ForeColor = System.Drawing.Color.Gray
                .PasswordChar = ""
            End If
        End With

    End Sub

    Private Sub TextBoxes_TextChanged(sender As Object, e As EventArgs) Handles Text_OTP.TextChanged, Text_Image.TextChanged

        Dim b = DirectCast(sender, TextBox)

        If b.Text.Length = 0 OrElse b.Text = b.Tag Then
            OK_Button.Enabled = False
        Else
            OK_Button.Enabled = True
        End If


    End Sub

    Private Sub AccountBoxes_TextChanged(sender As Object, e As EventArgs) Handles Text_Id.TextChanged, Text_Passwd.TextChanged

        Dim b1 = Text_Id
        Dim b2 = Text_Passwd

        If b1.Text.Length = 0 OrElse b1.Text = b1.Tag Then
            OK_Button.Enabled = False
            Exit Sub
        End If

        If b2.Text.Length = 0 OrElse b2.Text = b2.Tag Then
            OK_Button.Enabled = False
            Exit Sub
        End If

        OK_Button.Enabled = True

    End Sub
End Class
