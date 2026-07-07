@echo off
setlocal
cd /d "%~dp0"

rem Quick launcher: starter.bat [Release]
rem Always rebuilds before launch so code fixes are never skipped.

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

set "PROJECT=src\HorosPulse.App\HorosPulse.App.csproj"
set "EXE=src\HorosPulse.App\bin\%CONFIG%\net9.0-windows\HorosPulse.App.exe"

echo.
echo HorosPulse (%CONFIG%)
echo.

rem Stop a running instance so locked DLLs do not block rebuild.
tasklist /FI "IMAGENAME eq HorosPulse.App.exe" 2>nul | find /I "HorosPulse.App.exe" >nul
if not errorlevel 1 (
    echo Stopping running HorosPulse instance...
    taskkill /IM HorosPulse.App.exe /F >nul 2>&1
    timeout /t 2 /nobreak >nul
)

echo Building...
dotnet build "%PROJECT%" -c %CONFIG%
if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

if not exist "%EXE%" (
    echo.
    echo [ERROR] Expected executable not found: %EXE%
    pause
    exit /b 1
)

echo Launching...
start "" "%EXE%"
exit /b 0
