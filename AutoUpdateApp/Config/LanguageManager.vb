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

            AddString("ProgressUninstallingProduct", "กำลังถอนการติดตั้ง {0}...", "Uninstalling {0}...", "{0}をアンインストール中...")
            AddString("ProgressInstallingProduct", "กำลังติดตั้ง {0}...", "Installing {0}...", "{0}をインストール中...")
            AddString("ProgressDownloadingFile", "กำลังดาวน์โหลด: {0} ({1}/{2} ไฟล์)", "Downloading: {0} ({1}/{2} files)", "ダウンロード中: {0} ({1}/{2} ファイル)")

            ' ── Restart Prompt ──
            AddString("RestartPromptMsg", "ระบบรอการอัปเดตมานานกว่า 1 ชั่วโมงแล้ว" & Environment.NewLine & "กรุณารีสตาร์ทเครื่องเพื่อทำการอัปเดต System" & Environment.NewLine & Environment.NewLine & "ต้องการรีสตาร์ทตอนนี้หรือไม่?", "System has been waiting for update for over 1 hour." & Environment.NewLine & "Please restart to update the system." & Environment.NewLine & Environment.NewLine & "Restart now?", "システムは1時間以上更新を待っています。" & Environment.NewLine & "システムを更新するために再起動してください。" & Environment.NewLine & Environment.NewLine & "今すぐ再起動しますか?")
            AddString("RestartPromptTitle", "แจ้งเตือนอัปเดต", "Update Notice", "更新通知")

            ' ── Update Prompt Form ──
            AddString("PromptTitle", "แจ้งเตือนอัปเดต", "Update Alert", "アップデート通知")
            AddString("PromptNewVersion", "พบเวอร์ชันใหม่พร้อมอัปเดต!", "New version found and ready to update!", "新しいバージョンが見つかりました！")
            AddString("PromptCurrent", "ปัจจุบัน", "Current", "現在")
            AddString("PromptLatest", "ล่าสุด", "Latest", "最新")
            AddString("PromptUpdateNow", "อัปเดตตอนนี้", "Update Now", "今すぐ更新")
            AddString("PromptAfterRestart", "หลังรีสตาร์ท", "After Restart", "再起動後")
            AddString("PromptRemindLater", "เตือนทีหลัง", "Remind Later", "後で通知")

            AddString("PromptSuccess", "อัปเดตสำเร็จเรียบร้อยแล้ว", "Update completed successfully", "更新が正常に完了しました")
            AddString("PromptFailed", "อัปเดตไม่สำเร็จ กรุณาตรวจสอบ Log", "Update failed. Please check the log.", "更新に失敗しました。ログを確認してください")
            AddString("TitleSuccess", "สำเร็จ", "Success", "成功")
            AddString("CantRestart", "ไม่สามารถรีสตาร์ทได้: ", "Unable to restart: ", "再起動できません: ")
            AddString("TitleError", "ข้อผิดพลาด", "Error", "エラー")
            AddString("PromptFileNotFound", "ไม่พบไฟล์: ", "File not found: ", "ファイルが見つかりません: ")
            AddString("PromptFileNotFoundTitle", "ไม่พบไฟล์", "File Not Found", "ファイル未検出")
            AddString("PromptCantOpenFile", "ไม่สามารถเปิดไฟล์ได้: ", "Unable to open file: ", "ファイルを開けません: ")
            AddString("PromptPathNotConfigured", "ยังไม่ได้กำหนดเส้นทาง {0} ใน config.txt", "Path {0} is not configured in config.txt", "config.txt で {0} パスが設定されていません")
            AddString("PromptPathNotFoundTitle", "ไม่พบเส้นทาง", "Path Not Found", "パス未検出")
            AddString("PromptNoPdfInFolder", "ไม่พบไฟล์ PDF ในโฟลเดอร์: ", "No PDF files found in folder: ", "フォルダー内にPDFファイルが見つかりません: ")
            AddString("PromptChecking", "กำลังตรวจสอบ...", "Checking...", "確認中...")
            AddString("PromptCheckingUpdate", "กำลังตรวจสอบอัปเดต...", "Checking for updates...", "アップデート確認中...")
            AddString("PromptAlreadyChecking", "กำลังตรวจสอบอยู่แล้ว กรุณารอสักครู่", "Checking is already in progress, please wait.", "既に確認中のため、しばらくお待ちください。")
            AddString("PromptNotice", "แจ้งเตือน", "Notice", "通知")
            AddString("PromptCheckDone", "ตรวจสอบเสร็จ", "Check completed", "確認完了")
            AddString("TitleCheckResult", "ผลการตรวจสอบ", "Check Result", "確認結果")
            AddString("PromptSuccessCompleted", "อัปเดตสำเร็จเรียบร้อย!", "Update completed successfully!", "アップデートが正常に完了しました！")
            AddString("MsgNotInConfig", "เครื่องนี้ไม่อยู่ในระบบ (TesterType.csv)", "This machine is not in the system (TesterType.csv)", "このマシンはシステムにありません (TesterType.csv)")
            AddString("MsgHourNotMatching", "ยังไม่ถึงเวลาตรวจสอบตามที่ตั้งไว้", "Not the scheduled hour to check for updates.", "アップデート確認のスケジュール時間外です。")
            AddString("MsgAlreadyCheckedToday", "ได้ทำการตรวจสอบอัปเดตของวันนี้ไปแล้ว", "Already checked today.", "本日既にアップデート確認を行いました。")
            AddString("MsgUpToDate", "โปรแกรมเป็นเวอร์ชันล่าสุดแล้ว", "Program is up to date.", "プログラムは最新バージョンです。")
            AddString("MsgPendingRestart", "มีไฟล์อัปเดตที่รอการรีสตาร์ทเครื่องอยู่", "Pending restart update is already scheduled.", "再起動待ちのアップデートがスケジュールされています。")
            AddString("MsgInstallerNotFound", "ไม่พบไฟล์อัปเดตบน Server", "Installer files not found on server.", "サーバー上にアップデートファイルが見つかりません。")
            AddString("MsgCancelled", "การตรวจสอบถูกยกเลิก", "Check cancelled.", "確認がキャンセルされました。")
            AddString("DebugTitle", "Config Debug", "Config Debug", "設定デバッグ")
            AddString("DebugExeLocation", "══════ ตำแหน่ง EXE ══════", "══════ EXE Location ══════", "══════ EXEの場所 ══════")
            AddString("DebugConfigStatus", "══════ สถานะ Config ══════", "══════ Config Status ══════", "══════ 設定ファイルのステータス ══════")
            AddString("DebugReadValues", "══════ ค่าที่อ่านได้ ══════", "══════ Read Values ══════", "══════ 読み取られた値 ══════")

            ' ── Restart Notice Form ──
            AddString("RestartNoticeTitle", "⚠ แจ้งเตือนรีสตาร์ท", "⚠ Restart Required", "⚠ 再起動が必要です")
            AddString("RestartNoticeHeader", "กรุณารีสตาร์ทเครื่อง", "Please Restart Your Computer", "コンピュータを再起動してください")
            AddString("RestartNoticeBody", "ระบบได้ตั้งค่าอัปเดตเรียบร้อยแล้ว" & Environment.NewLine & "กรุณารีสตาร์ทเครื่องเพื่อทำการติดตั้งอัปเดต" & Environment.NewLine & Environment.NewLine & "หมายเหตุ: กรุณาบันทึกงานทั้งหมดก่อนรีสตาร์ท", "Update has been scheduled successfully." & Environment.NewLine & "Please restart your computer to install the update." & Environment.NewLine & Environment.NewLine & "Note: Please save all your work before restarting.", "アップデートが正常にスケジュールされました。" & Environment.NewLine & "アップデートをインストールするために再起動してください。" & Environment.NewLine & Environment.NewLine & "注意: 再起動前にすべての作業を保存してください。")
            AddString("RestartNoticeBtn", "รีสตาร์ทเดี๋ยวนี้", "Restart Now", "今すぐ再起動")
            AddString("RestartNoticeMinimizeWarn", "หน้าต่างจะกลับมาแสดงใน 20 วินาที", "This window will reappear in 20 seconds", "このウィンドウは20秒後に再表示されます")

            ' ── Language ──
            AddString("LangTH", "TH", "TH", "TH")
            AddString("LangEN", "EN", "EN", "EN")
            AddString("LangJP", "JP", "JP", "JP")
        End Sub
    End Class
End Namespace
