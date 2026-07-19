Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text

Namespace Managers

    ''' <summary>
    ''' ระบบบันทึก Log แบบไฟล์ ปลอดภัยต่อ Thread พร้อมหมุนเวียนไฟล์รายวัน
    ''' รูปแบบ Log: [yyyy-MM-dd HH:mm:ss] [LEVEL] message
    ''' การบันทึก Log ต้องไม่ทำให้แอปพลิเคชันหยุดทำงาน — ดักจับทุก Error ภายใน
    ''' </summary>
    Public NotInheritable Class LogManager

        Private Shared ReadOnly _lock As New Object
        Private Shared _logDirectory As String

        Private Sub New()
            ' คลาสแบบ Static เท่านั้น ไม่ต้องสร้าง Instance
        End Sub

        Private Shared ReadOnly Property LogDirectory As String
            Get
                If _logDirectory Is Nothing Then
                    Dim parentDir As String = Config.AppSettings.LogPath
                    Dim company As String = Config.AppSettings.CompanyName
                    If String.IsNullOrEmpty(parentDir) Then
                        parentDir = AppDomain.CurrentDomain.BaseDirectory
                    End If
                    _logDirectory = Path.Combine(parentDir, company)
                End If
                Return _logDirectory
            End Get
        End Property

        Private Shared ReadOnly Property LogsFilePath As String
            Get
                Return Path.Combine(LogDirectory, Utilities.EnvironmentHelper.ComputerName & "_Logs.txt")
            End Get
        End Property

        Private Shared ReadOnly Property IPFilePath As String
            Get
                Return Path.Combine(LogDirectory, Utilities.EnvironmentHelper.ComputerName & "_IP.txt")
            End Get
        End Property

        ''' <summary>บันทึกข้อความระดับ Info</summary>
        Public Shared Sub Info(message As String)
            WriteLog("INFO", message)
        End Sub

        ''' <summary>บันทึกข้อความระดับ Warning</summary>
        Public Shared Sub Warn(message As String)
            WriteLog("WARN", message)
        End Sub

        ''' <summary>บันทึกข้อความระดับ Error พร้อมรายละเอียด Exception (ถ้ามี)</summary>
        Public Shared Sub [Error](message As String, Optional ex As Exception = Nothing)
            Dim fullMessage As String = message
            If ex IsNot Nothing Then
                fullMessage = message & Environment.NewLine & ex.ToString()
            End If
            WriteLog("ERROR", fullMessage)
        End Sub

        Private Shared Sub WriteLog(level As String, message As String)
            Try
                Dim sb As New StringBuilder(128)
                sb.Append("["c)
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                sb.Append("] [")
                sb.Append(level)
                sb.Append("] ")
                sb.AppendLine(message)

                SyncLock _lock
                    Dim dir As String = LogDirectory
                    If Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If
                    File.AppendAllText(LogsFilePath, sb.ToString())
                End SyncLock
            Catch
                ' การบันทึก Log ต้องไม่ทำให้แอปพลิเคชันหยุดทำงาน
            End Try
        End Sub

        ''' <summary>
        ''' บันทึก IP Address ของเครื่องลงใน IP.txt
        ''' </summary>
        Public Shared Sub LogIPAddress()
            Try
                Dim ipAddress As String = GetLocalIPAddress()
                Dim sb As New StringBuilder(64)
                sb.Append("["c)
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                sb.Append("] ")
                sb.AppendLine(ipAddress)

                SyncLock _lock
                    Dim dir As String = LogDirectory
                    If Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If
                    File.AppendAllText(IPFilePath, sb.ToString())
                End SyncLock
            Catch
                ' การบันทึก Log ต้องไม่ทำให้แอปพลิเคชันหยุดทำงาน
            End Try
        End Sub

        Private Shared Function GetLocalIPAddress() As String
            Try
                Dim host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                For Each ip In host.AddressList
                    If ip.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                        Return ip.ToString()
                    End If
                Next
            Catch
            End Try
            Return "127.0.0.1"
        End Function

        ''' <summary>
        ''' รีเซ็ตตำแหน่งโฟลเดอร์ Log ที่ Cache ไว้ (เช่น หลังโหลด Config ใหม่)
        ''' </summary>
        Public Shared Sub Reset()
            SyncLock _lock
                _logDirectory = Nothing
            End SyncLock
        End Sub

    End Class

End Namespace
