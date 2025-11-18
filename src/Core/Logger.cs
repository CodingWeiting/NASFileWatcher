using System;
using System.IO;

namespace NASFileWatcher.Core
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        // 使用使用者資料夾存放日誌,避免權限問題
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NASFileWatcher",
            "Logs"
        );
        
        private static readonly object _lock = new object();

        static Logger()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            var config = AppConfig.Load();
            if (!config.EnableLogging && level == LogLevel.Info)
                return;

            lock (_lock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string logMessage = $"[{timestamp}] [{level}] {message}";
                    
                    // 寫入今天的日誌檔
                    string logFile = Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
                    File.AppendAllText(logFile, logMessage + Environment.NewLine);
                    
                    // 同時輸出到除錯視窗
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
                catch
                {
                    // 避免日誌記錄本身造成錯誤
                }
            }
        }

        public static void CleanOldLogs(int keepDays = 30)
        {
            try
            {
                var files = Directory.GetFiles(LogDirectory, "*.log");
                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"清理舊日誌失敗: {ex.Message}", LogLevel.Warning);
            }
        }
    }
}
