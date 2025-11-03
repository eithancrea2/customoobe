# Quick Start - Script de inicio rápido para Custom OOBE
# Este script ejecuta todos los pasos automáticamente

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Custom OOBE - Inicio Rápido" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Verificar permisos de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Este script requiere permisos de administrador." -ForegroundColor Red
    Write-Host "Reiniciando con permisos elevados..." -ForegroundColor Yellow

    Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$ErrorActionPreference = "Stop"

# Paso 1: Instalar dependencias
Write-Host "`n[Paso 1/3] Instalando dependencias..." -ForegroundColor Yellow
& "$PSScriptRoot\install-dependencies.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nError en la instalación de dependencias" -ForegroundColor Red
    pause
    exit 1
}

# Paso 2: Compilar proyecto
Write-Host "`n[Paso 2/3] Compilando proyecto..." -ForegroundColor Yellow
& "$PSScriptRoot\build.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nError en la compilación" -ForegroundColor Red
    pause
    exit 1
}

# Paso 3: Desplegar
Write-Host "`n[Paso 3/3] Desplegando en el sistema..." -ForegroundColor Yellow

# Pedir nombre del equipo
$computerName = Read-Host "`nIngresa el nombre del equipo (Enter para usar 'Mi Equipo Premium')"
if ([string]::IsNullOrWhiteSpace($computerName)) {
    $computerName = "Mi Equipo Premium"
}

& "$PSScriptRoot\deploy.ps1" -ComputerName $computerName

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nError en el despliegue" -ForegroundColor Red
    pause
    exit 1
}

# Preguntar si desea probar ahora
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "   ¡Instalación Completada!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

$response = Read-Host "¿Deseas probar el OOBE ahora? (S/N)"
if ($response -eq "S" -or $response -eq "s") {
    Write-Host "`nIniciando Custom OOBE..." -ForegroundColor Yellow
    Start-Process "$env:ProgramFiles\CustomOOBE\CustomOOBE.exe" -ArgumentList "/oobe"
}
else {
    Write-Host "`nEl Custom OOBE se ejecutará automáticamente en el próximo reinicio." -ForegroundColor Cyan
}

Write-Host "`nPresiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
