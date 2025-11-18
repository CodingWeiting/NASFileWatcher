# NASFileWatcher 安裝程式

本資料夾包含 NASFileWatcher 的 Inno Setup 安裝程式腳本和建置工具。

---

## 📦 檔案說明

| 檔案 | 說明 |
|------|------|
| `NASFileWatcher-Setup.iss` | Inno Setup 安裝程式腳本 |
| `Build-Installer.ps1` | PowerShell 自動化建置腳本 |
| `Output/` | 編譯後的安裝檔輸出位置 (自動產生) |

---

## 🚀 快速開始

### 方法 1: 使用自動化腳本 (推薦)

1. 在此資料夾按右鍵 → **在終端機中開啟**
2. 執行建置腳本:
   ```powershell
   .\Build-Installer.ps1
   ```
3. 腳本會自動:
   - ✅ 檢查環境 (.NET SDK、Inno Setup)
   - ✅ 清理舊檔案
   - ✅ 編譯專案
   - ✅ 建立安裝程式
   - ✅ 顯示結果

### 方法 2: 手動編譯

#### 步驟 1: 編譯專案

在編譯安裝程式之前,必須先編譯 .NET 專案:

```powershell
# 在專案根目錄執行
cd "C:\Users\KAOPC120\Desktop\C# Apps\NASFileWatcher"

# 清理舊檔案
dotnet clean

# 發佈 Release 版本 (包含所有依賴檔案)
dotnet publish -c Release -r win-x64 --self-contained false
```

**重要**: 確保 `bin\Release\net8.0-windows\publish` 資料夾存在並包含所有檔案!

#### 步驟 2: 編譯安裝程式

**方法 A: 圖形介面**
1. 在檔案總管中找到 `Setup\NASFileWatcher-Setup.iss`
2. 對檔案按右鍵 → **Compile**
3. Inno Setup Compiler 會自動開啟並開始編譯
4. 等待編譯完成

**方法 B: Inno Setup Compiler**
1. 開啟 **Inno Setup Compiler**
2. File → Open → 選擇 `NASFileWatcher-Setup.iss`
3. Build → Compile (或按 Ctrl+F9)
4. 等待編譯完成

**方法 C: 命令列**
```powershell
cd "C:\Users\KAOPC120\Desktop\C# Apps\NASFileWatcher\Setup"
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "NASFileWatcher-Setup.iss"
```

#### 輸出檔案

編譯成功後,安裝檔會產生在:

```
Setup\Output\NASFileWatcher-Setup-v1.0.0.exe
```

檔案大小約: 15-30 MB (取決於依賴項目)

---

## ✨ 安裝程式功能

### 安裝時:
- ✅ 自動檢查 .NET 8.0 Desktop Runtime
- ✅ 自訂安裝路徑
- ✅ 建立桌面捷徑 (可選)
- ✅ 開機自動啟動 (可選)
- ✅ 建立開始功能表資料夾
- ✅ 自動從範例建立 config.json
- ✅ 建立 Logs 資料夾
- ✅ 支援繁體中文和英文介面

### 解除安裝時:
- ✅ 詢問是否保留設定檔 (config.json)
- ✅ 詢問是否保留日誌資料夾 (Logs)
- ✅ 自動移除桌面捷徑
- ✅ 自動移除開機啟動捷徑
- ✅ 移除開始功能表資料夾

---

## 📋 前置需求

### 建置安裝程式需要:

1. **.NET 8.0 SDK**
   - 下載: https://dotnet.microsoft.com/download/dotnet/8.0
   - 選擇: SDK (開發用)

2. **Inno Setup 6.x**
   - 下載: https://jrsoftware.org/isdl.php
   - 選擇: innosetup-6.x.x.exe

### 使用者安裝時需要:

1. **.NET 8.0 Desktop Runtime**
   - 下載: https://dotnet.microsoft.com/download/dotnet/8.0
   - 選擇: Desktop Runtime (使用者用)
   - 安裝程式會自動偵測,若未安裝會提示下載

---

## 🎯 安裝後的檔案結構

```
C:\Program Files\NASFileWatcher\
├── NASFileWatcher.exe           # 主程式
├── *.dll                        # 依賴的 DLL 檔案
├── config.json                  # 設定檔 (從範例複製)
├── config.example.json          # 設定檔範例
├── Assets\
│   └── app.ico                  # 應用程式圖示
├── docs\                        # 文件資料夾
│   ├── README.md
│   ├── QUICK_START.md
│   ├── BATCH_NOTIFICATION.md
│   ├── DISPLAY_LOGIC.md
│   ├── FOLDER_STRUCTURE.md
│   └── FOLDER_STRUCTURE_EXAMPLES.md
├── workflows\                   # n8n 工作流程
│   └── n8n-workflow.json
└── Logs\                        # 日誌資料夾 (空)
```

### 捷徑位置:

- **桌面**: `NAS 檔案監控.lnk`
- **開始功能表**: `開始 → NAS 檔案監控`
  - NAS 檔案監控
  - 設定 (開啟 config.json)
  - 開啟日誌資料夾
  - README
  - 解除安裝
