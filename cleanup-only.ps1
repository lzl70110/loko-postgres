#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param()

function Step($t){ Write-Host "`n>> $t" -ForegroundColor Cyan }
function Ok($t){ Write-Host "   OK:  $t" -ForegroundColor Green }
function Warn($t){ Write-Host "  WARN: $t" -ForegroundColor Yellow }
function Err($t){ Write-Host "   ERR: $t" -ForegroundColor Red }

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $root) { $root = (Get-Location).Path }
Set-Location $root

Step "Работна директория: $((Get-Location).Path)"

# Хелпър за изпълнение на команден ред, с поддръжка на -WhatIf/-Confirm
function Invoke-IfApproved([string]$description, [scriptblock]$action) {
    if ($PSCmdlet.ShouldProcess($description)) {
        & $action
    } else {
        Write-Host "   (whatif) $description" -ForegroundColor DarkGray
    }
}

# 1) Спиране на build server-и и евентуални инстанции
Step "Спиране на build server-и и евентуални инстанции на Loco1.Web.exe"
Invoke-IfApproved 'dotnet build-server shutdown' { dotnet build-server shutdown | Out-Null }
Invoke-IfApproved 'taskkill /IM Loco1.Web.exe /F' { taskkill /IM Loco1.Web.exe /F 2>$null | Out-Null }

# 2) Премахване на Repositories проект (ако съществува)
$reposProj = Join-Path $root 'Repositories\Loco1.Repositories.csproj'
if (Test-Path $reposProj) {
    Step "Премахване от .sln: Repositories/Loco1.Repositories.csproj"
    Invoke-IfApproved "dotnet sln remove $reposProj" { dotnet sln remove "$reposProj" 2>$null }
} else {
    Ok "Repositories/Loco1.Repositories.csproj липсва (прескачам remove от .sln)"
}
if (Test-Path "$root\Repositories") {
    Step "Изтриване на папката .\Repositories"
    Invoke-IfApproved "git rm -r --cached .\Repositories" { git rm -r --cached "$root\Repositories" 2>$null }
    Invoke-IfApproved "Remove-Item .\Repositories"       { Remove-Item -Recurse -Force "$root\Repositories" }
    Ok "Repositories е премахнат"
} else {
    Ok "Папката .\Repositories липсва (ОК)"
}

# 3) Премахване на Loco1.Data.Models (ако съществува)
$dataModelsProj = Join-Path $root 'Loco1.Data.Models\Loco1.Data.Models.csproj'
if (Test-Path $dataModelsProj) {
    Step "Премахване от .sln: Loco1.Data.Models"
    Invoke-IfApproved "dotnet sln remove $dataModelsProj" { dotnet sln remove "$dataModelsProj" 2>$null }

    Step "Изтриване на папката .\Loco1.Data.Models"
    Invoke-IfApproved "git rm -r --cached .\Loco1.Data.Models" { git rm -r --cached "$root\Loco1.Data.Models" 2>$null }
    Invoke-IfApproved "Remove-Item .\Loco1.Data.Models"        { Remove-Item -Recurse -Force "$root\Loco1.Data.Models" }
    Ok "Loco1.Data.Models е премахнат"
} else {
    Ok "Loco1.Data.Models.csproj липсва (прескачам)"
}

# 4) Премахване на празната папка Loco1.Web/Views/Roles
$rolesViewDir = Join-Path $root 'Loco1.Web\Views\Roles'
if (Test-Path $rolesViewDir) {
    Step "Премахване на Loco1.Web/Views/Roles"
    Invoke-IfApproved "Remove-Item $rolesViewDir" { Remove-Item -Recurse -Force "$rolesViewDir" }
    Ok "Views/Roles е премахната"
} else {
    Ok "Views/Roles липсва (ОК)"
}

# 5) Спиране на tracking за appsettings.Development.json (оставя файла локално)
$webDir = Join-Path $root 'Loco1.Web'
$devJson = Join-Path $webDir 'appsettings.Development.json'
$webGitIgnore = Join-Path $webDir '.gitignore'
if (Test-Path $devJson) {
    Step "Добавяне в .gitignore и спиране на tracking за appsettings.Development.json"
    if (-not (Test-Path $webGitIgnore)) {
        Invoke-IfApproved "New-Item $webGitIgnore" { New-Item -ItemType File -Path "$webGitIgnore" -Force | Out-Null }
    }
    $ignoreLine = 'appsettings.Development.json'
    $giContent = if (Test-Path $webGitIgnore) { Get-Content $webGitIgnore -ErrorAction SilentlyContinue } else { @() }
    if ($giContent -notcontains $ignoreLine) {
        Invoke-IfApproved "Add-Content $webGitIgnore '$ignoreLine'" { Add-Content "$webGitIgnore" $ignoreLine }
        Ok "Добавен в .gitignore: $ignoreLine"
    } else {
        Ok "Вече е в .gitignore"
    }
    Invoke-IfApproved "git rm --cached $devJson" { git rm --cached "$devJson" 2>$null }
    Invoke-IfApproved "git add $webGitIgnore"    { git add "$webGitIgnore" }
    Ok "Tracking stopped (файлът остава локално)"
} else {
    Ok "appsettings.Development.json не е намерен (прескачам)"
}

# 6) ProjectReference-и: Web -> Data/Service/ViewModels/Localizer/GCommon
Step "ProjectReference-и за Loco1.Web"
$refsForWeb = @(
  'GCommon\GCommon.csproj',
  'Loco1.Data\Loco1.Data.csproj',
  'Loco1.Service\Loco1.Service.csproj',
  'Loco1.ViewModels\Loco1.ViewModels.csproj',
  'Loco1.Localizer\Loco1.Localizer.csproj'
)
foreach ($r in $refsForWeb) {
    $full = Join-Path $root $r
    if (Test-Path $full) {
        Invoke-IfApproved "dotnet add Loco1.Web reference $r" { dotnet add "$webDir\Loco1.Web.csproj" reference "$full" 2>$null }
    } else {
        Warn "Пропускам reference (липсва файл): $r"
    }
}

# 7) ProjectReference-и: Service -> Data/ViewModels/Localizer/GCommon
Step "ProjectReference-и за Loco1.Service"
$serviceProj = Join-Path $root 'Loco1.Service\Loco1.Service.csproj'
$refsForService = @(
  'Loco1.Data\Loco1.Data.csproj',
  'Loco1.ViewModels\Loco1.ViewModels.csproj',
  'Loco1.Localizer\Loco1.Localizer.csproj',
  'GCommon\GCommon.csproj'
)
if (Test-Path $serviceProj) {
    foreach ($r in $refsForService) {
        $full = Join-Path $root $r
        if (Test-Path $full) {
            Invoke-IfApproved "dotnet add Loco1.Service reference $r" { dotnet add "$serviceProj" reference "$full" 2>$null }
        } else {
            Warn "Пропускам reference (липсва файл): $r"
        }
    }
} else {
    Warn "Липсва Loco1.Service.csproj (прескачам refs към Service)"
}

# 8) Почистване и билд (без run)
Step "Премахване на bin/obj"
Invoke-IfApproved "Remove-Item bin/obj" { Get-ChildItem -Path . -Include bin,obj -Recurse -Force | Remove-Item -Recurse -Force }

Step "Restore + Build (без стартиране)"
Invoke-IfApproved "dotnet restore" { dotnet restore }
Invoke-IfApproved "dotnet build"   { dotnet build }

Ok "Готово: Почистено и билднато (без run)."
Write-Host "`nСъвет: пусни 'dotnet sln list' за финален оглед на проектите в решението." -ForegroundColor DarkCyan