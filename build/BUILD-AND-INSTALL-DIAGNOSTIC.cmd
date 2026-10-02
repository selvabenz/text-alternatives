@echo off
setlocal EnableExtensions
cd /d "%~dp0\.."

set "ROOT=%CD%"
set "LOG=%ROOT%\build-install.log"

cls
echo ============================================================
echo Tamil Divine Name - Paratext 9.5.110.1
echo Diagnostic Build and Install
echo ============================================================
echo.
echo This window will remain open even if the build fails.
echo A complete log will be saved to:
echo   %LOG%
echo.
echo Started: %DATE% %TIME%
echo ============================================================ > "%LOG%"
echo Started: %DATE% %TIME% >> "%LOG%"
echo Root: %ROOT% >> "%LOG%"
echo. >> "%LOG%"

echo Checking C# 5 source compatibility...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\build\CHECK-CSharp5-SOURCE.ps1" >> "%LOG%" 2>&1
if not "%ERRORLEVEL%"=="0" (
    type "%LOG%"
    echo.
    echo SOURCE COMPATIBILITY CHECK FAILED.
    echo Please send me: %LOG%
    echo.
    pause
    exit /b 1
)
echo.

echo [1/2] Building plugin...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\build\build-paratext95.ps1" >> "%LOG%" 2>&1
set "BUILD_EXIT=%ERRORLEVEL%"

type "%LOG%"

if not "%BUILD_EXIT%"=="0" (
    echo.
    echo ============================================================
    echo BUILD FAILED - installation was NOT attempted.
    echo ============================================================
    echo.
    echo Please send me this file:
    echo   %LOG%
    echo.
    pause
    exit /b %BUILD_EXIT%
)

echo.
echo [2/2] Installing plugin...
echo Windows may ask for Administrator permission.
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\build\install-paratext95.ps1"
set "INSTALL_EXIT=%ERRORLEVEL%"

echo. >> "%LOG%"
echo Install exit code: %INSTALL_EXIT% >> "%LOG%"

if not "%INSTALL_EXIT%"=="0" (
    echo.
    echo ============================================================
    echo INSTALL FAILED.
    echo ============================================================
    echo Build succeeded, so the plugin should exist here:
    echo   %ROOT%\dist\TamilDivineName.ptxplg
    echo.
    echo Please send me:
    echo   %LOG%
    echo.
    pause
    exit /b %INSTALL_EXIT%
)

echo.
echo ============================================================
echo BUILD AND INSTALL SUCCESSFUL
echo ============================================================
echo.
echo Restart Paratext 9.5.110.1, then open a Scripture project.
echo Look under the Scripture Text tools menu for:
echo   Tamil Divine Name...
echo.
pause
exit /b 0
