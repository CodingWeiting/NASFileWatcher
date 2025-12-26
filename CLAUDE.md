# NAS File Watcher 開發指南

## 版本更新

更新版本時需修改以下檔案：

- `NASFileWatcher.csproj` - 修改 `<Version>` 標籤
- `Setup/Build-Installer.cmd` - 修改 `VERSION` 變數和標題列版本號
- `Setup/NASFileWatcher-Setup.iss` - 修改 `#define MyAppVersion`

## 編譯指令

- **測試編譯**: `dotnet build --configuration Debug`
- **發布編譯**: `dotnet build --configuration Release`
- **建置安裝程式**: 執行 `Setup/Build-Installer.cmd`

## 輸出路徑

- Debug: `bin\Debug\net8.0-windows\`
- Release: `bin\Release\net8.0-windows\`
- 安裝程式: `Setup/Output/`
