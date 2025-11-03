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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(3);

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            var software = _softwareService.GetAvailableSoftware();
            SoftwareList.ItemsSource = software;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();

        private void SkipButton_Click(object sender, RoutedEventArgs e) =>
            NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username));

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSoftware = _softwareService.GetAvailableSoftware()
                .Where(s => s.IsSelected).ToList();

            if (selectedSoftware.Count == 0)
            {
                NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username));
                return;
            }

            NextButton.IsEnabled = false;
            InstallProgress.Visibility = Visibility.Visible;

            var progress = new Progress<(string packageName, int progress)>(update =>
            {
                ProgressText.Text = $"Instalando {update.packageName}...";
                ProgressBar.Value = update.progress;
            });

            await _softwareService.InstallSelectedSoftwareAsync(selectedSoftware, progress);

            NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username));
        }
    }
}
