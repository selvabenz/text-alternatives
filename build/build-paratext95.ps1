param(
    [string]$ParatextInstallDir = "C:\Program Files\Paratext 9"
)

$ErrorActionPreference = "Stop"

function Step($message) {
    Write-Host ""
    Write-Host "============================================================"
    Write-Host $message
    Write-Host "============================================================"
}

function Find-ParatextDir([string]$requested) {
    $candidates = @(
        $requested,
        (Join-Path $env:ProgramFiles "Paratext 9"),
        $(if (${env:ProgramFiles(x86)}) { Join-Path ${env:ProgramFiles(x86)} "Paratext 9" })
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

    foreach ($candidate in $candidates) {
        if ((Test-Path (Join-Path $candidate "PluginInterfaces.dll")) -and
            (Test-Path (Join-Path $candidate "CorePluginInterfaces.dll")) -and
            (Test-Path (Join-Path $candidate "EmbeddedUiPluginInterfaces.dll"))) {
            return $candidate
        }
    }

    throw @"
Paratext 9 plugin interface DLLs were not found.

Checked:
  $requested
  $env:ProgramFiles\Paratext 9
  ${env:ProgramFiles(x86)}\Paratext 9

Expected:
  PluginInterfaces.dll
  CorePluginInterfaces.dll
  EmbeddedUiPluginInterfaces.dll

If Paratext is installed elsewhere, run:
  powershell -ExecutionPolicy Bypass -File build-paratext95.ps1 -ParatextInstallDir "D:\Your\Paratext 9"
"@
}

function Find-Csc {
    $candidates = @(
        "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
        "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
    )
    foreach ($p in $candidates) {
        if (Test-Path $p) { return $p }
    }
    throw ".NET Framework C# compiler (csc.exe) was not found."
}

function Find-NetStandardDll([string]$ParatextInstallDir, [string]$ProjectRoot) {
    $exactCandidates = @(
        "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll",
        "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll",
        "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll",
        "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll",
        "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\Facades\netstandard.dll",
        "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\Facades\netstandard.dll",
        (Join-Path $ParatextInstallDir "netstandard.dll"),
        (Join-Path $ProjectRoot ".build-deps\NETStandard.Library.2.0.3\build\netstandard2.0\ref\netstandard.dll")
    ) | Where-Object { $_ }

    Write-Host "Searching standard netstandard.dll locations..."
    foreach ($candidate in $exactCandidates) {
        if (Test-Path $candidate) {
            Write-Host "Found netstandard.dll: $candidate"
            return $candidate
        }
    }

    $searchRoots = @(
        $ParatextInstallDir,
        "$env:USERPROFILE\.nuget\packages\netstandard.library",
        "$env:USERPROFILE\.nuget\packages\netstandard.library.ref",
        "$env:ProgramFiles\dotnet\packs\NETStandard.Library.Ref",
        "${env:ProgramFiles(x86)}\dotnet\packs\NETStandard.Library.Ref",
        "$env:WINDIR\Microsoft.NET\Framework64",
        "$env:WINDIR\Microsoft.NET\Framework",
        "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework",
        "C:\Program Files\Reference Assemblies\Microsoft\Framework",
        (Join-Path $ProjectRoot ".build-deps")
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

    Write-Host "Searching Paratext, NuGet cache, .NET SDK packs, and local build dependencies..."
    foreach ($searchRoot in $searchRoots) {
        try {
            $hits = Get-ChildItem -Path $searchRoot -Filter netstandard.dll -Recurse -ErrorAction SilentlyContinue
            if ($hits) {
                $preferred = $hits |
                    Sort-Object @{ Expression = {
                        if ($_.FullName -match "netstandard2\.0") { 0 }
                        elseif ($_.FullName -match "\\Facades\\") { 1 }
                        elseif ($_.FullName -match "netstandard2\.1") { 3 }
                        else { 2 }
                    }}, FullName |
                    Select-Object -First 1
                if ($preferred) {
                    Write-Host "Found netstandard.dll: $($preferred.FullName)"
                    return $preferred.FullName
                }
            }
        }
        catch { }
    }

    return $null
}

function Bootstrap-NetStandard([string]$ProjectRoot) {
    $version = "2.0.3"
    $depsRoot = Join-Path $ProjectRoot ".build-deps"
    $extractDir = Join-Path $depsRoot ("NETStandard.Library." + $version)
    $expected = Join-Path $extractDir "build\netstandard2.0\ref\netstandard.dll"

    if (Test-Path $expected) { return $expected }

    New-Item -ItemType Directory -Force -Path $depsRoot | Out-Null
    $nupkg = Join-Path $depsRoot ("NETStandard.Library." + $version + ".nupkg")

    Write-Host ""
    Write-Host "netstandard.dll is not installed locally."
    Write-Host "Attempting a one-time download of the official NETStandard.Library $version reference package from NuGet..."
    Write-Host "It will be kept only under the project's .build-deps folder."

    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $url = "https://www.nuget.org/api/v2/package/NETStandard.Library/$version"
        Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $nupkg

        if (Test-Path $extractDir) { Remove-Item $extractDir -Recurse -Force }
        New-Item -ItemType Directory -Force -Path $extractDir | Out-Null

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($nupkg, $extractDir)

        if (Test-Path $expected) {
            Write-Host "Downloaded build reference: $expected"
            return $expected
        }

        $hit = Get-ChildItem -Path $extractDir -Filter netstandard.dll -Recurse -ErrorAction SilentlyContinue |
               Where-Object { $_.FullName -match "netstandard2\.0" } |
               Select-Object -First 1
        if ($hit) {
            Write-Host "Downloaded build reference: $($hit.FullName)"
            return $hit.FullName
        }
    }
    catch {
        Write-Warning ("Automatic NuGet bootstrap failed: " + $_.Exception.Message)
    }
    return $null
}

try {
    Step "Tamil Divine Name plugin - Paratext 9.5 diagnostic build"

    $root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
    $src = Join-Path $root "src"
    $out = Join-Path $root "dist"
    New-Item -ItemType Directory -Force -Path $out | Out-Null

    $ParatextInstallDir = Find-ParatextDir $ParatextInstallDir
    $csc = Find-Csc
    $netstandard = Find-NetStandardDll $ParatextInstallDir $root
    if (!$netstandard) { $netstandard = Bootstrap-NetStandard $root }
    if (!$netstandard) {
        throw @"
netstandard.dll is not installed locally and the automatic official NuGet bootstrap failed.

Supported options:
  1. Connect this computer to the internet and run the diagnostic launcher again.
  2. Install the Microsoft .NET Framework 4.8 Developer Pack.

No plugin source has been compiled yet.
"@
    }
    $facadesDir = Split-Path -Parent $netstandard

    $required = @(
        (Join-Path $ParatextInstallDir "PluginInterfaces.dll"),
        (Join-Path $ParatextInstallDir "CorePluginInterfaces.dll"),
        (Join-Path $ParatextInstallDir "EmbeddedUiPluginInterfaces.dll")
    )

    Write-Host "Paratext folder : $ParatextInstallDir"

    $paratextExe = Join-Path $ParatextInstallDir "Paratext.exe"
    if (Test-Path $paratextExe) {
        $pv = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($paratextExe).FileVersion
        Write-Host "Paratext version: $pv"
        if ($pv -and -not $pv.StartsWith("9.5.110.1")) {
            Write-Warning "This package was prepared for Paratext 9.5.110.1. Detected: $pv"
        }
    } else {
        Write-Warning "Paratext.exe was not found, but the plugin interface DLLs were found."
    }

    foreach ($dll in $required) {
        $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll)
        Write-Host ("Interface DLL   : {0}  [{1}]" -f (Split-Path $dll -Leaf), $info.FileVersion)
    }

    Write-Host "Compiler        : $csc"
    try {
        $compilerInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($csc)
        Write-Host "Compiler file version: $($compilerInfo.FileVersion)"
        Write-Host "Language target : C# 5 compatible source"
    } catch { }

    Write-Host "Facade folder   : $facadesDir"

    $sources = Get-ChildItem $src -Filter *.cs |
        Where-Object { $_.Name -notlike "*.Designer.cs" } |
        ForEach-Object { $_.FullName }

    if (!$sources -or $sources.Count -eq 0) {
        throw "No C# source files found in $src"
    }

    $target = Join-Path $out "TamilDivineName.ptxplg"
    $rsp = Join-Path $out "compiler.rsp"

    # Using a response file avoids Windows command-line quoting/length problems.
    $refs = @(
        $required[0],
        $required[1],
        $required[2],
        $netstandard
    )

    # Add the most common .NET Standard facade dependencies when present.
    $facadeNames = @(
        "System.Runtime.dll",
        "System.Collections.dll",
        "System.ComponentModel.dll",
        "System.Linq.dll",
        "System.ObjectModel.dll",
        "System.Reflection.dll",
        "System.Resources.ResourceManager.dll",
        "System.Runtime.Extensions.dll",
        "System.Threading.dll"
    )
    foreach ($name in $facadeNames) {
        $p = Join-Path $facadesDir $name
        if (Test-Path $p) { $refs += $p }
    }

    $lines = @(
        "/nologo",
        "/target:library",
        "/platform:anycpu",
        "/langversion:5",
        "/optimize+",
        "/out:`"$target`"",
        "/reference:System.dll",
        "/reference:System.Core.dll",
        "/reference:System.Drawing.dll",
        "/reference:System.Windows.Forms.dll",
        "/reference:System.Web.Extensions.dll",
        "/reference:System.Security.dll"
    )

    foreach ($r in $refs) {
        $lines += "/reference:`"$r`""
    }
    foreach ($s in $sources) {
        $lines += "`"$s`""
    }

    # ASCII-compatible response file. Source files themselves remain UTF-8.
    [System.IO.File]::WriteAllLines($rsp, $lines, (New-Object System.Text.UTF8Encoding($false)))

    Step "Compiling"
    Write-Host "Response file   : $rsp"
    Write-Host "Output          : $target"
    Write-Host ""

    & $csc "@$rsp"
    $compileExit = $LASTEXITCODE

    if ($compileExit -ne 0) {
        throw "Compilation failed with exit code $compileExit. The source is compiled in C# 5 mode for the legacy .NET Framework compiler; see the first compiler error above."
    }

    if (!(Test-Path $target)) {
        throw "Compiler reported success, but plugin file was not produced: $target"
    }

    Step "BUILD SUCCESS"
    Write-Host "Plugin created:"
    Write-Host "  $target"
    Write-Host ""
    Write-Host "The build step does not modify the live Paratext Scripture project."
    exit 0
}
catch {
    Write-Host ""
    Write-Host "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" -ForegroundColor Red
    Write-Host "BUILD FAILED" -ForegroundColor Red
    Write-Host "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Full exception:"
    Write-Host $_
    exit 1
}
