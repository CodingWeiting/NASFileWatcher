# 快速開始指南

## 📋 前置需求

### 1. 安裝 .NET 6.0 Desktop Runtime

**下載連結:** https://dotnet.microsoft.com/download/dotnet/6.0

**選擇版本:**
- Windows x64: `.NET Desktop Runtime 6.0.x`
- 下載並安裝 `windowsdesktop-runtime-6.0.xx-win-x64.exe`

**驗證安裝:**
```bash
dotnet --version
```

### 2. Visual Studio (建議) 或 Visual Studio Code

**Visual Studio 2022 (推薦):**
- 下載: https://visualstudio.microsoft.com/
- 工作負載選擇: `.NET 桌面開發`

**或使用 Visual Studio Code:**
- 下載: https://code.visualstudio.com/
- 安裝擴充套件: `C# Dev Kit`

---

## 🚀 編譯方式

### 方法 1: 使用 Visual Studio (最簡單)

1. **開啟專案**
   - 雙擊 `NASFileWatcher.sln`
   - Visual Studio 會自動開啟

2. **還原 NuGet 套件**
   - Visual Studio 會自動還原
   - 或手動: 右鍵方案 → `還原 NuGet 套件`

3. **編譯**
   - 按 `Ctrl + Shift + B`
   - 或點選 `建置` → `建置方案`

4. **執行**
   - 按 `F5` (偵錯模式)
   - 或 `Ctrl + F5` (非偵錯模式)

5. **發行版本**
   - 右鍵專案 → `發行`
   - 選擇 `資料夾`
   - 設定 `Release` 模式
   - 點擊 `發行`

**編譯後位置:**
```
bin/Release/net6.0-windows/
```

---

### 方法 2: 使用命令列

1. **開啟命令提示字元或 PowerShell**
   ```bash
   cd NASFileWatcher
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **編譯 (Debug 版本)**
   ```bash
   dotnet build
   ```

4. **編譯 (Release 版本)**
   ```bash
   dotnet build -c Release
   ```

5. **執行**
   ```bash
   dotnet run
   ```

**編譯後位置:**
```
bin/Debug/net6.0-windows/        (Debug 版)
bin/Release/net6.0-windows/      (Release 版)
```

---

### 方法 3: 使用 Visual Studio Code

1. **開啟資料夾**
   - File → Open Folder
   - 選擇 `NASFileWatcher` 資料夾

2. **安裝擴充套件** (如果還沒裝)
   - C# Dev Kit

3. **還原套件**
   - 按 `Ctrl + Shift + P`
   - 輸入 `.NET: Restore All Projects`

4. **編譯**
   - 按 `Ctrl + Shift + B`
   - 選擇 `build`

5. **執行**
   - 按 `F5`

---

## 📦 必要的 NuGet 套件

程式會自動下載這些套件:

1. **Hardcodet.NotifyIcon.Wpf** (1.1.0)
   - 系統托盤圖示功能

2. **System.Text.Json** (8.0.0)
   - JSON 序列化/反序列化

**如果自動還原失敗,手動安裝:**
```bash
dotnet add package Hardcodet.NotifyIcon.Wpf --version 1.1.0
dotnet add package System.Text.Json --version 8.0.0
```

---

## ⚙️ 首次設定

### 1. 執行程式
第一次執行時,程式會自動建立預設設定檔 `config.json`

### 2. 開啟設定視窗
- 右鍵點擊系統托盤的圖示
- 選擇 `設定`

### 3. 填寫資訊

#### NAS 路徑
```
範例: \\192.168.1.100\FamilyLibrary
或: \\NAS名稱\共享資料夾
```

#### Webhook URL
```
你的 n8n Webhook 完整網址
例如: https://your-n8n.com/webhook/file-change
```

#### 防抖動時間
```
建議: 3 秒
範圍: 1-10 秒
```

#### 批次通知設定
```
啟用: ✅ 勾選
批次閾值: 10 個檔案
時間窗口: 10 秒
```

### 4. 測試連線

點擊兩個測試按鈕:
- `測試 NAS 連線` - 確認可以存取 NAS
- `測試 Webhook` - 確認 n8n 可以接收

### 5. 儲存設定

點擊 `儲存` 後,程式會自動重新啟動監控。

---

## 🔧 常見編譯問題

### 問題 1: 找不到 .NET SDK

**錯誤訊息:**
```
The command 'dotnet' is not recognized...
```

**解決方法:**
1. 確認已安裝 .NET 6.0 Desktop Runtime
2. 重新啟動命令提示字元
3. 如果還是不行,重新安裝 .NET

---

### 問題 2: NuGet 套件還原失敗

**錯誤訊息:**
```
Unable to find package...
```

**解決方法:**
```bash
# 清除 NuGet 快取
dotnet nuget locals all --clear

