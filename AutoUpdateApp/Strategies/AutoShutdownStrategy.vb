Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text.RegularExpressions

Namespace Strategies

    Public Class AutoShutdownStrategy
        Implements IUpdateStrategy

        Public Const CheckIntervalSeconds As Integer = 2

        Private _invokeControl As System.Windows.Forms.Control

        Public Sub New(Optional invokeControl As System.Windows.Forms.Control = Nothing)
            _invokeControl = invokeControl
        End Sub

        Public Function Execute(context As Models.UpdateContext) As UpdateResult Implements IUpdateStrategy.Execute
            ' In AUTOSHUTDOWN mode, periodic checking of the stop condition is handled
            ' independently by MainForm via AutoModeTimer every CheckIntervalSeconds.
            Return UpdateResult.NoAction
        End Function

        Public Shared Function CheckAutoShutdownCondition(computerName As String) As Boolean
            Try
                Dim tester As Models.TesterInfo = Managers.ConfigManager.GetTesterByName(computerName)
                If tester Is Nothing OrElse Not String.Equals(tester.Mode, "AUTOSHUTDOWN", StringComparison.OrdinalIgnoreCase) Then
                    Return False
                End If

                Dim watchFolder As String = Config.AppSettings.AutoWatchFolderPath
                Dim stopLogFolder As String = Config.AppSettings.AutoStopLogFolderPath

                If String.IsNullOrEmpty(watchFolder) OrElse String.IsNullOrEmpty(stopLogFolder) Then
                    Return False
                End If

                If Not File.Exists(watchFolder) AndAlso Not Directory.Exists(watchFolder) Then Return False
                If Not File.Exists(stopLogFolder) AndAlso Not Directory.Exists(stopLogFolder) Then Return False

                Dim watchPath As String = AutoStrategy.FindLatestFile(watchFolder, computerName)
                Dim stopLogPath As String = AutoStrategy.FindLatestFile(stopLogFolder, computerName)

                If String.IsNullOrEmpty(watchPath) OrElse String.IsNullOrEmpty(stopLogPath) Then
                    Return False
                End If

                Dim endOfTestTime As DateTime = AutoStrategy.ReadEndOfTestTime(watchPath)
                Dim stopTime As DateTime = AutoStrategy.ReadLatestStopTime(stopLogPath)

                If endOfTestTime = DateTime.MinValue OrElse stopTime = DateTime.MinValue Then
                    Return False
                End If

                ' Prevent using an old endOfTestTime from multiple days ago against a recent stop
                If stopTime > endOfTestTime AndAlso (stopTime - endOfTestTime).TotalHours > 24 Then
                    Return False
                End If

                Return (endOfTestTime < stopTime)
            Catch ex As Exception
                Managers.LogManager.Error("AutoShutdown mode: CheckAutoShutdownCondition error: " & ex.Message, ex)
                Return False
            End Try
        End Function

    End Class

End Namespace
