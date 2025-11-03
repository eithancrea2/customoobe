using System;
using System.Windows;

namespace CustomOOBE
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Determinar el tema basado en la hora del día
            var currentHour = DateTime.Now.Hour;
            var isDarkMode = currentHour >= 18 || currentHour < 6;

            ApplyTheme(isDarkMode ? "Dark" : "Light");
        }

        public static void ApplyTheme(string theme)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"Themes/{theme}Theme.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