# 重新還原
dotnet restore
```

---

### 問題 3: 編譯錯誤 - 找不到 TaskbarIcon

**錯誤訊息:**
```
The type or namespace name 'TaskbarIcon' could not be found
```

**解決方法:**
確認 `Hardcodet.NotifyIcon.Wpf` 套件已正確安裝:
```bash
dotnet list package
```

如果沒有,手動安裝:
```bash
dotnet add package Hardcodet.NotifyIcon.Wpf --version 1.1.0
```

---

### 問題 4: 執行時找不到 DLL

**錯誤訊息:**
```
Could not load file or assembly 'Hardcodet.Wpf.TaskbarNotification'
```

**解決方法:**
1. 確認所有 DLL 都在執行檔同一資料夾
2. 重新編譯 Release 版本:
   ```bash
   dotnet build -c Release
   ```

---

## 📁 專案結構說明

```
NASFileWatcher/
├── NASFileWatcher.sln          ← Solution 檔案 (用 Visual Studio 開啟)
├── NASFileWatcher.csproj       ← 專案檔
├── App.xaml                    ← 應用程式定義
├── App.xaml.cs                 ← 應用程式邏輯
├── MainWindow.xaml             ← 主視窗介面
├── MainWindow.xaml.cs          ← 主視窗邏輯
├── SettingsWindow.xaml         ← 設定視窗介面
├── SettingsWindow.xaml.cs      ← 設定視窗邏輯
├── RecentNotificationsWindow.xaml      ← 最近通知視窗
├── RecentNotificationsWindow.xaml.cs   ← 最近通知邏輯
├── Config.cs                   ← 設定檔管理
├── Logger.cs                   ← 日誌記錄
├── FileWatcherService.cs       ← 檔案監控核心
├── WebhookSender.cs           ← Webhook 發送
├── config.example.json         ← 設定檔範例
├── README.md                   ← 完整說明文件
├── QUICK_START.md             ← 本檔案
└── 其他文件...
```

---

## 🎯 編譯後的檔案

### Debug 版本
```
bin/Debug/net6.0-windows/
├── NASFileWatcher.exe          ← 主程式
├── NASFileWatcher.dll
├── config.json                 ← 設定檔 (執行後產生)
├── Hardcodet.Wpf.TaskbarNotification.dll
├── System.Text.Json.dll
└── Logs/                       ← 日誌資料夾 (執行後產生)
```

### Release 版本 (建議使用)
```
bin/Release/net6.0-windows/
├── NASFileWatcher.exe          ← 主程式
└── (其他 DLL 檔案)
```

---

## 🚀 部署到使用者電腦

### 方法 1: 複製整個資料夾

1. 編譯 Release 版本
2. 複製整個 `bin/Release/net6.0-windows/` 資料夾
3. 在目標電腦上:
   - 確認已安裝 .NET 6.0 Desktop Runtime
   - 執行 `NASFileWatcher.exe`

### 方法 2: 發行為單一執行檔 (進階)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

這會產生一個包含所有相依性的單一 .exe 檔案。

---

## 🔍 驗證安裝

執行程式後,檢查:

1. ✅ 系統托盤出現圖示
2. ✅ 右鍵選單正常運作
3. ✅ 設定視窗可以開啟
4. ✅ NAS 連線測試成功
5. ✅ Webhook 測試成功
6. ✅ 日誌資料夾已建立

---

## 📞 需要協助?

**常見問題:**
- 查看 `README.md` 的常見問題章節
- 檢查 `Logs/` 資料夾的日誌檔

**技術問題:**
- .NET 安裝問題 → 查看 Microsoft 官方文件
- NuGet 套件問題 → 清除快取後重新還原

---

**祝編譯順利!** 🎉
