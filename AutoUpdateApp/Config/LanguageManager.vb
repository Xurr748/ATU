Option Strict On
Option Explicit On

Namespace Config
    Public Class LanguageManager
        Private Shared _currentLang As String = "en"
        Private Shared _strings As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)

        Shared Sub New()
            InitStrings()
        End Sub

        Public Shared Property CurrentLanguage As String
            Get
                Return _currentLang
            End Get
            Set(value As String)
                If value IsNot Nothing AndAlso (value.Equals("en", StringComparison.OrdinalIgnoreCase) OrElse
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
                If langDict.ContainsKey("en") Then
                    Return langDict("en")
                End If
            End If
            Return key
        End Function

        Private Shared Sub AddString(key As String, en As String, jp As String)
            Dim d As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            d("en") = en
            d("jp") = jp
            _strings(key) = d
        End Sub

        Private Shared Sub InitStrings()
            AddString("AppTitle", "Auto Update", "自動更新")
            AddString("InfoTitle", "Test Machine Info", "テストマシン情報")
            AddString("ComputerName", "Computer:", "マシン名:")
            AddString("Type", "Type:", "タイプ:")
            AddString("Mode", "Mode:", "モード:")
            AddString("ScheduleTime", "Check Time:", "チェック時間:")
            AddString("NotFoundInConfig", "(Not found in Config)", "(設定に見つかりません)")
            AddString("VersionTitle", "Software Status", "ソフトウェア状況")
            AddString("CurrentVersion", "Current Version:", "現在のバージョン:")
            AddString("ServerVersion", "Server Version:", "サーバーバージョン:")
            AddString("Status", "Status:", "ステータス:")
            AddString("VersionNotFound", "(Not found)", "(見つかりません)")
            AddString("VersionReadError", "(Cannot read)", "(読み取れません)")
            AddString("StatusPendingRestart", "● Pending restart for update", "● 更新のため再起動待ち")
            AddString("StatusNotInstalled", "● Program not installed", "● プログラム未インストール")
            AddString("StatusServerError", "● Cannot read server version", "● サーバーバージョン読み取り不可")
            AddString("StatusUpToDate", "● Up to date", "● 最新です")
            AddString("StatusUpdateAvailable", "● Update available", "● 更新あり")
            AddString("BtnCheck", "Check", "確認")
            AddString("BtnRefresh", "Refresh", "更新")
            AddString("BtnExit", "Exit", "終了")
            AddString("BtnUpdateNow", "Update Now", "今すぐ更新")
            AddString("BtnDetails", "Details", "詳細")
            AddString("BtnDebugConfig", "[Debug] View loaded Config", "[Debug] ロード済み設定")
            AddString("MenuCheckNow", "Check for updates now", "今すぐ更新を確認")
            AddString("MenuExit", "Exit", "終了")
            AddString("ConfirmUpdate", "Do you want to update the application now?", "今すぐアプリケーションを更新しますか?")
            AddString("ConfirmTitle", "Confirm", "確認")
            AddString("MachineNotInSystem", "This machine is not in the system (TesterType.csv)", "このマシンはシステムにありません (TesterType.csv)")
            AddString("Updating", "Updating...", "更新中...")
            AddString("ProgressDownloading", "Preparing download...", "ダウンロード準備中...")
            AddString("ProgressUninstalling", "Uninstalling...", "アンインストール中...")
            AddString("ProgressInstalling", "Installing...", "インストール中...")
            AddString("ProgressComplete", "Update completed", "更新完了")
            AddString("ProgressFailed", "Download failed", "ダウンロード失敗")
            AddString("ProgressSearching", "Searching for installed program...", "インストール済みプログラムを検索中...")
            AddString("ProgressUninstallingProduct", "Uninstalling {0}...", "{0}をアンインストール中...")
            AddString("ProgressInstallingProduct", "Installing {0}...", "{0}をインストール中...")
            AddString("ProgressDownloadingFile", "Downloading: {0} ({1}/{2} files)", "ダウンロード中: {0} ({1}/{2} ファイル)")
            AddString("RestartPromptMsg", "System has been waiting for update for over 1 hour." & Environment.NewLine & "Please restart to update the system." & Environment.NewLine & Environment.NewLine & "Restart now?", "システムは1時間以上更新を待っています。" & Environment.NewLine & "システムを更新するために再起動してください。" & Environment.NewLine & Environment.NewLine & "今すぐ再起動しますか?")
            AddString("RestartPromptTitle", "Update Notice", "更新通知")
            AddString("PromptTitle", "Update Alert", "アップデート通知")
            AddString("PromptNewVersion", "New version found and ready to update!", "新しいバージョンが見つかりました！")
            AddString("PromptCurrent", "Current", "現在")
            AddString("PromptLatest", "Latest", "最新")
            AddString("PromptUpdateNow", "Update Now", "今すぐ更新")
            AddString("PromptAfterRestart", "After Restart", "再起動後")
            AddString("PromptRemindLater", "Remind Later", "後で通知")
            AddString("PromptSuccess", "Update completed successfully", "更新が正常に完了しました")
            AddString("PromptFailed", "Update failed. Please check the log.", "更新に失敗しました。ログを確認してください")
            AddString("TitleSuccess", "Success", "成功")
            AddString("CantRestart", "Unable to restart: ", "再起動できません: ")
            AddString("TitleError", "Error", "エラー")
            AddString("PromptFileNotFound", "File not found: ", "ファイルが見つかりません: ")
            AddString("PromptFileNotFoundTitle", "File Not Found", "ファイル未検出")
            AddString("PromptCantOpenFile", "Unable to open file: ", "ファイルを開けません: ")
            AddString("PromptPathNotConfigured", "Path {0} is not configured in config.txt", "config.txt で {0} パスが設定されていません")
            AddString("PromptPathNotFoundTitle", "Path Not Found", "パス未検出")
            AddString("PromptNoPdfInFolder", "No PDF files found in folder: ", "フォルダー内にPDFファイルが見つかりません: ")
            AddString("PromptChecking", "Checking...", "確認中...")
            AddString("PromptCheckingUpdate", "Checking for updates...", "アップデート確認中...")
            AddString("PromptAlreadyChecking", "Checking is already in progress, please wait.", "既に確認中のため、しばらくお待ちください。")
            AddString("PromptNotice", "Notice", "通知")
            AddString("PromptCheckDone", "Check completed", "確認完了")
            AddString("TitleCheckResult", "Check Result", "確認結果")
            AddString("PromptSuccessCompleted", "Update completed successfully!", "アップデートが正常に完了しました！")
            AddString("MsgNotInConfig", "This machine is not in the system (TesterType.csv)", "このマシンはシステムにありません (TesterType.csv)")
            AddString("MsgHourNotMatching", "Not the scheduled hour to check for updates.", "アップデート確認のスケジュール時間外です。")
            AddString("MsgAlreadyCheckedToday", "Already checked today.", "本日既にアップデート確認を行いました。")
            AddString("MsgUpToDate", "Program is up to date.", "プログラムは最新バージョンです。")
            AddString("MsgPendingRestart", "Pending restart update is already scheduled.", "再起動待ちのアップデートがスケジュールされています。")
            AddString("MsgInstallerNotFound", "Installer files not found on server.", "サーバー上にアップデートファイルが見つかりません。")
            AddString("MsgCancelled", "Check cancelled.", "確認がキャンセルされました。")
            AddString("DebugTitle", "Config Debug", "設定デバッグ")
            AddString("DebugExeLocation", "══════ EXE Location ══════", "══════ EXEの場所 ══════")
            AddString("DebugConfigStatus", "══════ Config Status ══════", "══════ 設定ファイルのステータス ══════")
            AddString("DebugReadValues", "══════ Read Values ══════", "══════ 読み取られた値 ══════")
            AddString("RestartNoticeTitle", "Restart Required", "再起動が必要です")
            AddString("RestartNoticeHeader", "Please Restart Your Computer", "コンピュータを再起動してください")
            AddString("RestartNoticeBody", "Update has been scheduled successfully." & Environment.NewLine & "Please restart your computer to install the update." & Environment.NewLine & Environment.NewLine & "Note: Please save all your work before restarting.", "アップデートが正常にスケジュールされました。" & Environment.NewLine & "アップデートをインストールするために再起動してください。" & Environment.NewLine & Environment.NewLine & "注意: 再起動前にすべての作業を保存してください。")
            AddString("RestartNoticeBtn", "Restart Now", "今すぐ再起動")
            AddString("RestartNoticeBtnCancel", "Cancel", "キャンセル")
            AddString("RestartNoticeCountdown", "Auto restart in {0} seconds", "{0}秒後に自動再起動します")
            AddString("RestartNoticeMinimizeWarn", "This window will reappear in 20 seconds", "このウィンドウは20秒後に再表示されます")
            AddString("LangEN", "EN", "EN")
            AddString("LangJP", "JP", "JP")
        End Sub
    End Class
End Namespace
