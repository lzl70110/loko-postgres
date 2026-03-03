# scan-keys.ps1
# Usage: .\Tools\KeysAudit\scan-keys.ps1 <solutionRoot>
# Scans Razor/C# for L["..."] keys + attribute keys, and .resx data names.
param(
    [Parameter(Mandatory = $false)]
    [string]$Root = "."
)

# Resolve root
$rootPath = Resolve-Path $Root

# Collect used keys from .cshtml / .cs
$used = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
# Collect resx keys
$resx = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)

# Regex patterns
$rxLocalizer = [regex]'(@L|\b_L)\s*\[\s*"([^"]+)"\s*\]'
$rxAttr      = [regex]'\b(Name|ErrorMessage(Resource(Name)?)?)\s*=\s*"([^"]+)"'
$rxResx      = [regex]'<data\s+name="([^"]+)"'

# Scan code files
$codeFiles = Get-ChildItem -Path $rootPath -Recurse -Include *.cs, *.cshtml -File
foreach ($f in $codeFiles) {
    $text = Get-Content -LiteralPath $f.FullName -Raw

    foreach ($m in $rxLocalizer.Matches($text)) {
        [void]$used.Add($m.Groups[2].Value)
    }
    foreach ($m in $rxAttr.Matches($text)) {
        [void]$used.Add($m.Groups[$m.Groups.Count-1].Value)
    }
}

# Scan resx files
$resxFiles = Get-ChildItem -Path $rootPath -Recurse -Include *.resx -File
foreach ($f in $resxFiles) {
    $text = Get-Content -LiteralPath $f.FullName -Raw
    foreach ($m in $rxResx.Matches($text)) {
        [void]$resx.Add($m.Groups[1].Value)
    }
}

# Compute reports
$missing = $used | Where-Object { -not $resx.Contains($_) } | Sort-Object
$unused  = $resx | Where-Object { -not $used.Contains($_) } | Sort-Object
$legacy  = ($used + $resx) | Sort-Object -Unique | Where-Object { $_ -match '[ .]' }

# Write reports in solution root
$missing | Out-File -Encoding utf8 "$rootPath\keys_missing.txt"
$unused  | Out-File -Encoding utf8 "$rootPath\keys_unused.txt"
$legacy  | Out-File -Encoding utf8 "$rootPath\keys_legacy.txt"

Write-Host "Done."
Write-Host "missing: $($missing.Count), unused: $($unused.Count), legacy: $((($legacy | Measure-Object).Count))"