$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$src = Join-Path $root "src"

Write-Host "Checking source for syntax that the Paratext machine's C# 5 compiler cannot parse..."

$bad = @()

foreach ($file in Get-ChildItem $src -Filter *.cs) {
    $lineNo = 0
    foreach ($line in Get-Content $file.FullName) {
        $lineNo++

        # C# 6+ string interpolation is introduced by a string prefix:
        #   $"..."
        #   $@"..."
        #   @$"..."
        # Do NOT simply search for $" anywhere, because a valid C# 5
        # verbatim regex can end with a regex anchor like @"...\s*$".
        if ($line -match '(^|[=(:,;\[\]\{\}\s])\s*\$@"' -or
            $line -match '(^|[=(:,;\[\]\{\}\s])\s*@\$"' -or
            $line -match '(^|[=(:,;\[\]\{\}\s])\s*\$"') {
            $bad += "$($file.Name):$lineNo interpolated string"
        }

        if ($line -match '\bnameof\s*\(') {
            $bad += "$($file.Name):$lineNo nameof"
        }

        if ($line -match '\?\.' ) {
            $bad += "$($file.Name):$lineNo null-conditional operator"
        }

        if ($line -match '\?\?=') {
            $bad += "$($file.Name):$lineNo null-coalescing assignment"
        }

        if ($line -match '\bout\s+var\b') {
            $bad += "$($file.Name):$lineNo out-var declaration"
        }
    }
}

if ($bad.Count -gt 0) {
    Write-Host "C# 5 compatibility check FAILED:" -ForegroundColor Red
    $bad | ForEach-Object { Write-Host "  $_" }
    exit 1
}

Write-Host "PASS: no known C# 6+ syntax blockers were detected." -ForegroundColor Green
Write-Host "The actual C# compiler remains the final compatibility check."
exit 0
