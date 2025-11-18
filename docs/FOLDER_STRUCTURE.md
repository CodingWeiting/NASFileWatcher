# NASFileWatcher 專案資料夾結構說明

本文件說明重構後的專案資料夾結構,方便開發者快速理解專案組織。

## 📁 專案結構總覽

```
NASFileWatcher/
├── 📁 src/                          # 源碼資料夾
│   ├── 📁 Core/                     # 核心業務邏輯
│   │   ├── FileWatcherService.cs    # 檔案監控服務(核心)
│   │   ├── Config.cs                # 設定檔管理
│   │   ├── Logger.cs                # 日誌記錄器
│   │   └── WebhookSender.cs         # Webhook 發送器
│   │
│   ├── 📁 Models/                   # 資料模型
│   │   ├── FileChangeNotification.cs        # 檔案變動通知模型
│   │   └── StatusChangedEventArgs.cs        # 狀態變更事件參數
│   │
│   ├── 📁 Windows/                  # 視窗介面
│   │   ├── MainWindow.xaml          # 主視窗(托盤程式)
│   │   ├── MainWindow.xaml.cs
│   │   ├── SettingsWindow.xaml      # 設定視窗
│   │   ├── SettingsWindow.xaml.cs
│   │   ├── RecentNotificationsWindow.xaml # 通知記錄視窗
│   │   └── RecentNotificationsWindow.xaml.cs
│   │
│   ├── App.xaml                     # 應用程式定義
│   └── App.xaml.cs                  # 應用程式進入點
│
├── 📁 Assets/                       # 資源檔案
│   └── app.ico                      # 應用程式圖示
│
├── 📁 docs/                         # 文件資料夾
│   ├── README.md                    # 主要說明文件
│   ├── QUICK_START.md               # 快速開始指南
│   ├── BATCH_NOTIFICATION.md        # 批次通知說明
│   ├── DISPLAY_LOGIC.md             # 顯示邏輯說明
│   ├── FOLDER_STRUCTURE_EXAMPLES.md # 資料夾結構範例
│   └── FOLDER_STRUCTURE.md          # 本文件
│
├── 📁 config/                       # 設定檔案
│   └── config.example.json          # 設定範例
│
├── 📁 workflows/                    # n8n 工作流程
│   └── n8n-workflow.json            # n8n workflow 設定
│
├── 📁 bin/                          # 編譯輸出(自動產生)
├── 📁 obj/                          # 編譯中間檔(自動產生)
├── 📁 Logs/                         # 日誌資料夾(執行時產生)
│
├── NASFileWatcher.csproj            # 專案檔
├── NASFileWatcher.sln               # 方案檔
└── config.json                      # 執行時設定檔(由使用者建立)
```

---

## 📂 資料夾說明

### 1. `src/` - 源碼資料夾

所有程式碼都集中在這個資料夾,按照功能分層:

#### `src/Core/` - 核心業務邏輯層
包含應用程式的核心功能,不依賴 UI:

- **FileWatcherService.cs** (544 行)
  - 核心服務類別
  - 使用 `FileSystemWatcher` 監控檔案變動
  - 實作防抖動機制 (debounce)
  - 批次通知邏輯
  - 事件觸發與處理
  - 命名空間: `NASFileWatcher.Core`

- **Config.cs** (82 行)
  - 應用程式設定管理
  - 讀取/儲存 `config.json`
  - 設定驗證邏輯
  - 命名空間: `NASFileWatcher.Core`

- **Logger.cs** (80 行)
  - 日誌記錄器
  - 按日期自動分檔
  - 自動清理舊日誌 (保留 30 天)
  - 支援 Info/Warning/Error 三種級別
  - 命名空間: `NASFileWatcher.Core`

- **WebhookSender.cs** (126 行)
  - HTTP 請求發送器
  - 發送單一檔案變動通知
  - 發送批次變動通知
  - 錯誤處理與逾時控制
  - 命名空間: `NASFileWatcher.Core`

#### `src/Models/` - 資料模型層
定義應用程式使用的資料結構:

- **FileChangeNotification.cs**
  - `FileChangeNotification` - 檔案變動通知
  - `BatchFileChangeNotification` - 批次變動通知
  - `BatchFileInfo` - 批次檔案資訊
  - `FolderStructureInfo` - 資料夾結構資訊
  - `NotificationDisplayItem` - 顯示用通知項目
  - 命名空間: `NASFileWatcher.Models`

- **StatusChangedEventArgs.cs**
  - 狀態變更事件參數
  - 命名空間: `NASFileWatcher.Models`

#### `src/Windows/` - 視窗介面層
WPF 使用者介面:

- **MainWindow.xaml/.cs**
  - 主視窗 (隱藏,只顯示托盤圖示)
  - 托盤選單管理
  - 監控服務生命週期控制
  - 命名空間: `NASFileWatcher.Windows`

