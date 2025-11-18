using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NASFileWatcher.Models;

namespace NASFileWatcher.Core
{
    public class WebhookSender
    {
        private readonly string _webhookUrl;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public WebhookSender(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public async Task<bool> SendAsync(FileChangeNotification notification)
        {
            try
            {
                var payload = new
                {
                    event_type = notification.EventType,
                    file_name = notification.FileName,
                    file_path = notification.FilePath,
                    relative_path = notification.RelativePath,
                    timestamp = notification.Timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    old_name = notification.OldName
                };

                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Webhook 回應錯誤: {response.StatusCode}\nURL: {_webhookUrl}\n回應內容: {responseBody}", LogLevel.Warning);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log($"Webhook 網路錯誤: {ex.Message}\nURL: {_webhookUrl}", LogLevel.Error);
                return false;
            }
            catch (TaskCanceledException)
            {
                Logger.Log($"Webhook 請求逾時\nURL: {_webhookUrl}", LogLevel.Warning);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Webhook 發送失敗: {ex.Message}\nURL: {_webhookUrl}", LogLevel.Error);
                return false;
            }
        }

        public async Task<bool> SendBatchAsync(BatchFileChangeNotification batchNotification)
        {
            try
            {
                var payload = new
                {
                    event_type = "batch",
                    total_count = batchNotification.TotalCount,
                    created_count = batchNotification.CreatedCount,
                    modified_count = batchNotification.ModifiedCount,
                    deleted_count = batchNotification.DeletedCount,
                    start_time = batchNotification.StartTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    end_time = batchNotification.EndTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    common_path = batchNotification.CommonPath,
                    files = batchNotification.Files,
                    folder_structure = batchNotification.FolderStructure
                };

                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Logger.Log($"批次 Webhook 回應錯誤: {response.StatusCode}\nURL: {_webhookUrl}\n回應內容: {responseBody}", LogLevel.Warning);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log($"批次 Webhook 網路錯誤: {ex.Message}\nURL: {_webhookUrl}", LogLevel.Error);
                return false;
            }
            catch (TaskCanceledException)
            {
                Logger.Log($"批次 Webhook 請求逾時\nURL: {_webhookUrl}", LogLevel.Warning);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"批次 Webhook 發送失敗: {ex.Message}\nURL: {_webhookUrl}", LogLevel.Error);
                return false;
            }
        }
    }
}
