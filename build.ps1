# Script de compilación del Custom OOBE
# Ejecutar después de install-dependencies.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Custom OOBE - Script de Compilación" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$projectPath = "$PSScriptRoot\CustomOOBE\CustomOOBE.csproj"
$outputPath = "$PSScriptRoot\Build"

if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: No se encontró el archivo del proyecto en $projectPath" -ForegroundColor Red
    Write-Host "Asegúrate de que todos los archivos del proyecto estén en la carpeta CustomOOBE" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "[1/3] Limpiando compilaciones anteriores..." -ForegroundColor Yellow
if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

Write-Host "`n[2/3] Restaurando paquetes NuGet..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\CustomOOBE"
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo al restaurar paquetes NuGet" -ForegroundColor Red
    pause
    exit 1
}

Write-Host "`n[3/3] Compilando proyecto..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $outputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la compilación" -ForegroundColor Red
    pause
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Compilación completada exitosamente!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Ejecutable creado en: $outputPath\CustomOOBE.exe" -ForegroundColor Green
Write-Host "`nSiguiente paso: Ejecutar deploy.ps1 para configurar el auto-inicio" -ForegroundColor Yellow
Write-Host ""
