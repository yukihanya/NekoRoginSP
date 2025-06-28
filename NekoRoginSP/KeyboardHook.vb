Imports System.ComponentModel
Imports System.Runtime.InteropServices

Public Class KeyboardHook


    Implements IDisposable

    ''' <summary>
    ''' システム全体のキーダウンイベント。
    ''' </summary>
    Public Event KeyDown As EventHandler(Of KeyEventArgs)

    ''' <summary>
    ''' システム全体のキーアップイベント。
    ''' </summary>
    Public Event KeyUp As EventHandler(Of KeyEventArgs)


    Private Const WH_KEYBOARD_LL As Integer = 13
    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_KEYUP As Integer = &H101
    Private Const WM_SYSKEYDOWN As Integer = &H104
    Private Const WM_SYSKEYUP As Integer = &H105

    <StructLayout(LayoutKind.Sequential)>
    Private Structure KBDLLHOOKSTRUCT
        Public vkCode As UInteger
        Public scanCode As UInteger
        Public flags As UInteger
        Public time As UInteger
        Public dwExtraInfo As IntPtr
    End Structure

    Private Delegate Function LowLevelKeyboardProc(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function SetWindowsHookEx(idHook As Integer, lpfn As LowLevelKeyboardProc, hMod As IntPtr, dwThreadId As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function UnhookWindowsHookEx(hhk As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function CallNextHookEx(hhk As IntPtr, nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function GetModuleHandle(lpModuleName As String) As IntPtr
    End Function

    ' 修飾キーの状態を確実に取得するため
    <DllImport("USER32.dll")>
    Private Shared Function GetAsyncKeyState(vKey As Keys) As Short
    End Function


    ' コールバックデリゲートのインスタンス (GC防止のため保持)
    Private ReadOnly keyboardHookDelegate As LowLevelKeyboardProc
    ' フックハンドル
    Private hHook As IntPtr = IntPtr.Zero
    ' Disposeパターン用フラグ
    Private disposedValue As Boolean


    Public Sub New()
        ' デリゲートインスタンスを生成
        keyboardHookDelegate = AddressOf KeyboardHookCallback
    End Sub

    Protected Overrides Sub Finalize()
        ' ファイナライザは Dispose(False) を呼び出す (安全策)
        Dispose(disposing:=False)
        MyBase.Finalize()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを 'Dispose(disposing As Boolean)' メソッドに記述しますにゃ
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me) ' Disposeが呼ばれたらファイナライザは不要
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: マネージド状態を破棄します (マネージド オブジェクト)
                UninstallHook() ' フックを確実に停止
            End If

            ' TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドしますにゃ
            ' Ensure hook is stopped even if Dispose wasn't called explicitly (safety net)
            UninstallHook() ' ファイナライザからもStopを呼ぶ（念のため）

            disposedValue = True
        End If
    End Sub


    ''' <summary>
    ''' グローバルフックを開始します。
    ''' </summary>
    Public Sub InstallHook()
        If hHook <> IntPtr.Zero Then
            Throw New InvalidOperationException("フックは既にインストールされていますにゃ!!")
        End If
        If disposedValue Then
            Throw New ObjectDisposedException(Me.GetType().Name)
        End If

        Using curProcess As Process = Process.GetCurrentProcess()
            Using curModule As ProcessModule = curProcess.MainModule
                Dim moduleHandle As IntPtr = GetModuleHandle(curModule.ModuleName)
                ' 低レベルキーボードフックを設定 (dwThreadId = 0 でグローバル)
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookDelegate, moduleHandle, 0)
            End Using
        End Using

        If hHook = IntPtr.Zero Then
            Dim errorCode = Marshal.GetLastWin32Error()
            Throw New Win32Exception(errorCode, "SetWindowsHookEx に失敗しましたにゃ!!")
        End If
        Debug.WriteLine("GlobalKeyboardHook started.")
    End Sub

    ''' <summary>
    ''' グローバルフックを停止します。
    ''' </summary>
    Public Sub UninstallHook()
        If hHook = IntPtr.Zero Then Return

        If UnhookWindowsHookEx(hHook) Then
            Debug.WriteLine("GlobalKeyboardHook stopped.")
            hHook = IntPtr.Zero
        Else
            Dim errorCode = Marshal.GetLastWin32Error()
            ' UninstallHook/Dispose内で例外をスローするのは避けた方が良い場合もある
            Debug.WriteLine($"UnhookWindowsHookEx に失敗しましたにゃ!! エラーコード: {errorCode}")
            ' Throw New Win32Exception(errorCode, "UnhookWindowsHookEx に失敗しましたにゃ!!")
        End If
    End Sub


    ''' <summary>
    ''' 低レベルキーボードフックプロシージャ。
    ''' </summary>
    Private Function KeyboardHookCallback(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        If nCode >= 0 Then
            Try
                Dim kbdStruct As KBDLLHOOKSTRUCT = DirectCast(Marshal.PtrToStructure(lParam, GetType(KBDLLHOOKSTRUCT)), KBDLLHOOKSTRUCT)
                Dim vkCode As Keys = CType(kbdStruct.vkCode, Keys)

                ' 修飾キーの状態を取得 (GetAsyncKeyStateを使う方が確実)
                Dim modifiers As Keys = Keys.None
                If (GetAsyncKeyState(Keys.ShiftKey) And &H8000) <> 0 Then modifiers = modifiers Or Keys.Shift
                If (GetAsyncKeyState(Keys.ControlKey) And &H8000) <> 0 Then modifiers = modifiers Or Keys.Control
                If (GetAsyncKeyState(Keys.Menu) And &H8000) <> 0 Then modifiers = modifiers Or Keys.Alt ' Menuキー (Alt)

                ' イベント引数を作成
                Dim e As New KeyEventArgs(vkCode Or modifiers) ' KeyDataには修飾キーも含むにゃ

                Select Case wParam.ToInt32()
                    Case WM_KEYDOWN, WM_SYSKEYDOWN
                        ' KeyDownイベントを発生させるにゃ
                        RaiseEvent KeyDown(Me, e)

                    Case WM_KEYUP, WM_SYSKEYUP
                        ' KeyUpイベントを発生させるにゃ
                        RaiseEvent KeyUp(Me, e)
                End Select

                ' イベントハンドラで e.Handled = True にしても、
                ' ここで CallNextHookEx を呼ぶ限り、他のアプリへの伝播は止められないにゃ。
                ' 伝播を止めるには、特定の条件下で CallNextHookEx を呼ばずに new IntPtr(1) を返す必要があるが、
                ' 通常は推奨されないにゃ。

                ' ここで return 1 のように返すと、他のアプリケーションにキー入力が渡らなくなるにゃ（非推奨）
                ' return new IntPtr(1)


            Catch ex As Exception
                ' コールバック内での例外は握りつぶさず、デバッグ出力等で記録する
                Debug.WriteLine($"Error in KeyboardHookCallback: {ex.Message}")
            End Try
        End If

        ' 次のフックに処理を渡す (※これを呼ばないと他のフックやOSがキー入力を処理できなくなるにゃ)
        Return CallNextHookEx(hHook, nCode, wParam, lParam)
    End Function


End Class
