@echo off
setlocal
cd /d "%~dp0\.."
cls
echo ============================================================
echo Tamil Divine Name - Auto Dependency Build + Install
echo ============================================================
echo.
echo The builder will search your computer for netstandard.dll.
echo If it is missing, it will try a one-time download of the
echo official NETStandard.Library 2.0.3 reference package into:
echo.
echo   %CD%\.build-deps
echo.
echo It does NOT install that package into Windows or Paratext.
echo.
pause
call "%CD%\build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd"
