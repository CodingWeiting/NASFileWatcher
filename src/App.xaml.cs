using System;
using System.Windows;
using NASFileWatcher.Windows;

namespace NASFileWatcher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 確保只運行一個實例
            bool createdNew;
            var mutex = new System.Threading.Mutex(true, "NASFileWatcherMutex", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("程式已在運行中!", "NAS 檔案監控", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return;
            }

            // 全域例外處理
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"未處理的錯誤: {ex?.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // 啟動主視窗
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
