param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$src = Join-Path $root "src"
$out = Join-Path $root "dist"
New-Item -ItemType Directory -Force -Path $out | Out-Null

$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (!(Test-Path $csc)) {
    $csc = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (!(Test-Path $csc)) { throw "C# compiler not found." }

$required = @(
    (Join-Path $ParatextInstallDir "PluginInterfaces.dll"),
    (Join-Path $ParatextInstallDir "CorePluginInterfaces.dll"),
    (Join-Path $ParatextInstallDir "EmbeddedUiPluginInterfaces.dll")
)
foreach ($p in $required) {
    if (!(Test-Path $p)) { throw "Missing Paratext interface assembly: $p" }
}

$facadeCandidates = @(
    "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll",
    "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll",
    "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll"
)
$netstandard = $facadeCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (!$netstandard) {
    throw "netstandard.dll facade not found. Install the .NET Framework 4.8 Developer Pack."
}

$sources = Get-ChildItem $src -Filter *.cs | ForEach-Object { $_.FullName }
$target = Join-Path $out "TamilDivineName.ptxplg"

Write-Host "Paratext: $ParatextInstallDir"
Write-Host "Compiler: $csc"
Write-Host "netstandard facade: $netstandard"
Write-Host "Output: $target"

$args = @(
    "/nologo",
    "/target:library",
    "/platform:anycpu",
    "/langversion:7.3",
    "/optimize+",
    "/out:$target",
    "/reference:$($required[0])",
    "/reference:$($required[1])",
    "/reference:$($required[2])",
    "/reference:$netstandard",
    "/reference:System.dll",
    "/reference:System.Core.dll",
    "/reference:System.Drawing.dll",
    "/reference:System.Windows.Forms.dll",
    "/reference:System.Web.Extensions.dll",
    "/reference:System.Security.dll"
) + $sources

& $csc $args
if ($LASTEXITCODE -ne 0) { throw "Compilation failed with exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "SUCCESS: $target"
