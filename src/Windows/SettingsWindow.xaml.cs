using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using NASFileWatcher.Core;
using NASFileWatcher.Models;
using Microsoft.Win32;

namespace NASFileWatcher.Windows
{
    public partial class SettingsWindow : Window
    {
        private AppConfig _config;
        public bool IsClosed { get; private set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            Closed += (s, e) => IsClosed = true;

            // 註冊自動儲存事件
            NasPathTextBox.TextChanged += AutoSave;
            WebhookUrlTextBox.TextChanged += AutoSave;
            DebounceSlider.ValueChanged += AutoSave;
            EnableLoggingCheckBox.Checked += AutoSave;
            EnableLoggingCheckBox.Unchecked += AutoSave;
            EnableBatchCheckBox.Checked += AutoSave;
            EnableBatchCheckBox.Unchecked += AutoSave;
            BatchThresholdSlider.ValueChanged += AutoSave;
            BatchWindowSlider.ValueChanged += AutoSave;
        }

        private void LoadSettings()
        {
            _config = AppConfig.Load();

            NasPathTextBox.Text = _config.NasPath;
            WebhookUrlTextBox.Text = _config.WebhookUrl;
            DebounceSlider.Value = _config.DebounceSeconds;
            EnableLoggingCheckBox.IsChecked = _config.EnableLogging;
            EnableBatchCheckBox.IsChecked = _config.EnableBatchNotification;
            BatchThresholdSlider.Value = _config.BatchThreshold;
            BatchWindowSlider.Value = _config.BatchWindowSeconds;
        }

        private void BrowseNasButton_Click(object sender, RoutedEventArgs e)
        {
            // 使用 Windows Forms FolderBrowserDialog 來選擇資料夾
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "選擇 NAS 資料夾或本機資料夾";
                dialog.ShowNewFolderButton = false;

                // 如果已有路徑,設定為初始路徑
                if (!string.IsNullOrWhiteSpace(NasPathTextBox.Text) && Directory.Exists(NasPathTextBox.Text))
                {
                    dialog.SelectedPath = NasPathTextBox.Text;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    NasPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private async void TestNasButton_Click(object sender, RoutedEventArgs e)
        {
            string nasPath = NasPathTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(nasPath))
            {
                MessageBox.Show("請輸入 NAS 路徑", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TestNasButton.IsEnabled = false;
            TestNasButton.Content = "測試中...";

            await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(nasPath))
                    {
                        // 嘗試列出檔案
                        var files = Directory.GetFiles(nasPath, "*.rfa", SearchOption.TopDirectoryOnly);
                        
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"連線成功!\n找到 {files.Length} 個 .rfa 檔案", 
                                "測試結果", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("找不到指定的 NAS 路徑\n\n請檢查:\n1. 路徑格式是否正確\n2. 網路連線是否正常\n3. 是否有存取權限", 
                                "連線失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("沒有存取權限\n請檢查您的帳號是否有權限存取此資料夾", 
                            "權限錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"測試失敗:\n{ex.Message}", 
                            "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        TestNasButton.IsEnabled = true;
                        TestNasButton.Content = "測試 NAS 連線";
                    });
                }
            });
        }

        private async void TestWebhookButton_Click(object sender, RoutedEventArgs e)
        {
            string webhookUrl = WebhookUrlTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                MessageBox.Show("請輸入 Webhook URL", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show("Webhook URL 格式不正確", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TestWebhookButton.IsEnabled = false;
            TestWebhookButton.Content = "測試中...";

            await Task.Run(async () =>
            {
                try
                {
                    var testNotification = new FileChangeNotification
                    {
                        EventType = "test",
                        FileName = "測試檔案.rfa",
                        FilePath = @"\\NAS\測試\測試檔案.rfa",
                        RelativePath = @"測試\測試檔案.rfa",
                        Timestamp = DateTime.Now
                    };

                    var sender = new WebhookSender(webhookUrl);
                    bool success = await sender.SendAsync(testNotification);

                    Dispatcher.Invoke(() =>
                    {
                        if (success)
                        {
                            MessageBox.Show("Webhook 測試成功!\n請檢查 n8n 或 Slack 是否收到測試訊息", 
                                "測試結果", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Webhook 測試失敗\n請檢查 URL 是否正確", 
                                "測試失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
                catch (HttpRequestException ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"網路錯誤:\n{ex.Message}\n\n請檢查:\n1. URL 是否正確\n2. n8n 服務是否運行\n3. 網路連線是否正常", 
                            "連線失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"測試失敗:\n{ex.Message}", 
                            "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        TestWebhookButton.IsEnabled = true;
                        TestWebhookButton.Content = "測試 Webhook";
                    });
                }
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 驗證輸入
            string nasPath = NasPathTextBox.Text.Trim();
            string webhookUrl = WebhookUrlTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nasPath))
            {
                MessageBox.Show("請輸入 NAS 路徑", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                NasPathTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                MessageBox.Show("請輸入 Webhook URL", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                WebhookUrlTextBox.Focus();
                return;
            }

            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show("Webhook URL 格式不正確", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                WebhookUrlTextBox.Focus();
                return;
            }

            // 儲存設定
            _config.NasPath = nasPath;
            _config.WebhookUrl = webhookUrl;
            _config.DebounceSeconds = (int)DebounceSlider.Value;
            _config.EnableLogging = EnableLoggingCheckBox.IsChecked ?? true;
            _config.EnableBatchNotification = EnableBatchCheckBox.IsChecked ?? true;
            _config.BatchThreshold = (int)BatchThresholdSlider.Value;
            _config.BatchWindowSeconds = (int)BatchWindowSlider.Value;

            try
            {
                _config.Save();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存設定失敗:\n{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AutoSave(object sender, RoutedEventArgs e)
        {
            // 避免在載入設定時觸發自動儲存
            if (!IsLoaded)
                return;

            try
            {
                // 更新設定物件
                _config.NasPath = NasPathTextBox.Text.Trim();
                _config.WebhookUrl = WebhookUrlTextBox.Text.Trim();
                _config.DebounceSeconds = (int)DebounceSlider.Value;
                _config.EnableLogging = EnableLoggingCheckBox.IsChecked ?? true;
                _config.EnableBatchNotification = EnableBatchCheckBox.IsChecked ?? true;
                _config.BatchThreshold = (int)BatchThresholdSlider.Value;
                _config.BatchWindowSeconds = (int)BatchWindowSlider.Value;

                // 儲存到檔案
                _config.Save();
            }
            catch (Exception ex)
            {
                // 自動儲存失敗時不顯示錯誤,避免干擾使用者
                System.Diagnostics.Debug.WriteLine($"自動儲存失敗: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 關閉前最後一次儲存
            try
            {
                _config.NasPath = NasPathTextBox.Text.Trim();
                _config.WebhookUrl = WebhookUrlTextBox.Text.Trim();
                _config.DebounceSeconds = (int)DebounceSlider.Value;
                _config.EnableLogging = EnableLoggingCheckBox.IsChecked ?? true;
                _config.EnableBatchNotification = EnableBatchCheckBox.IsChecked ?? true;
                _config.BatchThreshold = (int)BatchThresholdSlider.Value;
                _config.BatchWindowSeconds = (int)BatchWindowSlider.Value;

                _config.Save();
                DialogResult = true; // 告訴 MainWindow 需要重新啟動監控
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存設定失敗:\n{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }

            Close();
        }
    }
}
