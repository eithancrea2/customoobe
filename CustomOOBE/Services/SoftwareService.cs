using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
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
                    IconUrl = "https://www.google.com/chrome/static/images/chrome-logo-m100.svg",
                    SizeInMB = 90
                },
                new SoftwarePackage
                {
                    Name = "Mozilla Firefox",
                    Description = "Navegador web de código abierto",
                    Category = "Navegadores",
                    DownloadUrl = "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=es-ES",
                    IconUrl = "https://www.mozilla.org/media/protocol/img/logos/firefox/browser/logo.svg",
                    SizeInMB = 60
                },
                new SoftwarePackage
                {
                    Name = "Microsoft Edge",
                    Description = "Navegador web de Microsoft (Preinstalado)",
                    Category = "Navegadores",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/9/98/Microsoft_Edge_logo_%282019%29.svg",
                    DownloadUrl = "",
                    SizeInMB = 0
                },

                // Herramientas de compresión
                new SoftwarePackage
                {
                    Name = "7-Zip",
                    Description = "Compresor de archivos gratuito",
                    Category = "Compresión",
                    IconUrl = "https://www.7-zip.org/7ziplogo.png",
                    DownloadUrl = "https://www.7-zip.org/a/7z2301-x64.exe",
                    SizeInMB = 1
                },
                new SoftwarePackage
                {
                    Name = "WinRAR",
                    Description = "Compresor de archivos popular",
                    Category = "Compresión",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/f/f4/WinRAR_Logo.svg",
                    DownloadUrl = "https://www.rarlab.com/rar/winrar-x64-623.exe",
                    SizeInMB = 3
                },

                // Reproductores de medios
                new SoftwarePackage
                {
                    Name = "VLC Media Player",
                    Description = "Reproductor multimedia universal",
                    Category = "Multimedia",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/e/e6/VLC_Icon.svg",
                    DownloadUrl = "https://get.videolan.org/vlc/last/win64/vlc-3.0.20-win64.exe",
                    SizeInMB = 40
                },
                new SoftwarePackage
                {
                    Name = "Windows Media Player",
                    Description = "Reproductor de Windows (Preinstalado)",
                    Category = "Multimedia",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Windows_Media_Player_2020_icon.svg",
                    DownloadUrl = "",
                    SizeInMB = 0
                },

                // Herramientas de limpieza
                new SoftwarePackage
                {
                    Name = "CCleaner",
                    Description = "Limpiador y optimizador del sistema",
                    Category = "Optimización",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/1/1b/CCleaner_icon.svg",
                    DownloadUrl = "https://download.ccleaner.com/ccsetup608.exe",
                    SizeInMB = 50
                },

                // Antivirus
                new SoftwarePackage
                {
                    Name = "Windows Defender",
                    Description = "Antivirus integrado de Windows (Preinstalado)",
                    Category = "Seguridad",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7e/Windows_Defender_logo.svg",
                    DownloadUrl = "",
                    SizeInMB = 0
                },
                new SoftwarePackage
                {
                    Name = "Avast Free Antivirus",
                    Description = "Antivirus gratuito",
                    Category = "Seguridad",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/0/08/Avast_Software_logo.svg",
                    DownloadUrl = "https://www.avast.com/download-thank-you.php?product=FAV-ONLINE",
                    SizeInMB = 500
                },

                // Productividad
                new SoftwarePackage
                {
                    Name = "Adobe Acrobat Reader",
                    Description = "Lector de documentos PDF",
                    Category = "Productividad",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/8/87/PDF_file_icon.svg",
                    DownloadUrl = "https://ardownload2.adobe.com/pub/adobe/reader/win/AcrobatDC/2300820360/AcroRdrDC2300820360_en_US.exe",
                    SizeInMB = 200
                },
                new SoftwarePackage
                {
                    Name = "LibreOffice",
                    Description = "Suite ofimática de código abierto",
                    Category = "Productividad",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a8/LibreOffice_logo.svg",
                    DownloadUrl = "https://download.documentfoundation.org/libreoffice/stable/7.6.4/win/x86_64/LibreOffice_7.6.4_Win_x64.msi",
                    SizeInMB = 300
                },

                // Comunicación
                new SoftwarePackage
                {
                    Name = "Discord",
                    Description = "Plataforma de comunicación",
                    Category = "Comunicación",
                    IconUrl = "https://assets-global.website-files.com/6257adef93867e50d84d30e2/636e0a6a49cf127bf92de1e2_icon_clyde_blurple_RGB.svg",
                    DownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64",
                    SizeInMB = 90
                },
                new SoftwarePackage
                {
                    Name = "Zoom",
                    Description = "Videoconferencias",
                    Category = "Comunicación",
                    IconUrl = "https://st1.zoom.us/static/5.17.11.3721/image/new/ZoomLogo.png",
                    DownloadUrl = "https://zoom.us/client/latest/ZoomInstallerFull.exe",
                    SizeInMB = 50
                }
            };
        }

        public async Task<BitmapImage?> DownloadIconAsync(string iconUrl)
        {
            if (string.IsNullOrEmpty(iconUrl))
                return null;

            try
            {
                // Configurar timeout y headers para la descarga
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, iconUrl);
                requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();

                var imageData = await response.Content.ReadAsByteArrayAsync();
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.DecodePixelWidth = 48; // Optimizar el tamaño del icono
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al descargar icono desde {iconUrl}: {ex.Message}");
                // Retornar un icono por defecto en caso de error
                return CreateDefaultIcon();
            }
        }

        private BitmapImage? CreateDefaultIcon()
        {
            try
            {
                // Crear un icono simple de respaldo
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/default_app_icon.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 48;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
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

                // Instalar el software con argumentos específicos según el instalador
                var arguments = GetInstallArguments(package.Name, tempPath);

                var installProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempPath,
                        Arguments = arguments,
                        UseShellExecute = true,
                        Verb = "runas", // Ejecutar como administrador
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                Debug.WriteLine($"Instalando {package.Name} con comando: {tempPath} {arguments}");
                installProcess.Start();

                progress?.Report(80);

                // Esperar a que termine la instalación (máximo 10 minutos para instaladores grandes)
                var timeout = package.SizeInMB > 200 ? 600000 : 300000;
                if (!installProcess.WaitForExit(timeout))
                {
                    try
                    {
                        installProcess.Kill();
                    }
                    catch { }
                    Debug.WriteLine($"Timeout al instalar {package.Name}");
                    return false;
                }

                progress?.Report(100);
                Debug.WriteLine($"Instalación de {package.Name} completada con código de salida: {installProcess.ExitCode}");

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

        private string GetInstallArguments(string packageName, string installerPath)
        {
            // Argumentos específicos según el tipo de instalador
            var extension = Path.GetExtension(installerPath).ToLower();

            // Argumentos específicos por nombre de paquete
            if (packageName.Contains("Chrome"))
                return "/silent /install";
            else if (packageName.Contains("Firefox"))
                return "/S";
            else if (packageName.Contains("7-Zip"))
                return "/S";
            else if (packageName.Contains("VLC"))
                return "/L=1033 /S";
            else if (packageName.Contains("CCleaner"))
                return "/S";
            else if (packageName.Contains("Avast"))
                return "/silent /norestart";
            else if (packageName.Contains("Adobe") || packageName.Contains("Acrobat"))
                return "/sPB /rs /msi";
            else if (packageName.Contains("Discord"))
                return "--silent";
            else if (packageName.Contains("Zoom"))
                return "/silent /norestart";
            else if (extension == ".msi")
                return "/qn /norestart"; // MSI installers (como LibreOffice)
            else
                return "/S /silent /quiet /norestart"; // Argumentos genéricos
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
