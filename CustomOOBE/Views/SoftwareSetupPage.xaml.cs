using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CustomOOBE.Services;

namespace CustomOOBE.Views
{
    public partial class SoftwareSetupPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly string _username;
        private readonly SoftwareService _softwareService;

        public SoftwareSetupPage(MainWindow mainWindow, string username)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _username = username;
            _softwareService = new SoftwareService();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(3);

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Deshabilitar navegación mientras se descargan los iconos
            NextButton.IsEnabled = false;
            SkipButton.IsEnabled = false;
            BackButton.IsEnabled = false;

            // Mostrar mensaje de carga
            StatusText.Text = "Cargando iconos de programas...";
            StatusText.Visibility = Visibility.Visible;

            var software = _softwareService.GetAvailableSoftware();

            // Contador de iconos descargados
            int totalIcons = software.Count(s => !string.IsNullOrEmpty(s.IconUrl));
            int downloadedIcons = 0;

            // Descargar iconos de manera asíncrona con reintentos
            foreach (var app in software)
            {
                if (!string.IsNullOrEmpty(app.IconUrl))
                {
                    // Intentar descargar hasta 3 veces
                    for (int retry = 0; retry < 3; retry++)
                    {
                        try
                        {
                            app.Icon = await _softwareService.DownloadIconAsync(app.IconUrl);
                            if (app.Icon != null)
                            {
                                downloadedIcons++;
                                StatusText.Text = $"Cargando iconos de programas... ({downloadedIcons}/{totalIcons})";
                                break; // Éxito, salir del bucle de reintentos
                            }
                        }
                        catch
                        {
                            if (retry < 2)
                            {
                                await System.Threading.Tasks.Task.Delay(1000); // Esperar 1 segundo antes de reintentar
                            }
                        }
                    }
                }
            }

            SoftwareList.ItemsSource = software;

            // Ocultar mensaje de carga y habilitar navegación
            StatusText.Visibility = Visibility.Collapsed;
            NextButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
            BackButton.IsEnabled = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();

        private void SkipButton_Click(object sender, RoutedEventArgs e) =>
            NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username, new System.Collections.Generic.List<Models.SoftwarePackage>()));

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSoftware = _softwareService.GetAvailableSoftware()
                .Where(s => s.IsSelected).ToList();

            // Navegar a la página de tema primero
            NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username, selectedSoftware));
        }
    }
}