- **開機啟動**: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\NAS 檔案監控.lnk`

---

## ✅ 測試安裝程式

### 基本測試

1. **執行安裝程式**
   - 雙擊 `NASFileWatcher-Setup-v1.0.0.exe`
   - 確認歡迎畫面正常顯示

2. **選擇安裝路徑**
   - 預設: `C:\Program Files\NASFileWatcher`
   - 測試自訂路徑

3. **選擇元件**
   - ✅ 建立桌面捷徑
   - ✅ 開機自動啟動

4. **完成安裝**
   - 勾選「執行程式」
   - 確認程式能正常啟動

### 功能測試

1. **檢查檔案結構**
   ```
   C:\Program Files\NASFileWatcher\
   ├── NASFileWatcher.exe
   ├── config.json (從 config.example.json 複製)
   ├── config.example.json
   ├── docs\
   │   └── *.md
   ├── workflows\
   │   └── n8n-workflow.json
   ├── Logs\ (空資料夾)
   └── [其他 DLL 檔案]
   ```

2. **檢查捷徑**
   - 桌面: `NAS 檔案監控.lnk`
   - 開始功能表: `NAS 檔案監控` 資料夾
   - 啟動資料夾: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\NAS 檔案監控.lnk`

3. **測試程式功能**
   - 開啟設定視窗
   - 修改設定並儲存
   - 確認 config.json 有更新

4. **測試解除安裝**
   - 控制台 → 程式和功能 → 解除安裝
   - 確認詢問是否保留設定檔
   - 確認詢問是否保留日誌
   - 確認所有檔案和捷徑都被移除

---

## 🔧 自訂安裝程式

### 修改版本號

編輯 `NASFileWatcher-Setup.iss` 第 28 行:

```pascal
#define MyAppVersion "1.0.0"  // 改成新版本號
```

### 修改發佈者資訊

編輯 `NASFileWatcher-Setup.iss` 第 29-30 行:

```pascal
#define MyAppPublisher "你的名字"
#define MyAppURL "https://github.com/yourusername/NASFileWatcher"
```

### 加入授權協議

在專案根目錄建立 `LICENSE.txt`,然後在 `[Setup]` 區段加入:

```pascal
LicenseFile=..\LICENSE.txt
```

### 修改安裝程式圖示

替換 `Assets\app.ico` 或修改第 56 行:

```pascal
SetupIconFile=..\Assets\your-icon.ico
```

---

## 📝 版本管理

### 更新版本號時需要修改的地方:

1. **Inno Setup 腳本** (`NASFileWatcher-Setup.iss`)
   ```pascal
   #define MyAppVersion "1.0.0"  // 第 28 行
   ```

2. **專案檔** (`NASFileWatcher.csproj`)
   ```xml
   <Version>1.0.0</Version>
   ```

3. **README.md** 文件

### 建議的版本號規則:

- **主版本號**: 重大功能更新 (1.x.x → 2.x.x)
- **次版本號**: 新增功能 (1.0.x → 1.1.x)
- **修訂號**: Bug 修復 (1.0.0 → 1.0.1)

---

## 🚀 發佈流程

1. **更新版本號** (上述 3 個地方)
2. **測試程式功能** (確保沒有 Bug)
3. **編譯 Release 版本**
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained false
   ```
4. **編譯安裝程式** (使用 Inno Setup)
5. **測試安裝程式** (完整安裝/解除安裝測試)
6. **發佈**
   - 上傳到 GitHub Releases
   - 或分享給使用者

---

## 📝 版本歷史

### v1.0.0 (2025-11-18)
- ✅ 初始版本
- ✅ 完整的安裝/解除安裝功能
- ✅ .NET Runtime 檢查
- ✅ 繁體中文/英文雙語支援
- ✅ 自動建立設定檔
- ✅ 開機自動啟動選項

---

## 🔧 常見問題

### Q1: 編譯時出現「找不到檔案」錯誤

**A**: 確保已先執行 `dotnet publish`,並檢查 `bin\Release\net8.0-windows\publish` 資料夾是否存在。

### Q2: 安裝時提示缺少 .NET Runtime

**A**: 這是正常的!安裝程式會提示使用者下載並安裝 .NET 8.0 Desktop Runtime。

### Q3: 想修改版本號

**A**: 編輯 `NASFileWatcher-Setup.iss`,修改第 28 行:
```pascal
#define MyAppVersion "1.0.0"  // 改成新版本號
```

### Q4: 想加入授權協議

**A**: 在 `[Setup]` 區段加入:
```pascal
LicenseFile=..\LICENSE.txt
```

### Q5: 想自訂安裝程式圖示

**A**: 替換 `Assets\app.ico` 檔案,或修改第 56 行:
```pascal
SetupIconFile=..\Assets\your-icon.ico
```

---

## 🐛 已知問題

### 問題 1: Windows Defender 誤報

**現象**: 安裝程式被 Windows Defender 標記為可疑

**解決**:
1. 這是因為程式沒有數位簽章
2. 可以暫時關閉 SmartScreen
3. 或右鍵 → 內容 → 解除封鎖
4. 正式發佈建議購買代碼簽章憑證

### 問題 2: 需要管理員權限

**現象**: 安裝時要求管理員權限

**原因**: 預設安裝路徑在 `Program Files`

**解決**:
- 這是正常的,點擊「是」繼續
- 或修改腳本改為使用者資料夾:
  ```pascal
  DefaultDirName={autopf}\{#MyAppEnglishName}  // 改成
  DefaultDirName={localappdata}\{#MyAppEnglishName}
  ```

---

## 📚 相關資源

- **Inno Setup 官方文件**: https://jrsoftware.org/ishelp/
- **Inno Setup 中文教學**: https://www.jrsoftware.org/ischinese.php
- **Inno Setup 範例**: https://jrsoftware.org/isinfo.php
- **.NET 發佈指南**: https://learn.microsoft.com/dotnet/core/deploying/

---

## 💡 技術支援

如有任何問題:

1. 查看本文件的「常見問題」區段
2. 檢查 Inno Setup 編譯輸出的錯誤訊息
3. 確認已正確執行 `dotnet publish`
4. 提交 Issue 到專案 GitHub

---

**建立日期**: 2025-11-18
**最後更新**: 2025-11-18
**版本**: 1.0.0
