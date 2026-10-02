@echo off
setlocal
cd /d "%~dp0\.."
set "LOG=%CD%\build-only.log"

cls
echo Tamil Divine Name - BUILD ONLY
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\build\CHECK-CSharp5-SOURCE.ps1" >> "%LOG%" 2>&1
if not "%ERRORLEVEL%"=="0" (
  type "%LOG%"
  echo.
  echo C# 5 SOURCE CHECK FAILED
  pause
  exit /b 1
)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\build\build-paratext95.ps1" > "%LOG%" 2>&1
set "EC=%ERRORLEVEL%"
type "%LOG%"
echo.
if "%EC%"=="0" (
  echo BUILD SUCCESS
  echo Plugin: %CD%\dist\TamilDivineName.ptxplg
) else (
  echo BUILD FAILED
  echo Please send me: %LOG%
)
echo.
pause
exit /b %EC%
