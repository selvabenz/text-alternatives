param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9"
)
$dest = Join-Path $ParatextInstallDir "plugins\TamilDivineName"
if (Test-Path $dest) {
    Remove-Item $dest -Recurse -Force
    Write-Host "Removed $dest"
}
