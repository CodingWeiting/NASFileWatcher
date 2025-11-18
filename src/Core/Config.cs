using System;
using System.IO;
using System.Text.Json;

namespace NASFileWatcher.Core
{
    public class AppConfig
    {
        public string NasPath { get; set; } = @"\\192.168.1.100\\FamilyLibrary";
        public string WebhookUrl { get; set; } = "https://your-n8n.com/webhook/file-change";
        public int DebounceSeconds { get; set; } = 10;
        public bool EnableLogging { get; set; } = true;
        public bool EnableBatchNotification { get; set; } = true;
        public int BatchThreshold { get; set; } = 10; // 超過此數量啟用批次通知
        public int BatchWindowSeconds { get; set; } = 30; // 多少秒內的變動視為批次
        
        // 使用使用者資料夾存放設定檔,避免權限問題
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NASFileWatcher",
            "config.json"
        );

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath, System.Text.Encoding.UTF8);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"載入設定檔失敗: {ex.Message}", LogLevel.Error);
            }

            // 如果載入失敗,建立預設設定檔
            var defaultConfig = new AppConfig();
            defaultConfig.Save();
            return defaultConfig;
        }

        public void Save()
        {
            try
            {
                // 確保目錄存在
                string directory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Log($"儲存設定檔失敗: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(NasPath))
                return false;
            
            if (string.IsNullOrWhiteSpace(WebhookUrl))
                return false;
            
            if (!Uri.TryCreate(WebhookUrl, UriKind.Absolute, out _))
                return false;
            
            if (DebounceSeconds < 0 || DebounceSeconds > 60)
                return false;
            
            if (BatchThreshold < 2 || BatchThreshold > 1000)
                return false;
            
            if (BatchWindowSeconds < 5 || BatchWindowSeconds > 300)
                return false;
            
            return true;
        }
    }
}
