Option Strict On
Option Explicit On

Namespace Config
    Public Class LanguageManager
        Private Shared _currentLang As String = "th"
        Private Shared _strings As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)

        Shared Sub New()
            InitStrings()
        End Sub

        Public Shared Property CurrentLanguage As String
            Get
                Return _currentLang
            End Get
            Set(value As String)
                If value IsNot Nothing AndAlso (value.Equals("th", StringComparison.OrdinalIgnoreCase) OrElse
                                                 value.Equals("en", StringComparison.OrdinalIgnoreCase) OrElse
                                                 value.Equals("jp", StringComparison.OrdinalIgnoreCase)) Then
                    _currentLang = value.ToLower()
                End If
            End Set
        End Property

        Public Shared Function GetText(key As String) As String
            If _strings.ContainsKey(key) Then
                Dim langDict = _strings(key)
                If langDict.ContainsKey(_currentLang) Then
                    Return langDict(_currentLang)
                End If
                If langDict.ContainsKey("th") Then
                    Return langDict("th")
                End If
            End If
            Return key
        End Function

        Private Shared Sub AddString(key As String, th As String, en As String, jp As String)
            Dim d As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            d("th") = th
            d("en") = en
            d("jp") = jp
            _strings(key) = d
        End Sub

        Private Shared Sub InitStrings()
            ' ── Title ──
            AddString("AppTitle", "Auto Update", "Auto Update", "自動更新")

            ' ── Info Card ──
            AddString("InfoTitle", "ข้อมูลเครื่องทดสอบ", "Test Machine Info", "テストマシン情報")
            AddString("ComputerName", "ชื่อเครื่อง:", "Computer:", "マシン名:")
            AddString("Type", "ประเภท:", "Type:", "タイプ:")
            AddString("Mode", "โหมด:", "Mode:", "モード:")
            AddString("ScheduleTime", "เวลาตรวจสอบ:", "Check Time:", "チェック時間:")
            AddString("NotFoundInConfig", "(ไม่พบใน Config)", "(Not found in Config)", "(設定に見つかりません)")

            ' ── Version Card ──
            AddString("VersionTitle", "สถานะซอฟต์แวร์", "Software Status", "ソフトウェア状況")
            AddString("CurrentVersion", "เวอร์ชันปัจจุบัน:", "Current Version:", "現在のバージョン:")
            AddString("ServerVersion", "เวอร์ชัน Server:", "Server Version:", "サーバーバージョン:")
            AddString("Status", "สถานะ:", "Status:", "ステータス:")
            AddString("VersionNotFound", "(ไม่พบ)", "(Not found)", "(見つかりません)")
            AddString("VersionReadError", "(อ่านไม่ได้)", "(Cannot read)", "(読み取れません)")

            ' ── Status Values ──
            AddString("StatusPendingRestart", "● รอรีสตาร์ทเพื่ออัปเดต", "● Pending restart for update", "● 更新のため再起動待ち")
            AddString("StatusNotInstalled", "● ไม่พบโปรแกรมที่ติดตั้ง", "● Program not installed", "● プログラム未インストール")
            AddString("StatusServerError", "● ไม่สามารถอ่านเวอร์ชัน Server ได้", "● Cannot read server version", "● サーバーバージョン読み取り不可")
            AddString("StatusUpToDate", "● เป็นเวอร์ชันล่าสุดแล้ว", "● Up to date", "● 最新です")
            AddString("StatusUpdateAvailable", "● มีอัปเดตใหม่", "● Update available", "● 更新あり")

            ' ── Buttons ──
            AddString("BtnCheck", "ตรวจสอบ", "Check", "確認")
            AddString("BtnRefresh", "รีเฟรช", "Refresh", "更新")
            AddString("BtnExit", "ออก", "Exit", "終了")
            AddString("BtnUpdateNow", "อัปเดตทันที", "Update Now", "今すぐ更新")
            AddString("BtnDetails", "Details", "Details", "詳細")
            AddString("BtnDebugConfig", "[Debug] ดู Config ที่โหลดแล้ว", "[Debug] View loaded Config", "[Debug] ロード済み設定")

            ' ── Context Menu ──
            AddString("MenuCheckNow", "ตรวจสอบอัปเดตทันที", "Check for updates now", "今すぐ更新を確認")
            AddString("MenuExit", "ออกจากโปรแกรม", "Exit", "終了")

            ' ── Dialogs ──
            AddString("ConfirmUpdate", "ต้องการอัปเดตแอปพลิเคชันเดี๋ยวนี้หรือไม่?", "Do you want to update the application now?", "今すぐアプリケーションを更新しますか?")
            AddString("ConfirmTitle", "ยืนยัน", "Confirm", "確認")
            AddString("MachineNotInSystem", "เครื่องนี้ไม่อยู่ในระบบ (TesterType.csv)", "This machine is not in the system (TesterType.csv)", "このマシンはシステムにありません (TesterType.csv)")
            AddString("Updating", "กำลังอัปเดต...", "Updating...", "更新中...")

            ' ── Progress ──
            AddString("ProgressDownloading", "กำลังเตรียมดาวน์โหลด...", "Preparing download...", "ダウンロード準備中...")
            AddString("ProgressUninstalling", "กำลังดำเนินการถอนการติดตั้ง...", "Uninstalling...", "アンインストール中...")
            AddString("ProgressInstalling", "กำลังดำเนินการติดตั้ง...", "Installing...", "インストール中...")
            AddString("ProgressComplete", "การอัปเดตเสร็จสมบูรณ์", "Update completed", "更新完了")
            AddString("ProgressFailed", "ดาวน์โหลดล้มเหลว", "Download failed", "ダウンロード失敗")
            AddString("ProgressSearching", "กำลังค้นหาโปรแกรมที่ติดตั้ง...", "Searching for installed program...", "インストール済みプログラムを検索中...")

            ' ── Restart Prompt ──
            AddString("RestartPromptMsg", "ระบบรอการอัปเดตมานานกว่า 1 ชั่วโมงแล้ว" & Environment.NewLine & "กรุณารีสตาร์ทเครื่องเพื่อทำการอัปเดต System" & Environment.NewLine & Environment.NewLine & "ต้องการรีสตาร์ทตอนนี้หรือไม่?", "System has been waiting for update for over 1 hour." & Environment.NewLine & "Please restart to update the system." & Environment.NewLine & Environment.NewLine & "Restart now?", "システムは1時間以上更新を待っています。" & Environment.NewLine & "システムを更新するために再起動してください。" & Environment.NewLine & Environment.NewLine & "今すぐ再起動しますか?")
            AddString("RestartPromptTitle", "แจ้งเตือนอัปเดต", "Update Notice", "更新通知")

            ' ── Language ──
            AddString("LangTH", "TH", "TH", "TH")
            AddString("LangEN", "EN", "EN", "EN")
            AddString("LangJP", "JP", "JP", "JP")
        End Sub
    End Class
End Namespace