- **SettingsWindow.xaml/.cs**
  - 設定視窗
  - NAS 路徑、Webhook URL 設定
  - 批次通知參數調整
  - 測試功能 (測試 NAS 連線、測試 Webhook)
  - 命名空間: `NASFileWatcher.Windows`

- **RecentNotificationsWindow.xaml/.cs**
  - 最近通知記錄視窗
  - 顯示最近 20 筆檔案變動
  - 即時更新
  - 命名空間: `NASFileWatcher.Windows`

#### `src/App.xaml/.cs` - 應用程式進入點
- 應用程式啟動邏輯
- 單一實例控制 (Mutex)
- 全域例外處理
- 命名空間: `NASFileWatcher`

---

### 2. `Assets/` - 資源檔案

存放應用程式使用的資源:
- `app.ico` - 應用程式圖示 (22KB)

---

### 3. `docs/` - 文件資料夾

所有 Markdown 文件集中管理:
- **README.md** - 主要說明文件 (包含安裝、使用、常見問題)
- **QUICK_START.md** - 快速開始指南
- **BATCH_NOTIFICATION.md** - 批次通知功能說明
- **DISPLAY_LOGIC.md** - 顯示邏輯說明
- **FOLDER_STRUCTURE_EXAMPLES.md** - 資料夾結構範例
- **FOLDER_STRUCTURE.md** - 本文件 (專案結構說明)

---

### 4. `config/` - 設定檔案資料夾

存放設定檔範例:
- `config.example.json` - 設定檔範本

---

### 5. `workflows/` - 工作流程資料夾

存放 n8n 工作流程定義:
- `n8n-workflow.json` - n8n 自動化流程設定

---

### 6. 其他資料夾

- `bin/` - 編譯輸出資料夾 (由 MSBuild 自動產生)
- `obj/` - 編譯中間檔資料夾 (由 MSBuild 自動產生)
- `Logs/` - 日誌資料夾 (程式執行時自動產生)

---

## 🔄 命名空間架構

```
NASFileWatcher (根命名空間)
├── NASFileWatcher.Core          (核心層)
│   ├── FileWatcherService
│   ├── AppConfig
│   ├── Logger
│   ├── LogLevel (enum)
│   └── WebhookSender
│
├── NASFileWatcher.Models        (模型層)
│   ├── FileChangeNotification
│   ├── BatchFileChangeNotification
│   ├── BatchFileInfo
│   ├── FolderStructureInfo
│   ├── NotificationDisplayItem
│   └── StatusChangedEventArgs
│
└── NASFileWatcher.Windows       (UI層)
    ├── MainWindow
    ├── SettingsWindow
    └── RecentNotificationsWindow
```

---

## 📦 相依性說明

### NuGet 套件
- `Hardcodet.NotifyIcon.Wpf` 1.1.0 - 系統托盤圖示
- `System.Text.Json` 9.0.0 - JSON 序列化

### 內部相依性
- `Windows` 層 → 依賴 `Core` 和 `Models`
- `Core` 層 → 依賴 `Models`
- `Models` 層 → 無相依

---

## 🛠️ 編譯與建置

### 編譯專案
```bash
dotnet restore
dotnet build -c Release
```

### 發佈專案
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### 輸出位置
- Debug: `bin/Debug/net8.0-windows/`
- Release: `bin/Release/net8.0-windows/`

---

## 📝 開發建議

### 新增功能時的檔案放置建議:

1. **新增核心業務邏輯** → `src/Core/`
   - 例如: 新增郵件通知功能 → `EmailSender.cs`

2. **新增資料模型** → `src/Models/`
   - 例如: 新增郵件設定模型 → `EmailConfig.cs`

3. **新增 UI 視窗** → `src/Windows/`
   - 例如: 新增統計視窗 → `StatisticsWindow.xaml`

4. **新增文件** → `docs/`
   - 例如: 新增 API 文件 → `API.md`

### 命名規則:
- 類別檔案: PascalCase (例如 `FileWatcherService.cs`)
- 命名空間: 與資料夾結構對應
- XAML 視窗: 與 .cs 檔案同名

---

## 🔍 快速導航

### 想修改核心監控邏輯?
👉 `src/Core/FileWatcherService.cs`

### 想調整設定項目?
👉 `src/Core/Config.cs`

### 想修改 UI 介面?
👉 `src/Windows/`

### 想了解資料結構?
👉 `src/Models/`

### 想查看使用說明?
👉 `docs/README.md`

---

## ✅ 重構完成檢查清單

- ✅ 核心業務邏輯分離到 `Core/`
- ✅ 資料模型獨立到 `Models/`
- ✅ UI 視窗集中到 `Windows/`
- ✅ 資源檔案移到 `Assets/`
- ✅ 文件集中到 `docs/`
- ✅ 設定檔範例移到 `config/`
- ✅ 工作流程定義移到 `workflows/`
- ✅ 命名空間更新完成
- ✅ 專案檔 (.csproj) 路徑更新

---

**最後更新**: 2025-11-18
**版本**: 1.0.0
