param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$plugin = Join-Path $root "dist\TamilDivineName.ptxplg"
if (!(Test-Path $plugin)) { throw "Build first: $plugin not found." }

$dest = Join-Path $ParatextInstallDir "plugins\TamilDivineName"
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $plugin (Join-Path $dest "TamilDivineName.ptxplg") -Force

Write-Host "Installed to $dest"
Write-Host "Restart Paratext 9.5."
