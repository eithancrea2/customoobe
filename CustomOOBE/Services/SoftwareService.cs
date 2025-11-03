using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CustomOOBE.Models;

namespace CustomOOBE.Services
{
    public class SoftwareService
    {
        private readonly HttpClient _httpClient = new();

        public List<SoftwarePackage> GetAvailableSoftware()
        {
            return new List<SoftwarePackage>
            {
                // Navegadores
                new SoftwarePackage
                {
                    Name = "Google Chrome",
                    Description = "Navegador web rápido y seguro",
                    Category = "Navegadores",
                    DownloadUrl = "https://dl.google.com/chrome/install/latest/chrome_installer.exe",
                    SizeInMB = 90
                },
                new SoftwarePackage
                {
                    Name = "Mozilla Firefox",
                    Description = "Navegador web de código abierto",
                    Category = "Navegadores",
                    DownloadUrl = "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=es-ES",
                    SizeInMB = 60
                },
                new SoftwarePackage
                {
                    Name = "Microsoft Edge",
                    Description = "Navegador web de Microsoft (Preinstalado)",
                    Category = "Navegadores",
                    DownloadUrl = "",
                    SizeInMB = 0
                },

                // Herramientas de compresión
                new SoftwarePackage
                {
                    Name = "7-Zip",
                    Description = "Compresor de archivos gratuito",
                    Category = "Compresión",
                    DownloadUrl = "https://www.7-zip.org/a/7z2301-x64.exe",
                    SizeInMB = 1
                },
                new SoftwarePackage
                {
                    Name = "WinRAR",
                    Description = "Compresor de archivos popular",
                    Category = "Compresión",
                    DownloadUrl = "https://www.rarlab.com/rar/winrar-x64-623.exe",
                    SizeInMB = 3
                },

                // Reproductores de medios
                new SoftwarePackage
                {
                    Name = "VLC Media Player",
                    Description = "Reproductor multimedia universal",
                    Category = "Multimedia",
                    DownloadUrl = "https://get.videolan.org/vlc/last/win64/vlc-3.0.20-win64.exe",
                    SizeInMB = 40
                },
                new SoftwarePackage
                {
                    Name = "Windows Media Player",
                    Description = "Reproductor de Windows (Preinstalado)",
                    Category = "Multimedia",
                    DownloadUrl = "",
                    SizeInMB = 0
                },

                // Herramientas de limpieza
                new SoftwarePackage
                {
                    Name = "CCleaner",
                    Description = "Limpiador y optimizador del sistema",
                    Category = "Optimización",
                    DownloadUrl = "https://download.ccleaner.com/ccsetup608.exe",
                    SizeInMB = 50
                },

                // Antivirus
                new SoftwarePackage
                {
                    Name = "Windows Defender",
                    Description = "Antivirus integrado de Windows (Preinstalado)",
                    Category = "Seguridad",
                    DownloadUrl = "",
                    SizeInMB = 0
                },
                new SoftwarePackage
                {
                    Name = "Avast Free Antivirus",
                    Description = "Antivirus gratuito",
                    Category = "Seguridad",
                    DownloadUrl = "https://www.avast.com/download-thank-you.php?product=FAV-ONLINE",
                    SizeInMB = 500
                },

                // Productividad
                new SoftwarePackage
                {
                    Name = "Adobe Acrobat Reader",
                    Description = "Lector de documentos PDF",
                    Category = "Productividad",
                    DownloadUrl = "https://ardownload2.adobe.com/pub/adobe/reader/win/AcrobatDC/2300820360/AcroRdrDC2300820360_en_US.exe",
                    SizeInMB = 200
                },
                new SoftwarePackage
                {
                    Name = "LibreOffice",
                    Description = "Suite ofimática de código abierto",
                    Category = "Productividad",
                    DownloadUrl = "https://download.documentfoundation.org/libreoffice/stable/7.6.4/win/x86_64/LibreOffice_7.6.4_Win_x64.msi",
                    SizeInMB = 300
                },

                // Comunicación
                new SoftwarePackage
                {
                    Name = "Discord",
                    Description = "Plataforma de comunicación",
                    Category = "Comunicación",
                    DownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64",
                    SizeInMB = 90
                },
                new SoftwarePackage
                {
                    Name = "Zoom",
                    Description = "Videoconferencias",
                    Category = "Comunicación",
                    DownloadUrl = "https://zoom.us/client/latest/ZoomInstallerFull.exe",
                    SizeInMB = 50
                }
            };
        }

        public async Task<bool> DownloadAndInstallAsync(SoftwarePackage package, IProgress<int> progress)
        {
            if (string.IsNullOrEmpty(package.DownloadUrl))
            {
                // Software preinstalado
                progress?.Report(100);
                return true;
            }

            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"{package.Name}_{Guid.NewGuid()}.exe");

                // Descargar el instalador
                progress?.Report(10);

                using (var response = await _httpClient.GetAsync(package.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                var progressPercentage = (int)((downloadedBytes * 50) / totalBytes) + 10;
                                progress?.Report(progressPercentage);
                            }
                        }
                    }
                }

                progress?.Report(60);

                // Instalar el software
                var installProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempPath,
                        Arguments = "/S /silent /quiet /norestart", // Argumentos comunes para instalación silenciosa
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    }
                };

                installProcess.Start();

                progress?.Report(80);

                // Esperar a que termine la instalación (máximo 5 minutos)
                if (!installProcess.WaitForExit(300000))
                {
                    installProcess.Kill();
                    Debug.WriteLine($"Timeout al instalar {package.Name}");
                    return false;
                }

                progress?.Report(100);

                // Limpiar archivo temporal
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                return installProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al descargar/instalar {package.Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<(SoftwarePackage Package, bool Success)>> InstallSelectedSoftwareAsync(
            List<SoftwarePackage> packages,
            IProgress<(string packageName, int progress)> progress)
        {
            var results = new List<(SoftwarePackage, bool)>();

            foreach (var package in packages)
            {
                if (!package.IsSelected) continue;

                var packageProgress = new Progress<int>(p =>
                {
                    progress?.Report((package.Name, p));
                });

                var success = await DownloadAndInstallAsync(package, packageProgress);
                results.Add((package, success));

                // Pequeña pausa entre instalaciones
                await Task.Delay(1000);
            }

            return results;
        }
    }
}
