@echo off
setlocal
cd /d "%~dp0\.."
cls
echo Tamil Divine Name - INSTALL ONLY
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\build\install-paratext95.ps1"
set "EC=%ERRORLEVEL%"
echo.
if "%EC%"=="0" (
  echo INSTALL SUCCESS
) else (
  echo INSTALL FAILED
)
echo.
pause
exit /b %EC%
