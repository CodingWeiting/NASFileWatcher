# NAS File Watcher - NAS 檔案監控工具

監控 NAS 族群庫中的 .rfa 檔案變動,並透過 n8n Webhook 發送 Slack 通知。

## 功能特色

✅ **即時監控** - 使用 FileSystemWatcher 即時偵測檔案變動  
✅ **自動過濾** - 排除 .0001.rfa 等 Revit 備份檔案  
✅ **防抖動機制** - 避免同一檔案短時間內重複通知  
✅ **托盤程式** - 常駐系統托盤,不佔用工作列空間  
✅ **視覺化管理** - 查看最近 20 筆變動記錄  
✅ **彈性設定** - 可自訂 NAS 路徑、Webhook URL 等參數  

## 系統需求

- Windows 10 或更新版本
- .NET 6.0 Runtime (Windows Desktop)
- 可存取的 NAS 網路磁碟機
- n8n 或其他接收 Webhook 的服務

## 安裝步驟

### 1. 安裝 .NET Runtime

如果尚未安裝,請下載並安裝:
https://dotnet.microsoft.com/download/dotnet/6.0

選擇 "Desktop Runtime" (Windows x64)

### 2. 編譯程式

```bash
cd NASFileWatcher
dotnet restore
dotnet build -c Release
```

編譯後的程式位於: `bin/Release/net6.0-windows/`

### 3. 首次執行設定

1. 執行 `NASFileWatcher.exe`
2. 右鍵托盤圖示 → **設定**
3. 填寫以下資訊:
   - **NAS 路徑**: 例如 `\\192.168.1.100\FamilyLibrary`
   - **Webhook URL**: 你的 n8n Webhook 網址
   - **防抖動時間**: 建議 3 秒
   - **啟用日誌**: 勾選以記錄詳細資訊

4. 點擊 **測試 NAS 連線** 和 **測試 Webhook** 確認設定正確
5. 點擊 **儲存**

## 使用說明

### 托盤選單

右鍵點擊系統托盤圖示可開啟選單:

- 📋 **查看最近通知** - 顯示最近 20 筆檔案變動
- ⚙️ **設定** - 修改 NAS 路徑、Webhook URL 等
- 📄 **開啟日誌檔** - 查看詳細日誌記錄
- ⏸️ **暫停監控** / ▶️ **繼續監控** - 臨時暫停或繼續
- ❌ **結束程式** - 關閉監控程式

### 狀態指示

- 🟢 **綠色圖示** - 正常運行中
- 🟡 **黃色圖示** - 已暫停監控
- 🔴 **紅色圖示** - 連線異常或錯誤

## Webhook 資料格式

程式會發送 JSON 格式的資料到你的 n8n Webhook:

```json
{
  "eventType": "created",
  "fileName": "H型鋼-300x150.rfa",
  "filePath": "\\\\192.168.1.100\\FamilyLibrary\\結構\\鋼構\\H型鋼-300x150.rfa",
  "relativePath": "結構\\鋼構\\H型鋼-300x150.rfa",
  "timestamp": "2025-11-17T14:30:00+08:00",
  "oldName": null
}
```

### 事件類型 (eventType)

- `created` - 新增檔案
- `modified` - 修改檔案
- `deleted` - 刪除檔案
- `renamed` - 重新命名檔案 (會有 oldName 欄位)

## n8n Workflow 範例

### 基本 Workflow 結構

```
[Webhook Trigger]
    ↓
[Function Node: 格式化訊息]
    ↓
[Slack Node: 發送通知]
```

### Function Node 範例程式碼

```javascript
// 取得 Webhook 資料
const eventType = $json.eventType;
const fileName = $json.fileName;
const relativePath = $json.relativePath;
const timestamp = $json.timestamp;
const oldName = $json.oldName;

// 轉換事件類型為中文
const eventMap = {
  'created': '📁 新增',
  'modified': '✏️ 修改',
  'deleted': '🗑️ 刪除',
  'renamed': '📝 重新命名'
};

// 格式化時間
const time = new Date(timestamp).toLocaleString('zh-TW', {
  timeZone: 'Asia/Taipei',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit'
});

// 建立訊息
let message = `${eventMap[eventType]} 族群檔案\n`;
message += `檔案: ${fileName}\n`;
message += `路徑: ${relativePath}\n`;
message += `時間: ${time}`;

if (eventType === 'renamed' && oldName) {
  message += `\n原檔名: ${oldName}`;
}

return {
  json: {
    text: message
  }
};
```

