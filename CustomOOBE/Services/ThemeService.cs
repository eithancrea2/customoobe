using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CustomOOBE.Services
{
    public class ThemeService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        public async Task<bool> ApplyWindowsThemeAsync(bool isDark)
        {
            return await Task.Run(() =>
            {
                try
                {
                    const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";

                    using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
                    {
                        if (key != null)
                        {
                            // Aplicar tema a las aplicaciones
                            key.SetValue("AppsUseLightTheme", isDark ? 0 : 1, RegistryValueKind.DWord);
                            // Aplicar tema al sistema
                            key.SetValue("SystemUsesLightTheme", isDark ? 0 : 1, RegistryValueKind.DWord);

                            Debug.WriteLine($"Tema de Windows establecido a: {(isDark ? "Oscuro" : "Claro")}");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al aplicar tema de Windows: {ex.Message}");
                }
                return false;
            });
        }

        public async Task<bool> SetWallpaperAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(imagePath))
                    {
                        Debug.WriteLine($"El archivo de fondo no existe: {imagePath}");
                        return false;
                    }

                    // Copiar la imagen a una ubicación permanente
                    var wallpaperPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Microsoft", "Windows", "Themes", "TranscodedWallpaper");

                    var directory = Path.GetDirectoryName(wallpaperPath);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(imagePath, wallpaperPath, true);

                    // Establecer el fondo de pantalla
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath,
                        SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                    // También actualizar el registro
                    const string keyPath = @"Control Panel\Desktop";
                    using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
                    {
                        key?.SetValue("Wallpaper", wallpaperPath);
                        key?.SetValue("WallpaperStyle", "10"); // Fill
                        key?.SetValue("TileWallpaper", "0");
                    }

                    Debug.WriteLine($"Fondo de pantalla establecido: {imagePath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al establecer fondo de pantalla: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> SetLockScreenAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(imagePath))
                    {
                        Debug.WriteLine($"El archivo de pantalla de bloqueo no existe: {imagePath}");
                        return false;
                    }

                    // Copiar la imagen
                    var lockScreenPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "Microsoft", "Windows", "SystemData", "S-1-5-18", "ReadOnly", "LockScreen_Z");

                    var directory = Path.GetDirectoryName(lockScreenPath);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(imagePath, lockScreenPath, true);

                    // Actualizar registro
                    const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";
                    using (var key = Registry.LocalMachine.CreateSubKey(keyPath))
                    {
                        key?.SetValue("LockScreenImagePath", imagePath);
                        key?.SetValue("LockScreenImageUrl", imagePath);
                        key?.SetValue("LockScreenImageStatus", 1);
                    }

                    Debug.WriteLine($"Pantalla de bloqueo establecida: {imagePath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al establecer pantalla de bloqueo: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> SetAccentColorAsync(System.Windows.Media.Color color)
        {
            return await Task.Run(() =>
            {
                try
                {
                    const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Accent";

                    using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            // Convertir color ARGB a formato ABGR
                            var abgr = (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
                            key.SetValue("AccentColorMenu", abgr, RegistryValueKind.DWord);
                            key.SetValue("StartColorMenu", abgr, RegistryValueKind.DWord);

                            Debug.WriteLine($"Color de acento establecido");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al establecer color de acento: {ex.Message}");
                }
                return false;
            });
        }

        public bool IsDarkTime()
        {
            var currentHour = DateTime.Now.Hour;
            return currentHour >= 18 || currentHour < 6;
        }
    }
}
