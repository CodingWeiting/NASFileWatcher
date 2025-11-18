using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NASFileWatcher.Models;

namespace NASFileWatcher.Core
{
    public class FileWatcherService
    {
        private FileSystemWatcher _watcher;
        private AppConfig _config;
        private readonly ConcurrentDictionary<string, FileChangeNotification> _pendingChanges = new();
        private readonly ConcurrentQueue<FileChangeNotification> _recentNotifications = new();
        private Timer _debounceTimer;
        private bool _isPaused = false;
        private bool _isRunning = false;
        private DateTime? _batchWindowStart = null;
        private readonly object _batchLock = new object();

        // 備份檔案正則表達式: 檔名.數字.rfa
        private static readonly Regex BackupFilePattern = new Regex(@"\.\d{4}\.rfa$", RegexOptions.IgnoreCase);

        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler NotificationsUpdated;

        public FileWatcherService()
        {
            _config = AppConfig.Load();

            // 啟動清理舊日誌
            Task.Run(() => Logger.CleanOldLogs());
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _config = AppConfig.Load();

            if (!_config.Validate())
            {
                throw new Exception("設定檔驗證失敗,請檢查設定");
            }

            if (!Directory.Exists(_config.NasPath))
            {
                throw new DirectoryNotFoundException($"找不到 NAS 路徑: {_config.NasPath}");
            }

            try
            {
                _watcher = new FileSystemWatcher(_config.NasPath)
                {
                    Filter = "*.rfa",
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
                };

                _watcher.Created += OnFileChanged;
                _watcher.Changed += OnFileChanged;
                _watcher.Deleted += OnFileChanged;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Error += OnError;

                _watcher.EnableRaisingEvents = true;
                _isRunning = true;

                // 啟動防抖動計時器
                _debounceTimer = new Timer(ProcessPendingChanges, null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1));

                Logger.Log($"開始監控: {_config.NasPath}");
                StatusChanged?.Invoke(this, new StatusChangedEventArgs { IsHealthy = true, Message = "監控已啟動" });
            }
            catch (Exception ex)
            {
                Logger.Log($"啟動監控失敗: {ex.Message}", LogLevel.Error);
                StatusChanged?.Invoke(this, new StatusChangedEventArgs { IsHealthy = false, Message = ex.Message });
                throw;
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _debounceTimer?.Dispose();

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            _isRunning = false;
            Logger.Log("監控已停止");
        }

        public void Pause()
        {
            _isPaused = true;
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
            }
            Logger.Log("監控已暫停");
        }

