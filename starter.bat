@echo off
setlocal
cd /d "%~dp0"

rem Quick launcher: starter.bat [Release]
rem Uses pre-built exe when present, otherwise dotnet run.

set "CONFIG=Debug"
if /I "%~1"=="Release" set "CONFIG=Release"
if /I "%~1"=="-c" if /I "%~2"=="Release" set "CONFIG=Release"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo.
    echo [ERROR] dotnet was not found in PATH.
    echo Install the .NET 9 SDK: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

set "PROJECT=src\WindowsPerformance.App\WindowsPerformance.App.csproj"
set "EXE=src\WindowsPerformance.App\bin\%CONFIG%\net9.0-windows\WindowsPerformance.App.exe"
set "PUBLISHED=artifacts\WindowsPerformance-0.1.0-win-x64\WindowsPerformance.App.exe"

echo.
echo WindowsPerformance (%CONFIG%)
echo.

if exist "%EXE%" (
    echo Launching built app...
    start "" "%EXE%"
    exit /b 0
)

if /I "%CONFIG%"=="Release" if exist "%PUBLISHED%" (
    echo Launching published portable build...
    start "" "%PUBLISHED%"
    exit /b 0
)

echo No build found — starting via dotnet run...
echo.
dotnet run --project "%PROJECT%" -c %CONFIG%
if errorlevel 1 (
    echo.
    echo [ERROR] Failed to start WindowsPerformance.
    pause
    exit /b 1
)
