# Script de despliegue y configuración automática
# Ejecutar como Administrador después de compilar

param(
    [string]$ComputerName = "Mi Equipo Premium"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Custom OOBE - Script de Despliegue" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Verificar permisos de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Este script requiere permisos de administrador" -ForegroundColor Red
    pause
    exit 1
}

$buildPath = "$PSScriptRoot\Build\CustomOOBE.exe"
$installPath = "$env:ProgramFiles\CustomOOBE"
$configPath = "$env:ProgramData\CustomOOBE"

if (-not (Test-Path $buildPath)) {
    Write-Host "ERROR: No se encontró el ejecutable compilado" -ForegroundColor Red
    Write-Host "Ejecuta build.ps1 primero para compilar el proyecto" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "[1/5] Creando directorios de instalación..." -ForegroundColor Yellow
if (-not (Test-Path $installPath)) {
    New-Item -ItemType Directory -Path $installPath -Force | Out-Null
}
if (-not (Test-Path $configPath)) {
    New-Item -ItemType Directory -Path $configPath -Force | Out-Null
}

Write-Host "`n[2/5] Copiando archivos..." -ForegroundColor Yellow
Copy-Item -Path "$PSScriptRoot\Build\*" -Destination $installPath -Recurse -Force

Write-Host "`n[3/5] Creando archivo de configuración..." -ForegroundColor Yellow
$configContent = @"
{
  "ComputerName": "$ComputerName",
  "FirstRun": true,
  "SetupCompleted": false,
  "Theme": "auto",
  "DatabasePath": "$configPath\\reviews.db"
}
"@
$configContent | Out-File -FilePath "$configPath\config.json" -Encoding UTF8

Write-Host "`n[4/5] Configurando auto-inicio en el registro..." -ForegroundColor Yellow

# Registrar en RunOnce para ejecutarse antes del login
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce"
Set-ItemProperty -Path $regPath -Name "CustomOOBE" -Value "`"$installPath\CustomOOBE.exe`" /oobe" -Force

# Crear tarea programada para ejecutarse en el inicio
$taskName = "CustomOOBE"
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

$action = New-ScheduledTaskAction -Execute "$installPath\CustomOOBE.exe" -Argument "/oobe"
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null

Write-Host "`n[5/5] Configurando permisos..." -ForegroundColor Yellow
icacls $installPath /grant "Users:(OI)(CI)RX" /T | Out-Null
icacls $configPath /grant "Users:(OI)(CI)F" /T | Out-Null

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Despliegue completado exitosamente!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Instalación completada en: $installPath" -ForegroundColor Green
Write-Host "Configuración guardada en: $configPath" -ForegroundColor Green
Write-Host "`nEl Custom OOBE se ejecutará automáticamente en el próximo reinicio" -ForegroundColor Yellow
Write-Host "`nPara probar ahora, ejecuta: $installPath\CustomOOBE.exe /oobe" -ForegroundColor Cyan
Write-Host ""
