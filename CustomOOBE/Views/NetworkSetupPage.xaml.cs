using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CustomOOBE.Models;
using CustomOOBE.Services;

namespace CustomOOBE.Views
{
    public partial class NetworkSetupPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly string _username;
        private readonly WiFiService _wifiService;
        private WiFiNetwork? _selectedNetwork;
        private DispatcherTimer? _ethernetCheckTimer;

        public NetworkSetupPage(MainWindow mainWindow, string username)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _username = username;
            _wifiService = new WiFiService();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(2);

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Verificar conexión Ethernet o WiFi
            if ((_wifiService.IsEthernetConnected() || _wifiService.IsWiFiConnected()) && _wifiService.IsConnectedToInternet())
            {
                StatusText.Text = _wifiService.IsEthernetConnected() ? "✓ Conectado por cable Ethernet" : "✓ Conectado a WiFi";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
                StatusText.Visibility = Visibility.Visible;
                NextButton.IsEnabled = true;
                SkipButton.IsEnabled = false; // Deshabilitar Skip si ya está conectado
            }
            else
            {
                await LoadNetworks();
                StartEthernetMonitoring();
            }
        }

        private void StartEthernetMonitoring()
        {
            // Crear un timer para verificar si se conecta un adaptador Ethernet
            _ethernetCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Verificar cada 2 segundos
            };
            _ethernetCheckTimer.Tick += async (s, e) =>
            {
                if (_wifiService.IsEthernetConnected() && _wifiService.IsConnectedToInternet())
                {
                    _ethernetCheckTimer?.Stop();
                    StatusText.Text = "✓ Cable Ethernet detectado y conectado";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                    StatusText.Visibility = Visibility.Visible;
                    NextButton.IsEnabled = true;
                    SkipButton.IsEnabled = false;

                    // Avanzar automáticamente a la siguiente página después de 2 segundos
                    await System.Threading.Tasks.Task.Delay(2000);
                    NavigationService?.Navigate(new SoftwareSetupPage(_mainWindow, _username));
                }
            };
            _ethernetCheckTimer.Start();
        }

        private void StopEthernetMonitoring()
        {
            if (_ethernetCheckTimer != null)
            {
                _ethernetCheckTimer.Stop();
                _ethernetCheckTimer = null;
            }
        }

        private async System.Threading.Tasks.Task LoadNetworks()
        {
            StatusText.Text = "Buscando redes...";
            StatusText.Visibility = Visibility.Visible;

            var networks = await _wifiService.GetAvailableNetworksAsync();
            NetworksList.ItemsSource = networks;

            StatusText.Visibility = Visibility.Collapsed;
        }

        private void NetworkItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is WiFiNetwork network)
            {
                _selectedNetwork = network;

                if (network.RequiresPassword)
                {
                    NetworkNameText.Text = $"Red: {network.SSID}";
                    PasswordDialog.Visibility = Visibility.Visible;
                    PasswordBox.Focus();
                }
                else
                {
                    ConnectToNetwork("");
                }
            }
        }

        private async void ConnectToNetwork(string password)
        {
            if (_selectedNetwork == null) return;

            StatusText.Text = $"Conectando a {_selectedNetwork.SSID}...";
            StatusText.Visibility = Visibility.Visible;
            NextButton.IsEnabled = false;

            var success = await _wifiService.ConnectToNetworkAsync(_selectedNetwork.SSID, password);

            if (success)
            {
                StatusText.Text = "✓ Conectado exitosamente";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
                NextButton.IsEnabled = true;
                SkipButton.IsEnabled = false; // Deshabilitar Skip después de conectar
            }
            else
            {
                StatusText.Text = "✗ Error al conectar. Verifica la contraseña.";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadNetworks();
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StopEthernetMonitoring();
            NavigationService?.GoBack();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar advertencia al saltar
            WarningDialog.Visibility = Visibility.Visible;
        }

        private void ContinueWithoutInternet_Click(object sender, RoutedEventArgs e)
        {
            StopEthernetMonitoring();
            WarningDialog.Visibility = Visibility.Collapsed;
            NavigationService?.Navigate(new ThemeSetupPage(_mainWindow, _username));
        }

        private void CancelSkip_Click(object sender, RoutedEventArgs e)
        {
            WarningDialog.Visibility = Visibility.Collapsed;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            StopEthernetMonitoring();
            NavigationService?.Navigate(new SoftwareSetupPage(_mainWindow, _username));
        }
        private void CancelPassword_Click(object sender, RoutedEventArgs e) => PasswordDialog.Visibility = Visibility.Collapsed;

        private void ConnectWithPassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordDialog.Visibility = Visibility.Collapsed;
            ConnectToNetwork(PasswordBox.Password);
            PasswordBox.Clear();
        }
    }
}
