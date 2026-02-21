param(
  [string]$Root,
  [string]$OutZip
)

# ---------------- Diagnostics ----------------
Write-Host "=== Pack-Loco.ps1 ==="
Write-Host "Incoming Root param  : '$Root'"
Write-Host "Incoming OutZip param: '$OutZip'"

# If no Root passed → use the folder where the script resides
if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = $PSScriptRoot
    Write-Host "Root not supplied → using PSScriptRoot: '$Root'"
}

# Normalize and validate Root
try {
    $Root = (Resolve-Path -LiteralPath $Root).Path
} catch {
    Write-Error "Root path is invalid: '$Root' → $($_.Exception.Message)"
    exit 2
}

# Default OutZip if not supplied
if ([string]::IsNullOrWhiteSpace($OutZip)) {
    $OutZip = "Loco_17_Feb_clean.zip"
}
# Build absolute OutZip under Root
$OutZip = Join-Path -Path $Root -ChildPath $OutZip

Write-Host "Resolved Root        : '$Root'"
Write-Host "Resolved OutZip      : '$OutZip'"

# ---------------- Selection rules ----------------
$includeExt = @(
  ".sln",".csproj",".props",".targets",
  ".cs",".cshtml",".razor",
  ".json",".resx",".config",
  ".txt",".md",".ico",".js",".css",".scss",".svg"
)
$excludeDirs = @("bin","obj",".git",".vs","node_modules","publish")

# Collect files robustly
$files = Get-ChildItem -LiteralPath $Root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $rel = $_.FullName.Substring($Root.Length).TrimStart('\','/')
        -not ($excludeDirs | ForEach-Object { $rel -match "(^|[\\/])$_([\\/]|$)" }) -and
        ($includeExt -contains $_.Extension.ToLowerInvariant() -or $_.Extension -eq "")
    }

Write-Host ("Files selected       : {0}" -f $files.Count)

# Ensure output folder exists
$zipDir = Split-Path -Path $OutZip -Parent
if (-not (Test-Path -LiteralPath $zipDir)) {
    New-Item -ItemType Directory -Path $zipDir | Out-Null
}

# Remove old zip if present
if (Test-Path -LiteralPath $OutZip) {
    Remove-Item -LiteralPath $OutZip -Force
}

# Create ZIP
if ($files.Count -eq 0) {
    Write-Warning "No files matched the filters. Aborting to avoid creating empty ZIP."
    exit 3
}

Compress-Archive -Path $files.FullName -DestinationPath $OutZip -CompressionLevel Optimal

Write-Host ("Created: {0}" -f (Resolve-Path -LiteralPath $OutZip).Path)
exit 0