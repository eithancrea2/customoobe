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

            // Cargar nombre del equipo desde configuración
            var computerName = Environment.GetEnvironmentVariable("COMPUTER_NAME") ?? "Equipo Premium";
            ComputerNameRun.Text = computerName;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(0);
            StartWelcomeAnimation();
        }

        private async void StartWelcomeAnimation()
        {
            // Animación secuencial de los mensajes
            await AnimateMessage(Message1, 0);
            await System.Threading.Tasks.Task.Delay(2500);

            await AnimateMessage(Message2, 0);
            await System.Threading.Tasks.Task.Delay(2500);

            await AnimateMessage(Message3, 0);
            await System.Threading.Tasks.Task.Delay(1500);

            await AnimateMessage(ContinueButton, 0);
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

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a la página de creación de usuario
            NavigationService?.Navigate(new UserSetupPage(_mainWindow));
        }
    }
}
