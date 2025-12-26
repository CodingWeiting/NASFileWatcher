; ============================================================================
; NAS 檔案監控 - Inno Setup 安裝腳本
; ============================================================================
;
; 【編譯方式說明】
;
; 1. 安裝 Inno Setup:
;    下載網址: https://jrsoftware.org/isdl.php
;    選擇: innosetup-6.x.x.exe (最新版本)
;    安裝完成後會有 Inno Setup Compiler
;
; 2. 編譯此腳本:
;    方法 1: 在檔案總管中對此 .iss 檔案按右鍵 → Compile
;    方法 2: 開啟 Inno Setup Compiler → 開啟此檔案 → Build → Compile
;    方法 3: 命令列: iscc "NASFileWatcher-Setup.iss"
;
; 3. 輸出檔案位置:
;    編譯後的安裝檔會產生在: Setup\Output\NASFileWatcher-Setup-v{版本號}.exe
;
; 4. 編譯前準備:
;    確保已執行 "dotnet publish -c Release" 編譯專案
;    確保 bin\Release\net8.0-windows\publish 資料夾存在並包含所有檔案
;
; ============================================================================

#define MyAppName "NAS 檔案監控"
#define MyAppEnglishName "NASFileWatcher"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Kyros"
#define MyAppURL "https://github.com/yourusername/NASFileWatcher"
#define MyAppExeName "NASFileWatcher.exe"
#define MyAppYear "2025"

[Setup]
; 基本資訊
AppId={{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=Copyright (C) {#MyAppYear} {#MyAppPublisher}

; 預設安裝路徑
DefaultDirName={autopf}\{#MyAppEnglishName}
DefaultGroupName={#MyAppName}

; 允許使用者自訂路徑
DisableDirPage=no
DisableProgramGroupPage=no

; 輸出設定
OutputDir=Output
OutputBaseFilename=NASFileWatcher-Setup-v{#MyAppVersion}
SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; 壓縮設定
Compression=lzma2/max
SolidCompression=yes

; 系統需求
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
PrivilegesRequired=admin

; 介面設定
WizardStyle=modern
DisableWelcomePage=no
DisableReadyPage=no

; 版本資訊
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} 安裝程式
VersionInfoCopyright=Copyright (C) {#MyAppYear} {#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
DotNetNotInstalled=.NET 8.0 Desktop Runtime not detected!%n%nPlease install .NET 8.0 Desktop Runtime before continuing.%n%nOpen download page?
KeepConfigFile=Keep configuration file (config.json)?
KeepConfigFileDesc=Recommended if you plan to reinstall
KeepLogsFolder=Keep logs folder (Logs)?
KeepLogsFolderDesc=Recommended if you need to review historical logs
AutoStartup=Start with Windows
AutoStartupDesc=Add program to Windows startup folder
DesktopShortcut=Create desktop shortcut

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopShortcut}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "{cm:AutoStartup}"; GroupDescription: "{cm:AutoStartupDesc}"; Flags: checkedonce

[Files]
; 主程式和所有 DLL
Source: "..\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 設定檔範例（保留供參考）
Source: "..\config\config.example.json"; DestDir: "{app}"; Flags: ignoreversion
; 文件檔案
Source: "..\docs\*.md"; DestDir: "{app}\docs"; Flags: ignoreversion
; n8n workflow
Source: "..\workflows\*.json"; DestDir: "{app}\workflows"; Flags: ignoreversion

[Dirs]
; 不需要在安裝目錄建立 Logs 資料夾，會自動建立在 %APPDATA%

[Icons]
; 開始功能表捷徑
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\設定"; Filename: "notepad.exe"; Parameters: """{app}\config.json"""
Name: "{group}\開啟日誌資料夾"; Filename: "{app}\Logs"
Name: "{group}\README"; Filename: "{app}\docs\README.md"
Name: "{group}\解除安裝 {#MyAppName}"; Filename: "{uninstallexe}"

; 桌面捷徑
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; 開機自動啟動捷徑 (使用 commonstartup 以配合 admin 權限)
Name: "{commonstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: autostart

[Registry]
; 記錄安裝資訊
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppEnglishName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppEnglishName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

[Run]
; 安裝完成後執行
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 解除安裝時刪除所有檔案和資料夾
Type: filesandordirs; Name: "{app}"

[Code]
var
  DotNetInstalled: Boolean;

// ============================================================================
// 檢查 .NET Runtime 是否已安裝
// ============================================================================
function IsDotNetInstalled(): Boolean;
var
  FindRec: TFindRec;
  DotNetPath: String;
  VersionFound: Boolean;
begin
  Result := False;
  VersionFound := False;

  // 檢查 .NET 8.0 Desktop Runtime
  DotNetPath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App');

  if DirExists(DotNetPath) then
  begin
    // 尋找所有 8.* 版本資料夾
    if FindFirst(DotNetPath + '\8*', FindRec) then
    begin
      try
        repeat
          // 只檢查資料夾,且名稱以 "8." 開頭
          if ((FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0) and
             (Copy(FindRec.Name, 1, 2) = '8.') then
          begin
            VersionFound := True;
            Break;
          end;
        until not FindNext(FindRec);
      finally
        FindClose(FindRec);
      end;
    end;
  end;

  Result := VersionFound;
end;

// ============================================================================
// 初始化設定
// ============================================================================
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  // 檢查 .NET Runtime
  DotNetInstalled := IsDotNetInstalled();

  if not DotNetInstalled then
  begin
    if MsgBox(CustomMessage('DotNetNotInstalled'), mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0', '', '', SW_SHOW, ewNoWait, ResultCode);
    end;
    Result := False;
  end;
end;

// ============================================================================
// 建立解除安裝確認頁面
// ============================================================================
procedure InitializeUninstallProgressForm();
begin
  UninstallProgressForm.Caption := '解除安裝 ' + '{#MyAppName}';
  UninstallProgressForm.StatusLabel.Caption := '正在移除檔案...';
end;

// ============================================================================
// 解除安裝前的確認
// ============================================================================
function InitializeUninstall(): Boolean;
var
  KeepUserData: Integer;
  AppDataPath: String;
begin
  Result := True;

  // 使用者資料路徑（設定檔和日誌）
  AppDataPath := ExpandConstant('{userappdata}\NASFileWatcher');

  // 詢問是否刪除使用者資料（設定檔和日誌）
  if DirExists(AppDataPath) then
  begin
    KeepUserData := MsgBox('Do you want to remove all user data (settings and logs)?' + #13#10 +
                           'If you plan to reinstall, you may want to keep them.' + #13#10#13#10 +
                           'User data location: ' + AppDataPath,
                           mbConfirmation, MB_YESNO);
    if KeepUserData = IDNO then
    begin
      // 刪除整個使用者資料夾
      DelTree(AppDataPath, True, True, True);
    end;
  end;
end;

// ============================================================================
// 安裝完成後的處理
// ============================================================================
procedure CurStepChanged(CurStep: TSetupStep);
begin
  // 不需要特別處理，config.json 會在程式首次執行時自動建立在 %APPDATA%
  if CurStep = ssPostInstall then
  begin
    // 保留空實作，未來可能需要
  end;
end;

// ============================================================================
// 解除安裝完成後的處理
// ============================================================================
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  StartupPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // 移除啟動資料夾中的捷徑
    StartupPath := ExpandConstant('{commonstartup}\{#MyAppName}.lnk');
    if FileExists(StartupPath) then
    begin
      DeleteFile(StartupPath);
    end;
  end;
end;
