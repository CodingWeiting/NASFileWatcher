@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║         NASFileWatcher 安裝程式建置工具 v1.0.1                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

REM 設定變數
set VERSION=1.0.1
set CONFIGURATION=Release

REM 取得專案根目錄（上一層）
set PROJECT_ROOT=%~dp0..
set SETUP_DIR=%~dp0
set ISS_FILE=%SETUP_DIR%NASFileWatcher-Setup.iss
set BUILD_DIR=%PROJECT_ROOT%\bin\%CONFIGURATION%\net8.0-windows
set OUTPUT_DIR=%SETUP_DIR%Output

REM Inno Setup 路徑
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

REM ============================================================================
REM 步驟 1: 檢查環境
REM ============================================================================
echo [1/4] 檢查編譯環境...
echo.

REM 檢查 .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ✗ .NET SDK 未安裝！
    echo.
    echo 請從以下網址下載並安裝 .NET SDK:
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

for /f "delims=" %%v in ('dotnet --version') do set DOTNET_VERSION=%%v
echo ✓ .NET SDK 版本: %DOTNET_VERSION%

REM 檢查 Inno Setup
if not exist %ISCC% (
    echo ✗ Inno Setup 未安裝！
    echo.
    echo 請從以下網址下載並安裝 Inno Setup:
    echo https://jrsoftware.org/isdl.php
    echo.
    pause
    exit /b 1
)
echo ✓ Inno Setup 已安裝

REM ============================================================================
REM 步驟 2: 清理舊檔案
REM ============================================================================
echo.
echo [2/4] 清理舊的編譯檔案...
echo.

pushd "%PROJECT_ROOT%"
dotnet clean -c %CONFIGURATION% >nul 2>&1
if errorlevel 1 (
    echo ⚠ 清理時發生警告，繼續編譯...
) else (
    echo ✓ 清理完成
)
popd

REM ============================================================================
REM 步驟 3: 編譯專案
REM ============================================================================
echo.
echo [3/4] 編譯專案 (%CONFIGURATION%)...
echo.

pushd "%PROJECT_ROOT%"
dotnet build -c %CONFIGURATION%

if errorlevel 1 (
    echo.
    echo ✗ 編譯失敗！
    echo.
    popd
    pause
    exit /b 1
)
popd

echo.
echo ✓ 專案編譯完成

REM 檢查輸出資料夾
if not exist "%BUILD_DIR%" (
    echo ✗ 找不到建置資料夾: %BUILD_DIR%
    pause
    exit /b 1
)

REM 計算檔案數量
set FILE_COUNT=0
for %%f in ("%BUILD_DIR%\*.*") do set /a FILE_COUNT+=1
echo ✓ 建置資料夾包含 %FILE_COUNT% 個檔案

REM ============================================================================
REM 步驟 4: 編譯安裝程式
REM ============================================================================
echo.
echo [4/4] 編譯 Inno Setup 安裝程式...
echo.

%ISCC% "%ISS_FILE%"

if errorlevel 1 (
    echo.
    echo ✗ Inno Setup 編譯失敗！
    echo.
    pause
    exit /b 1
)

echo.
echo ✓ 安裝程式編譯完成

REM ============================================================================
REM 顯示結果
REM ============================================================================
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                    建置成功! ✓                                 ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

REM 找到安裝檔案
for %%f in ("%OUTPUT_DIR%\NASFileWatcher-Setup-v*.exe") do (
    set SETUP_EXE=%%~ff
    set SETUP_NAME=%%~nxf
    set SETUP_SIZE=%%~zf
)

if defined SETUP_EXE (
    REM 計算檔案大小 (MB)
    set /a SIZE_MB=!SETUP_SIZE! / 1048576

    echo 安裝檔案: !SETUP_NAME!
    echo 檔案大小: !SIZE_MB! MB
    echo 輸出位置: !SETUP_EXE!
    echo.

    REM 詢問是否開啟輸出資料夾
    set /p OPEN_DIR="是否要開啟輸出資料夾? (Y/N): "
    if /i "!OPEN_DIR!"=="Y" (
        explorer.exe "%OUTPUT_DIR%"
    )

    echo.
    REM 詢問是否執行安裝程式
    set /p RUN_SETUP="是否要執行安裝程式進行測試? (Y/N): "
    if /i "!RUN_SETUP!"=="Y" (
        start "" "!SETUP_EXE!"
    )
) else (
    echo ✗ 找不到安裝檔案！
    pause
    exit /b 1
)

echo.
echo 建置流程結束。
echo.
pause
exit /b 0
