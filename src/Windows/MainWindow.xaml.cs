using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using NASFileWatcher.Core;
using NASFileWatcher.Models;

namespace NASFileWatcher.Windows
{
    public partial class MainWindow : Window
    {
        private FileWatcherService _watcherService;
        private bool _isPaused = false;
        private RecentNotificationsWindow _notificationsWindow;
        private SettingsWindow _settingsWindow;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化監控服務
            _watcherService = new FileWatcherService();
            _watcherService.StatusChanged += OnStatusChanged;

            // 檢查是否為第一次執行
            if (IsFirstRun())
            {
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "請先進行設定", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                // 延遲開啟設定視窗,確保托盤圖示已載入
                Dispatcher.BeginInvoke(new Action(() => Settings_Click(null, null)), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            else
            {
                // 啟動監控
                StartWatching();
            }
        }

        private bool IsFirstRun()
        {
            string configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NASFileWatcher",
                "config.json"
            );
            return !File.Exists(configPath);
        }

        private void StartWatching()
        {
            try
            {
                _watcherService.Start();
                UpdateTrayIcon(true);
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "監控已啟動", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"啟動監控失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateTrayIcon(false);
            }
        }

        private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateTrayIcon(e.IsHealthy);
                if (!e.IsHealthy)
                {
                    TrayIcon.ShowBalloonTip("NAS 檔案監控", e.Message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                }
            });
        }

        private void UpdateTrayIcon(bool isHealthy)
        {
            if (_isPaused)
            {
                TrayIcon.ToolTipText = "NAS 檔案監控 - 已暫停";
                // 可以換成黃色圖示
            }
            else if (isHealthy)
            {
                TrayIcon.ToolTipText = "NAS 檔案監控 - 運行中";
                // 綠色圖示
            }
            else
            {
                TrayIcon.ToolTipText = "NAS 檔案監控 - 連線異常";
                // 紅色圖示
            }
        }

        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            ViewRecentNotifications_Click(sender, e);
        }

        private void ViewRecentNotifications_Click(object sender, RoutedEventArgs e)
        {
            // 如果視窗已經開啟,則啟動它並帶到前景
            if (_notificationsWindow != null && !_notificationsWindow.IsClosed)
            {
                _notificationsWindow.Activate();
                _notificationsWindow.WindowState = WindowState.Normal;
                return;
            }

            // 建立新視窗
            _notificationsWindow = new RecentNotificationsWindow(_watcherService);
            _notificationsWindow.Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // 如果視窗已經開啟,則啟動它並帶到前景
            if (_settingsWindow != null && !_settingsWindow.IsClosed)
            {
                _settingsWindow.Activate();
                _settingsWindow.WindowState = WindowState.Normal;
                return;
            }

            // 建立新視窗
            _settingsWindow = new SettingsWindow();
            if (_settingsWindow.ShowDialog() == true)
            {
                // 重新啟動監控服務
                _watcherService.Stop();
                _watcherService.Start();
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "設定已更新,監控已重新啟動", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }

        private void OpenLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NASFileWatcher",
                    "Logs"
                );

                // 如果資料夾不存在,先建立它
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                Process.Start("explorer.exe", logPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法開啟日誌資料夾: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PauseResume_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                _watcherService.Pause();
                PauseMenuItem.Header = "繼續監控";
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "監控已暫停", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
            else
            {
                _watcherService.Resume();
                PauseMenuItem.Header = "暫停監控";
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "監控已繼續", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }

            UpdateTrayIcon(!_isPaused);
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _watcherService.Reconnect();
                _isPaused = false;
                PauseMenuItem.Header = "⏸️ 暫停監控";
                UpdateTrayIcon(true);
                TrayIcon.ShowBalloonTip("NAS 檔案監控", "重新連接成功", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                UpdateTrayIcon(false);
                TrayIcon.ShowBalloonTip("NAS 檔案監控", $"重新連接失敗: {ex.Message}", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // 直接關閉,不顯示確認對話框
            _watcherService?.Stop();
            TrayIcon.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _watcherService?.Stop();
            TrayIcon.Dispose();
            base.OnClosing(e);
        }
    }

}
