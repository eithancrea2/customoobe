using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CustomOOBE.Models;
using CustomOOBE.Services;

namespace CustomOOBE.Views
{
    public partial class InstallationPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly string _username;
        private readonly List<SoftwarePackage> _softwareToInstall;
        private readonly SoftwareService _softwareService;

        public InstallationPage(MainWindow mainWindow, string username, List<SoftwarePackage> softwareToInstall)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _username = username;
            _softwareToInstall = softwareToInstall;
            _softwareService = new SoftwareService();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(6);

            // Animación de entrada
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Iniciar animaciones de los círculos
            StartLoadingAnimation();

            // Si hay programas para instalar
            if (_softwareToInstall != null && _softwareToInstall.Count > 0)
            {
                await System.Threading.Tasks.Task.Delay(1500);

                var progress = new Progress<(string packageName, int progress)>(update =>
                {
                    StatusText.Text = $"Instalando {update.packageName}...";
                    ProgressText.Text = $"{update.progress}%";
                    ProgressBar.Value = update.progress;
                });

                await _softwareService.InstallSelectedSoftwareAsync(_softwareToInstall, progress);
            }
            else
            {
                // Simulación de configuración sin instalaciones
                for (int i = 0; i <= 100; i += 10)
                {
                    ProgressBar.Value = i;
                    ProgressText.Text = $"{i}%";
                    await System.Threading.Tasks.Task.Delay(200);
                }
            }

            // Mostrar mensaje final
            await ShowFinalMessage();
        }

        private void StartLoadingAnimation()
        {
            // Animación de rotación para los círculos
            var rotation1 = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotation2 = new DoubleAnimation
            {
                From = 360,
                To = 0,
                Duration = TimeSpan.FromSeconds(4),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotation3 = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2.5),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotateTransform1 = new System.Windows.Media.RotateTransform();
            var rotateTransform2 = new System.Windows.Media.RotateTransform();
            var rotateTransform3 = new System.Windows.Media.RotateTransform();

            LoadingCircle1.RenderTransformOrigin = new Point(0.5, 0.5);
            LoadingCircle2.RenderTransformOrigin = new Point(0.5, 0.5);
            LoadingCircle3.RenderTransformOrigin = new Point(0.5, 0.5);

            LoadingCircle1.RenderTransform = rotateTransform1;
            LoadingCircle2.RenderTransform = rotateTransform2;
            LoadingCircle3.RenderTransform = rotateTransform3;

            rotateTransform1.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotation1);
            rotateTransform2.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotation2);
            rotateTransform3.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotation3);
        }

        private async System.Threading.Tasks.Task ShowFinalMessage()
        {
            // Ocultar el panel izquierdo y las bolitas de progreso
            _mainWindow.HideLeftPanel();
            var progressGrid = _mainWindow.FindName("ProgressGrid") as FrameworkElement;
            if (progressGrid != null)
            {
                var fadeOutProgress = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                progressGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutProgress);
                await System.Threading.Tasks.Task.Delay(500);
                progressGrid.Visibility = Visibility.Collapsed;
            }

            // Fade out del contenido actual
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            await System.Threading.Tasks.Task.Delay(500);
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            await System.Threading.Tasks.Task.Delay(800);

            // Cambiar texto con estilos más grandes y centrados
            StatusText.Text = "¡Disfruta!";
            StatusText.FontSize = 48;
            StatusText.FontWeight = FontWeights.SemiBold;
            StatusText.TextAlignment = System.Windows.TextAlignment.Center;

            SubStatusText.Text = "Todo está listo para usar";
            SubStatusText.FontSize = 28;
            SubStatusText.TextAlignment = System.Windows.TextAlignment.Center;

            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressText.Visibility = Visibility.Collapsed;

            // Ocultar círculos de carga
            LoadingCircle1.Visibility = Visibility.Collapsed;
            LoadingCircle2.Visibility = Visibility.Collapsed;
            LoadingCircle3.Visibility = Visibility.Collapsed;

            // Fade in del mensaje final
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Esperar un poco antes de transicionar al escritorio
            await System.Threading.Tasks.Task.Delay(2000);

            // Transición suave al escritorio
            await TransitionToDesktop();
        }

        private async System.Threading.Tasks.Task TransitionToDesktop()
        {
            // Fade out de la música de fondo
            var audioService = _mainWindow.GetAudioService();
            var fadeOutTask = audioService.FadeOutMusicAsync(2000);

            // Fade out de toda la ventana
            var windowFadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            windowFadeOut.Completed += (s, e) =>
            {
                _mainWindow.AllowClose();
                Application.Current.Shutdown();
            };

            _mainWindow.BeginAnimation(UIElement.OpacityProperty, windowFadeOut);

            // Esperar a que termine el fade de música
            await fadeOutTask;
        }
    }
}
