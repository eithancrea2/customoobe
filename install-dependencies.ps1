# Script de instalación de dependencias para Custom OOBE
# Ejecutar como Administrador

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Custom OOBE - Instalador de Dependencias" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Verificar permisos de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Este script requiere permisos de administrador" -ForegroundColor Red
    Write-Host "Ejecuta PowerShell como Administrador e intenta nuevamente" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "[1/6] Verificando .NET SDK..." -ForegroundColor Yellow

# Verificar si .NET 6.0 o superior está instalado
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host ".NET SDK no encontrado. Descargando .NET 8.0 SDK..." -ForegroundColor Yellow

    $installerUrl = "https://download.visualstudio.microsoft.com/download/pr/93961dfb-d1e0-49c8-9230-abcba1ebab5a/811ed1eb63d7652325727720edda26a8/dotnet-sdk-8.0.403-win-x64.exe"
    $installerPath = "$env:TEMP\dotnet-sdk-installer.exe"

    Write-Host "Descargando desde: $installerUrl" -ForegroundColor Gray
    Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath

    Write-Host "Instalando .NET SDK..." -ForegroundColor Yellow
    Start-Process -FilePath $installerPath -ArgumentList "/quiet", "/norestart" -Wait

    Remove-Item $installerPath -Force

    # Actualizar PATH
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

    Write-Host ".NET SDK instalado correctamente" -ForegroundColor Green
} else {
    Write-Host ".NET SDK ya instalado (version $dotnetVersion)" -ForegroundColor Green
}

Write-Host "`n[2/6] Verificando Visual C++ Redistributable..." -ForegroundColor Yellow

# Verificar Visual C++ Redistributable
$vcRedist = Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* |
            Where-Object { $_.DisplayName -like "*Visual C++ 2015-2022 Redistributable*" }

if (-not $vcRedist) {
    Write-Host "Visual C++ Redistributable no encontrado. Descargando..." -ForegroundColor Yellow

    $vcRedistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
    $vcRedistPath = "$env:TEMP\vc_redist.x64.exe"

    Invoke-WebRequest -Uri $vcRedistUrl -OutFile $vcRedistPath

    Write-Host "Instalando Visual C++ Redistributable..." -ForegroundColor Yellow
    Start-Process -FilePath $vcRedistPath -ArgumentList "/quiet", "/norestart" -Wait

    Remove-Item $vcRedistPath -Force
    Write-Host "Visual C++ Redistributable instalado correctamente" -ForegroundColor Green
} else {
    Write-Host "Visual C++ Redistributable ya instalado" -ForegroundColor Green
}

Write-Host "`n[3/6] Instalando paquetes NuGet necesarios..." -ForegroundColor Yellow

# Crear archivo de proyecto temporal para restaurar paquetes
$projectDir = "$PSScriptRoot\CustomOOBE"
if (Test-Path $projectDir) {
    Set-Location $projectDir
    dotnet restore
    Write-Host "Paquetes NuGet restaurados correctamente" -ForegroundColor Green
} else {
    Write-Host "Directorio del proyecto no encontrado. Se crearán los archivos primero." -ForegroundColor Yellow
}

Write-Host "`n[4/6] Configurando Windows Features necesarios..." -ForegroundColor Yellow

# Habilitar .NET Framework 3.5 si no está habilitado
$netfx3 = Get-WindowsOptionalFeature -Online -FeatureName "NetFx3" -ErrorAction SilentlyContinue
if ($netfx3.State -ne "Enabled") {
    Write-Host "Habilitando .NET Framework 3.5..." -ForegroundColor Yellow
    Enable-WindowsOptionalFeature -Online -FeatureName "NetFx3" -All -NoRestart
}

Write-Host "`n[5/6] Verificando permisos del sistema..." -ForegroundColor Yellow

# Crear directorio para assets si no existe
$assetsDir = "$PSScriptRoot\CustomOOBE\Assets"
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
}

# Crear directorio para base de datos
$dbDir = "$env:ProgramData\CustomOOBE"
if (-not (Test-Path $dbDir)) {
    New-Item -ItemType Directory -Path $dbDir -Force | Out-Null
}

# Dar permisos completos
icacls $dbDir /grant "Users:(OI)(CI)F" /T | Out-Null

Write-Host "`n[6/6] Configurando firewall..." -ForegroundColor Yellow

# Agregar regla de firewall para permitir conexión a base de datos
$ruleName = "CustomOOBE Database Access"
$existingRule = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if (-not $existingRule) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort 1433 | Out-Null
    Write-Host "Regla de firewall creada" -ForegroundColor Green
} else {
    Write-Host "Regla de firewall ya existe" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Instalación completada exitosamente!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Siguiente paso: Ejecutar build.ps1 para compilar el proyecto" -ForegroundColor Yellow
Write-Host ""
