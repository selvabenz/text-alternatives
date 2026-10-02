param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$build = Join-Path $root "build\build-paratext95.ps1"
& $build -ParatextInstallDir $ParatextInstallDir

$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (!(Test-Path $csc)) { $csc = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" }

$dist = Join-Path $root "dist"
$testExe = Join-Path $dist "AlgorithmSelfTest.exe"
$plugin = Join-Path $dist "TamilDivineName.ptxplg"

& $csc /nologo /target:exe /out:$testExe /reference:$plugin (Join-Path $root "tests\AlgorithmSelfTest.cs")
if ($LASTEXITCODE -ne 0) { throw "Test compile failed." }

& $testExe
exit $LASTEXITCODE
