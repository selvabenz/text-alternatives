@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-paratext95.ps1"
if errorlevel 1 exit /b %errorlevel%
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-paratext95.ps1"
