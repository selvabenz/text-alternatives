$ErrorActionPreference = "Continue"
Write-Host "Tamil Divine Name - Build Prerequisite Check v0.1.3"
Write-Host "=================================================="
Write-Host ""

$ptxCandidates = @("C:\Program Files\Paratext 9", "C:\Program Files (x86)\Paratext 9")
$ptx = $null
foreach ($p in $ptxCandidates) {
  if (Test-Path (Join-Path $p "PluginInterfaces.dll")) { $ptx = $p; break }
}
if ($ptx) {
  Write-Host "[PASS] Paratext interface DLLs: $ptx" -ForegroundColor Green
  foreach ($dll in @("PluginInterfaces.dll","CorePluginInterfaces.dll","EmbeddedUiPluginInterfaces.dll")) {
    $fp=Join-Path $ptx $dll
    if (Test-Path $fp) { Write-Host "       $dll  $([Diagnostics.FileVersionInfo]::GetVersionInfo($fp).FileVersion)" }
    else { Write-Host "[FAIL] Missing $fp" -ForegroundColor Red }
  }
} else { Write-Host "[FAIL] Paratext interface DLLs not found." -ForegroundColor Red }

$csc=@("$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe","$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe") | Where-Object {Test-Path $_} | Select-Object -First 1
if ($csc) { Write-Host "[PASS] C# compiler: $csc" -ForegroundColor Green } else { Write-Host "[FAIL] C# compiler not found." -ForegroundColor Red }

$roots=@(
 "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework",
 "C:\Program Files\Reference Assemblies\Microsoft\Framework",
 "$env:WINDIR\Microsoft.NET\Framework64",
 "$env:WINDIR\Microsoft.NET\Framework",
 "$env:USERPROFILE\.nuget\packages\netstandard.library",
 "$env:USERPROFILE\.nuget\packages\netstandard.library.ref",
 "$env:ProgramFiles\dotnet\packs\NETStandard.Library.Ref",
 "${env:ProgramFiles(x86)}\dotnet\packs\NETStandard.Library.Ref",
 $(if($ptx){$ptx})
) | Where-Object {$_ -and (Test-Path $_)} | Select-Object -Unique
$found=$null
foreach($r in $roots){ try{$found=Get-ChildItem $r -Filter netstandard.dll -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1;if($found){break}}catch{} }
if($found){Write-Host "[PASS] netstandard: $($found.FullName)" -ForegroundColor Green}
else{
 Write-Host "[INFO] No local netstandard.dll found." -ForegroundColor Yellow
 Write-Host "       v0.1.3 will try to download official NETStandard.Library 2.0.3" -ForegroundColor Yellow
 Write-Host "       to .build-deps when the build runs." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Next: BUILD-WITH-AUTO-DEPENDENCY.cmd"
Write-Host ""
Write-Host "Press any key to close..."
$null=$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
