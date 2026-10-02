param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9",
    [switch]$Elevated
)

$ErrorActionPreference = "Stop"

function Is-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

try {
    $root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
    $plugin = Join-Path $root "dist\TamilDivineName.ptxplg"

    if (!(Test-Path $plugin)) {
        throw "Plugin has not been built: $plugin"
    }

    if (!(Test-Path $ParatextInstallDir)) {
        throw "Paratext folder not found: $ParatextInstallDir"
    }

    if (-not (Is-Administrator)) {
        Write-Host "Installation into Program Files requires Administrator permission."
        Write-Host "Windows will now show a User Account Control prompt."

        $argList = @(
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", "`"$PSCommandPath`"",
            "-ParatextInstallDir", "`"$ParatextInstallDir`"",
            "-Elevated"
        )

        $p = Start-Process powershell.exe -Verb RunAs -Wait -PassThru -ArgumentList $argList
        exit $p.ExitCode
    }

    $dest = Join-Path $ParatextInstallDir "plugins\TamilDivineName"
    New-Item -ItemType Directory -Force -Path $dest | Out-Null

    $destFile = Join-Path $dest "TamilDivineName.ptxplg"
    Copy-Item $plugin $destFile -Force

    if (!(Test-Path $destFile)) {
        throw "Copy completed without an exception, but the installed plugin was not found: $destFile"
    }

    Write-Host ""
    Write-Host "INSTALL SUCCESS" -ForegroundColor Green
    Write-Host "Installed:"
    Write-Host "  $destFile"
    Write-Host ""
    Write-Host "Close and restart Paratext 9.5.110.1."
    exit 0
}
catch {
    Write-Host ""
    Write-Host "INSTALL FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_
    exit 1
}
