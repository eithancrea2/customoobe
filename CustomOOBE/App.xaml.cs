using System;
using System.Windows;
using System.Windows.Media;
using CustomOOBE.Services;

namespace CustomOOBE
{
    public partial class App : Application
    {
        public static bool IsWindows11 { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Detectar versión de Windows
            var versionService = new WindowsVersionService();
            IsWindows11 = versionService.IsWindows11();

            // Determinar el tema basado en la hora del día
            var currentHour = DateTime.Now.Hour;
            var isDarkMode = currentHour >= 18 || currentHour < 6;

            ApplyTheme(isDarkMode ? "Dark" : "Light");

            // Aplicar estilos según la versión de Windows
            ApplyWindowsVersionStyles();
        }

        public static void ApplyTheme(string theme)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"Themes/{theme}Theme.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            // Replicar estilos de Windows
            ApplyWindowsVersionStyles();
        }

        private static void ApplyWindowsVersionStyles()
        {
            if (IsWindows11)
            {
                // Windows 11: Bordes más redondeados, efectos más suaves
                Application.Current.Resources["WindowCornerRadius"] = new CornerRadius(12);
                Application.Current.Resources["ButtonCornerRadius"] = new CornerRadius(6);
                Application.Current.Resources["InputCornerRadius"] = new CornerRadius(6);
                Application.Current.Resources["CardCornerRadius"] = new CornerRadius(12);
            }
            else
            {
                // Windows 10: Bordes menos redondeados, diseño más angular
                Application.Current.Resources["WindowCornerRadius"] = new CornerRadius(0);
                Application.Current.Resources["ButtonCornerRadius"] = new CornerRadius(2);
                Application.Current.Resources["InputCornerRadius"] = new CornerRadius(2);
                Application.Current.Resources["CardCornerRadius"] = new CornerRadius(4);
            }
        }
    }
}
