@echo off
setlocal EnableDelayedExpansion

set "ROOT=%~dp0"
set "OUTZIP=%ROOT%Loco_17_Feb_clean.zip"

echo ROOT  : "%ROOT%"
echo OUTZIP: "%OUTZIP%"
echo Packing, please wait...

powershell -NoLogo -NoProfile -Command ^
  "$root = [System.IO.Path]::GetFullPath('%ROOT%');" ^
  "$out  = [System.IO.Path]::GetFullPath('%OUTZIP%');" ^
  "$includeExt = '.sln','.csproj','.props','.targets','.cs','.cshtml','.razor','.json','.resx','.config','.txt','.md','.ico','.js','.css','.scss','.svg';" ^
  "$exclude = 'bin','obj','.git','.vs','node_modules','publish';" ^
  "$files = Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object { " ^
  "   $rel = $_.FullName.Substring($root.Length).TrimStart('\','/'); " ^
  "   -not ($exclude | ForEach-Object { $rel -match '(^|[\\/])' + [regex]::Escape($_) + '([\\/]|$)' }) -and " ^
  "   ($includeExt -contains $_.Extension.ToLower() -or $_.Extension -eq '') };" ^
  "if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force };" ^
  "if ($files.Count -eq 0) { Write-Host 'No files matched filters.'; exit 3 };" ^
  "Compress-Archive -Path $files.FullName -DestinationPath $out -CompressionLevel Optimal;" ^
  "Write-Host ('Created: ' + (Resolve-Path -LiteralPath $out).Path);"

echo.
pause