        public void Resume()
        {
            _isPaused = false;
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = true;
            }
            Logger.Log("監控已繼續");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_isPaused || IsBackupFile(e.FullPath))
                return;

            // 判斷事件類型
            string eventType = e.ChangeType switch
            {
                WatcherChangeTypes.Created => "created",
                WatcherChangeTypes.Changed => "modified",
                WatcherChangeTypes.Deleted => "deleted",
                _ => "modified"
            };

            var notification = new FileChangeNotification
            {
                EventType = eventType,
                FileName = Path.GetFileName(e.FullPath),
                FilePath = e.FullPath,
                RelativePath = GetRelativePath(e.FullPath),
                Timestamp = DateTime.Now
            };

            // 如果是 deleted,應該覆蓋任何現有事件
            if (eventType == "deleted")
            {
                Logger.Log($"檔案刪除: {e.Name}");
                _pendingChanges[e.FullPath] = notification;
                return;
            }

            // 如果已經有 Created 事件,不要被 Changed 覆蓋
            if (_pendingChanges.ContainsKey(e.FullPath))
            {
                var existing = _pendingChanges[e.FullPath];

                // 如果已經是 Created,保留 Created
                if (existing.EventType == "created")
                {
                    Logger.Log($"保留 Created 事件,忽略 Changed: {e.Name}");
                    return; // 忽略後續的 Changed 事件
                }

                // 否則更新為最新事件
                _pendingChanges[e.FullPath] = notification;
            }
            else
            {
                // 第一次加入
                Logger.Log($"新增 {eventType} 事件: {e.Name}");
                _pendingChanges[e.FullPath] = notification;
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_isPaused || IsBackupFile(e.FullPath))
                return;

            // 清除舊路徑的 pending (因為檔案已重新命名)
            _pendingChanges.TryRemove(e.OldFullPath, out _);
            _pendingChanges.TryRemove(e.FullPath, out _);

            // 重新命名立即處理
            var notification = new FileChangeNotification
            {
                EventType = "renamed",
                FileName = Path.GetFileName(e.FullPath),
                FilePath = e.FullPath,
                RelativePath = GetRelativePath(e.FullPath),
                Timestamp = DateTime.Now,
                OldName = Path.GetFileName(e.OldFullPath)
            };

            Task.Run(() => SendNotification(notification));
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            Logger.Log($"FileSystemWatcher 錯誤: {ex?.Message}", LogLevel.Error);
            StatusChanged?.Invoke(this, new StatusChangedEventArgs
            {
                IsHealthy = false,
                Message = "監控發生錯誤,可能是網路連線中斷"
            });
        }

        private void ProcessPendingChanges(object state)
        {
            if (_isPaused)
                return;

            lock (_batchLock)
            {
                var now = DateTime.Now;
                var itemsToProcess = _pendingChanges
                    .Where(kvp => (now - kvp.Value.Timestamp).TotalSeconds >= _config.DebounceSeconds)
                    .ToList();

                if (itemsToProcess.Count == 0)
                    return;

                // 檢查是否啟用批次通知
                if (_config.EnableBatchNotification)
                {
                    // 如果是第一個檔案,開始批次窗口
                    if (_batchWindowStart == null)
                    {
                        _batchWindowStart = now;
                    }

                    // 計算批次窗口已經過了多久
                    var windowElapsed = (now - _batchWindowStart.Value).TotalSeconds;

                    // 如果窗口時間還沒到,繼續收集
                    if (windowElapsed < _config.BatchWindowSeconds)
                    {
                        return; // 繼續收集,不管數量多少
                    }

                    // 時間窗口已結束,處理所有收集到的變動
                    foreach (var item in itemsToProcess)
                    {
                        _pendingChanges.TryRemove(item.Key, out _);
                    }

                    // 重置批次窗口
                    _batchWindowStart = null;

                    // 判斷是否達到批次閾值
                    if (itemsToProcess.Count >= _config.BatchThreshold)
                    {
                        // 數量達標,批次處理
                        Logger.Log($"批次處理 {itemsToProcess.Count} 個檔案變動");
                        ProcessBatchChanges(itemsToProcess);
                    }
                    else
                    {
                        // 數量未達標,逐一處理
                        Logger.Log($"逐一處理 {itemsToProcess.Count} 個檔案變動");
                        foreach (var item in itemsToProcess)
                        {
                            ProcessFileChange(item.Value);
                        }
                    }
                }
                else
                {
                    // 未啟用批次通知,逐一處理
                    foreach (var item in itemsToProcess)
                    {
                        _pendingChanges.TryRemove(item.Key, out _);
                    }

                    foreach (var item in itemsToProcess)
                    {
                        ProcessFileChange(item.Value);
                    }
                }
            }
        }

        private void ProcessBatchChanges(List<KeyValuePair<string, FileChangeNotification>> changes)
        {
            try
            {
                var notifications = new List<FileChangeNotification>();

                foreach (var item in changes)
                {
                    var notification = item.Value;

                    // 再次確認檔案狀態 (以防在等待期間被刪除)
                    if (notification.EventType != "deleted" && !File.Exists(notification.FilePath))
                    {
                        notification.EventType = "deleted";
                    }

                    notifications.Add(notification);
                }

                // 發送批次通知
                Task.Run(() => SendBatchNotification(notifications));
            }
            catch (Exception ex)
            {
                Logger.Log($"處理批次變動失敗: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task SendBatchNotification(List<FileChangeNotification> notifications)
        {
            try
            {
                var grouped = notifications.GroupBy(n => n.EventType).ToDictionary(g => g.Key, g => g.ToList());

                var batchNotification = new BatchFileChangeNotification
                {
                    TotalCount = notifications.Count,
                    CreatedCount = grouped.ContainsKey("created") ? grouped["created"].Count : 0,
                    ModifiedCount = grouped.ContainsKey("modified") ? grouped["modified"].Count : 0,
                    DeletedCount = grouped.ContainsKey("deleted") ? grouped["deleted"].Count : 0,
                    StartTime = notifications.Min(n => n.Timestamp),
                    EndTime = notifications.Max(n => n.Timestamp),
                    Files = notifications.Take(20).Select(n => new BatchFileInfo
                    {
                        FileName = n.FileName,
                        RelativePath = n.RelativePath,
                        EventType = n.EventType
                    }).ToList(),
                    CommonPath = GetCommonPath(notifications.Select(n => n.RelativePath).ToList()),
                    FolderStructure = GetFolderStructure(notifications)
                };

                // 加入最近通知清單 (只記錄一筆批次通知)
                var summaryNotification = new FileChangeNotification
                {
                    EventType = "batch",
                    FileName = $"{notifications.Count} 個檔案",
                    FilePath = batchNotification.CommonPath,
                    RelativePath = batchNotification.CommonPath,
                    Timestamp = DateTime.Now
                };
                _recentNotifications.Enqueue(summaryNotification);
                while (_recentNotifications.Count > 20)
                {
                    _recentNotifications.TryDequeue(out _);
                }

                // 觸發通知更新事件
                NotificationsUpdated?.Invoke(this, EventArgs.Empty);

                // 發送到 Webhook
                var webhookSender = new WebhookSender(_config.WebhookUrl);
                bool success = await webhookSender.SendBatchAsync(batchNotification);

                if (success)
                {
                    Logger.Log($"批次通知已發送: {notifications.Count} 個檔案變動");
                }
                else
                {
                    Logger.Log($"批次通知發送失敗", LogLevel.Warning);
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs
                    {
                        IsHealthy = false,
                        Message = "Webhook 發送失敗"
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"發送批次通知時發生錯誤: {ex.Message}", LogLevel.Error);
            }
        }

        private List<FolderStructureInfo> GetFolderStructure(List<FileChangeNotification> notifications)
        {
            // 依資料夾分組
            var folderGroups = notifications
                .GroupBy(n => Path.GetDirectoryName(n.RelativePath) ?? "根目錄")
                .Select(g => new
                {
                    FolderPath = g.Key,
                    Files = g.ToList(),
                    Created = g.Count(f => f.EventType == "created"),
                    Modified = g.Count(f => f.EventType == "modified"),
                    Deleted = g.Count(f => f.EventType == "deleted")
                })
                .OrderByDescending(g => g.Files.Count)
                .ToList();

            var result = new List<FolderStructureInfo>();

            foreach (var group in folderGroups)
            {
                result.Add(new FolderStructureInfo
                {
                    FolderPath = group.FolderPath,
                    TotalFiles = group.Files.Count,
                    CreatedCount = group.Created,
                    ModifiedCount = group.Modified,
                    DeletedCount = group.Deleted,
                    Depth = group.FolderPath.Split('\\').Length
                });
            }

            return result;
        }

        private string GetCommonPath(List<string> paths)
        {
            if (paths.Count == 0)
                return "";

            if (paths.Count == 1)
                return Path.GetDirectoryName(paths[0]);

            // 找出共同的父資料夾
            var splitPaths = paths.Select(p => p.Split('\\')).ToList();
            var minDepth = splitPaths.Min(p => p.Length);

            for (int i = 0; i < minDepth; i++)
            {
                var firstPart = splitPaths[0][i];
                if (splitPaths.All(p => p[i] == firstPart))
                {
                    continue;
                }
                else
                {
                    // 找到第一個不同的部分
                    if (i == 0)
                        return "多個資料夾";
                    return string.Join("\\", splitPaths[0].Take(i));
                }
            }

            return string.Join("\\", splitPaths[0].Take(minDepth - 1));
        }

        private void ProcessFileChange(FileChangeNotification notification)
        {
            try
            {
                // 再次確認檔案狀態 (以防在等待期間被刪除)
                if (notification.EventType != "deleted" && !File.Exists(notification.FilePath))
                {
                    notification.EventType = "deleted";
                }

                Task.Run(() => SendNotification(notification));
            }
            catch (Exception ex)
            {
                Logger.Log($"處理檔案變動失敗 [{notification.FilePath}]: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task SendNotification(FileChangeNotification notification)
        {
            try
            {
                // 加入最近通知清單
                _recentNotifications.Enqueue(notification);
                while (_recentNotifications.Count > 20)
                {
                    _recentNotifications.TryDequeue(out _);
                }

                // 觸發通知更新事件
                NotificationsUpdated?.Invoke(this, EventArgs.Empty);

                // 發送到 n8n Webhook
                var webhookSender = new WebhookSender(_config.WebhookUrl);
                bool success = await webhookSender.SendAsync(notification);

                if (success)
                {
                    Logger.Log($"通知已發送: {notification.EventType} - {notification.RelativePath}");
                }
                else
                {
                    Logger.Log($"通知發送失敗: {notification.RelativePath}", LogLevel.Warning);
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs
                    {
                        IsHealthy = false,
                        Message = "Webhook 發送失敗"
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"發送通知時發生錯誤: {ex.Message}", LogLevel.Error);
            }
        }

        private bool IsBackupFile(string filePath)
        {
            return BackupFilePattern.IsMatch(filePath);
        }

        private string GetRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(_config.NasPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(_config.NasPath.Length).TrimStart('\\');
            }
            return fullPath;
        }

        public List<FileChangeNotification> GetRecentNotifications()
        {
            return _recentNotifications.ToList();
        }
    }
}
