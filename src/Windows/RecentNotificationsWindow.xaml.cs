using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NASFileWatcher.Core;
using NASFileWatcher.Models;

namespace NASFileWatcher.Windows
{
    public partial class RecentNotificationsWindow : Window
    {
        private List<FileChangeNotification> _notifications;
        private FileWatcherService _watcherService;
        public bool IsClosed { get; private set; } = false;

        public RecentNotificationsWindow(FileWatcherService watcherService)
        {
            InitializeComponent();
            _watcherService = watcherService;

            // 訂閱通知更新事件
            _watcherService.NotificationsUpdated += OnNotificationsUpdated;

            LoadNotifications();
            Closed += (s, e) =>
            {
                IsClosed = true;
                // 取消訂閱事件
                _watcherService.NotificationsUpdated -= OnNotificationsUpdated;
            };
        }

        private void OnNotificationsUpdated(object sender, EventArgs e)
        {
            // 在 UI 執行緒上更新
            Dispatcher.Invoke(() => LoadNotifications());
        }

        private void LoadNotifications()
        {
            // 從 FileWatcherService 取得最新資料
            _notifications = _watcherService?.GetRecentNotifications() ?? new List<FileChangeNotification>();

            if (_notifications == null || _notifications.Count == 0)
            {
                StatusTextBlock.Text = "目前沒有任何通知記錄";
                NotificationsDataGrid.ItemsSource = null;
                return;
            }

            // 轉換成顯示用的格式
            var displayItems = _notifications
                .OrderByDescending(n => n.Timestamp)
                .Select(n => new NotificationDisplayItem
                {
                    TimeDisplay = n.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"),
                    EventTypeDisplay = GetEventTypeDisplay(n.EventType),
                    FileName = n.FileName,
                    RelativePath = n.RelativePath,
                    OldName = n.OldName
                })
                .ToList();

            NotificationsDataGrid.ItemsSource = displayItems;
            StatusTextBlock.Text = $"共 {displayItems.Count} 筆記錄";
        }

        private string GetEventTypeDisplay(string eventType)
        {
            return eventType switch
            {
                "created" => "📁 新增",
                "modified" => "✏️ 修改",
                "deleted" => "🗑️ 刪除",
                "renamed" => "📝 重新命名",
                "batch" => "📦 批次變動",
                "test" => "🧪 測試",
                _ => eventType
            };
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 重新載入通知 (從 MainWindow 取得最新資料)
            LoadNotifications();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
