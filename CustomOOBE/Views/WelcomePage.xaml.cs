using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CustomOOBE.Views
{
    public partial class WelcomePage : Page
    {
        private readonly MainWindow _mainWindow;

        public WelcomePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            // Obtener nombre real del equipo desde System Information
            var computerName = Environment.MachineName ?? Environment.GetEnvironmentVariable("COMPUTERNAME") ?? "Este Equipo";
            ComputerNameRun.Text = computerName;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(0);
            StartWelcomeAnimation();
        }

        private async void StartWelcomeAnimation()
        {
            // Animación secuencial de los mensajes de bienvenida (pantalla completa)
            await AnimateMessage(Message1, 0);
            await System.Threading.Tasks.Task.Delay(2500);

            await AnimateMessage(Message2, 0);
            await System.Threading.Tasks.Task.Delay(3000);

            // Desvanecer los mensajes de bienvenida
            var fadeOut1 = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut2 = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Message1.BeginAnimation(UIElement.OpacityProperty, fadeOut1);
            Message2.BeginAnimation(UIElement.OpacityProperty, fadeOut2);

            await System.Threading.Tasks.Task.Delay(800);

            // Ocultar el panel de mensajes de bienvenida
            WelcomeMessagesPanel.Visibility = Visibility.Collapsed;

            // Mostrar el panel de configuración y el panel lateral
            ConfigurationPanel.Visibility = Visibility.Visible;
            _mainWindow.ShowLeftPanel();

            // Animar la aparición del mensaje de configuración
            var fadeInPanel = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ConfigurationPanel.BeginAnimation(UIElement.OpacityProperty, fadeInPanel);

            // Esperar 3 segundos y navegar automáticamente
            await System.Threading.Tasks.Task.Delay(3000);
            NavigationService?.Navigate(new UserSetupPage(_mainWindow));
        }

        private System.Threading.Tasks.Task AnimateMessage(UIElement element, int delay)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideIn = new DoubleAnimation
            {
                From = 30,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var transform = new System.Windows.Media.TranslateTransform();
            element.RenderTransform = transform;

            fadeIn.Completed += (s, e) => tcs.SetResult(true);

            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideIn);

            return tcs.Task;
        }

    }
}