### Slack Node 設定

- **Channel**: 選擇你的頻道 (例如 #revit-alerts)
- **Message**: `{{$json.text}}`

## 設定檔說明

設定檔位於: `config.json`

```json
{
  "NasPath": "\\\\192.168.1.100\\FamilyLibrary",
  "WebhookUrl": "https://your-n8n.com/webhook/file-change",
  "DebounceSeconds": 3,
  "EnableLogging": true
}
```

## 日誌檔案

日誌檔案儲存位置: `Logs/` 資料夾

- 每天產生一個日誌檔: `2025-11-17.log`
- 自動保留最近 30 天的日誌
- 可透過托盤選單 → **開啟日誌檔** 快速存取

## 開機自動啟動 (選擇性)

### 方法 1: 啟動資料夾

1. 按 `Win + R` 輸入 `shell:startup` 並按 Enter
2. 在開啟的資料夾中建立 `NASFileWatcher.exe` 的捷徑
3. 重新啟動電腦測試

### 方法 2: 工作排程器

1. 開啟 **工作排程器**
2. 建立基本工作 → 設定為 **登入時** 啟動
3. 動作選擇 **啟動程式** → 瀏覽到 `NASFileWatcher.exe`

## 常見問題

### Q: 程式無法連線到 NAS?

**A:** 請檢查:
1. NAS 路徑格式是否正確 (例如 `\\192.168.1.100\ShareFolder`)
2. 網路是否正常連線
3. 是否有檔案存取權限
4. 可以嘗試在檔案總管手動開啟該路徑測試

### Q: Webhook 測試失敗?

**A:** 請檢查:
1. n8n Webhook URL 是否正確
2. n8n 服務是否正常運行
3. 防火牆是否阻擋連線
4. 網路是否可以連到 n8n 伺服器

### Q: 為什麼有些檔案變動沒有通知?

**A:** 可能原因:
1. 該檔案是備份檔 (.0001.rfa) 已被自動過濾
2. 防抖動機制:同一檔案在 3 秒內的多次變動只會通知一次
3. 網路暫時中斷
4. 檢查日誌檔確認詳細情況

### Q: 如何變更監控的檔案類型?

**A:** 目前程式只監控 `.rfa` 檔案。如果需要監控其他類型,請修改程式碼中的:
```csharp
Filter = "*.rfa"  // 改成其他副檔名或 "*.*" 監控所有檔案
```

### Q: 可以同時監控多個 NAS 資料夾嗎?

**A:** 目前版本只支援單一資料夾。如果需要監控多個資料夾,可以:
1. 執行多個程式實例(需要修改 Mutex 名稱)
2. 或將多個資料夾都設為監控根目錄的子資料夾

## 技術細節

### 使用的 NuGet 套件

- `Hardcodet.NotifyIcon.Wpf` 1.1.0 - 系統托盤圖示
- `System.Text.Json` 8.0.0 - JSON 序列化

### 防抖動機制

為避免 Revit 儲存檔案時觸發多次事件,程式實作了防抖動機制:
1. 檔案變動時先加入待處理佇列
2. 等待 N 秒(可設定)
3. N 秒內同一檔案的多次變動只發送一次通知

### 備份檔過濾規則

使用正則表達式 `\.\d{4}\.rfa$` 過濾備份檔:
- ✅ 過濾: `檔名.0001.rfa`
- ✅ 過濾: `檔名.9999.rfa`
- ❌ 不過濾: `檔名.rfa`
- ❌ 不過濾: `檔名2024.rfa` (因為不是 .數字.rfa 格式)

## 授權

MIT License

## 版本歷史

### v1.0.0 (2025-11-17)
- 初始版本
- 支援監控 NAS .rfa 檔案
- 托盤程式介面
- n8n Webhook 整合
- 防抖動機制
- 備份檔自動過濾

## 作者

Kyros

---

如有問題或建議,請隨時聯繫! 🚀
