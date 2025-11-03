using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CustomOOBE.Services;
using CustomOOBE.Views;

namespace CustomOOBE
{
    public partial class MainWindow : Window
    {
        private KeyboardBlocker _keyboardBlocker;
        private TaskManagerBlocker _taskManagerBlocker;
        private int _currentStep = 0;
        private readonly DispatcherTimer _animationTimer;
        private readonly AudioService _audioService;
        private List<SecondaryDisplayWindow> _secondaryWindows = new List<SecondaryDisplayWindow>();

        public MainWindow()
        {
            InitializeComponent();

            _keyboardBlocker = new KeyboardBlocker();
            _taskManagerBlocker = new TaskManagerBlocker();
            _audioService = new AudioService();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            SetupProgressIndicator();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Activar bloqueo de teclas
            _keyboardBlocker.StartBlocking();
            _taskManagerBlocker.StartBlocking();

            // Crear ventanas en monitores secundarios
            CreateSecondaryDisplayWindows();

            // Posicionar la ventana principal en el monitor primario
            PositionOnPrimaryScreen();

            // Iniciar animación de fondo (pero el panel está oculto inicialmente)
            StartBackgroundAnimation();

            // Iniciar música de fondo si existe un archivo (opcional)
            // _audioService.StartBackgroundMusic("path/to/music.mp3");
            // MuteButton.Visibility = Visibility.Visible;

            // Navegar a la primera pantalla
            ContentFrame.Navigate(new WelcomePage(this));
        }

        private void CreateSecondaryDisplayWindows()
        {
            // Obtener todas las pantallas excepto la primaria
            var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();

            foreach (var screen in screens)
            {
                var secondaryWindow = new SecondaryDisplayWindow();

                // Posicionar en la pantalla secundaria
                secondaryWindow.Left = screen.Bounds.Left;
                secondaryWindow.Top = screen.Bounds.Top;
                secondaryWindow.Width = screen.Bounds.Width;
                secondaryWindow.Height = screen.Bounds.Height;

                secondaryWindow.Show();
                _secondaryWindows.Add(secondaryWindow);
            }
        }

        private void PositionOnPrimaryScreen()
        {
            var primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen != null)
            {
                this.Left = primaryScreen.Bounds.Left;
                this.Top = primaryScreen.Bounds.Top;
                this.Width = primaryScreen.Bounds.Width;
                this.Height = primaryScreen.Bounds.Height;
            }
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Desactivar bloqueos
            _keyboardBlocker.StopBlocking();
            _taskManagerBlocker.StopBlocking();
            _animationTimer.Stop();
            _audioService.Dispose();

            // Cerrar ventanas secundarias
            foreach (var window in _secondaryWindows)
            {
                window.Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Bloquear teclas adicionales a nivel de ventana
            if (e.Key == Key.Escape || e.Key == Key.F4 ||
                (e.Key == Key.System && e.SystemKey == Key.F4))
            {
                e.Handled = true;
            }
        }

        private void SetupProgressIndicator()
        {
            // 7 pasos en total
            for (int i = 0; i < 7; i++)
            {
                var circle = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = Brushes.Gray,
                    Margin = new Thickness(5, 0, 5, 0),
                    Opacity = 0.3
                };

                ProgressIndicator.Children.Add(circle);
            }

            UpdateProgressIndicator(0);
        }

        public void UpdateProgressIndicator(int step)
        {
            _currentStep = step;

            for (int i = 0; i < ProgressIndicator.Children.Count; i++)
            {
                if (ProgressIndicator.Children[i] is Ellipse circle)
                {
                    var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentBrush"];

                    if (i < step)
                    {
                        // Completado
                        circle.Fill = accentBrush;
                        circle.Opacity = 1.0;
                    }
                    else if (i == step)
                    {
                        // Actual
                        circle.Fill = accentBrush;
                        circle.Opacity = 1.0;

                        // Animación de pulso
                        var pulseAnimation = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.5,
                            Duration = TimeSpan.FromSeconds(0.8),
                            AutoReverse = true,
                            RepeatBehavior = RepeatBehavior.Forever
                        };
                        circle.BeginAnimation(OpacityProperty, pulseAnimation);
                    }
                    else
                    {
                        // Pendiente
                        circle.Fill = Brushes.Gray;
                        circle.Opacity = 0.3;
                        circle.BeginAnimation(OpacityProperty, null);
                    }
                }
            }
        }

        private void StartBackgroundAnimation()
        {
            _animationTimer.Start();
        }

        private Random _random = new Random();
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Crear círculos flotantes animados (estilo macOS)
            if (_random.Next(0, 10) < 3)
            {
                CreateFloatingCircle();
            }

            // Limpiar círculos viejos
            if (AnimationCanvas.Children.Count > 30)
            {
                AnimationCanvas.Children.RemoveAt(0);
            }
        }

        private void CreateFloatingCircle()
        {
            var size = _random.Next(40, 180);
            var circle = new Ellipse
            {
                Width = size,
                Height = size,
                Opacity = 0
            };

            // Color basado en el tema actual con más contraste
            var accentColor = (Color)Application.Current.Resources["AccentColor"];
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(80, accentColor.R, accentColor.G, accentColor.B), 0),
                    new GradientStop(Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B), 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }
            };
            circle.Fill = brush;

            var startX = _random.Next(0, (int)AnimationCanvas.ActualWidth);
            var startY = (int)AnimationCanvas.ActualHeight;

            Canvas.SetLeft(circle, startX);
            Canvas.SetTop(circle, startY);

            AnimationCanvas.Children.Add(circle);

            var duration = _random.Next(10, 20);

            // Animación de movimiento vertical - más suave
            var moveAnimation = new DoubleAnimation
            {
                From = startY,
                To = -size,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Animación de movimiento horizontal - efecto de deriva
            var horizontalAnimation = new DoubleAnimation
            {
                From = startX,
                To = startX + _random.Next(-250, 250),
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            // Animación de opacidad - fade in/out más suave
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.20,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            moveAnimation.Completed += (s, e) =>
            {
                AnimationCanvas.Children.Remove(circle);
            };

            circle.BeginAnimation(Canvas.TopProperty, moveAnimation);
            circle.BeginAnimation(Canvas.LeftProperty, horizontalAnimation);
            circle.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        public void AllowClose()
        {
            _keyboardBlocker.StopBlocking();
            _taskManagerBlocker.StopBlocking();
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            _audioService.ToggleMuteMusic();
            // Actualizar icono del botón
            // Nota: Esto requiere acceso al TextBlock dentro del template, por simplicidad usamos el evento
        }

        public AudioService GetAudioService() => _audioService;

        public void ShowLeftPanel()
        {
            // Animar la aparición del panel izquierdo
            var widthAnimation = new DoubleAnimation
            {
                From = 0,
                To = this.ActualWidth * 0.4, // 40% del ancho de la pantalla
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            LeftPanel.BeginAnimation(WidthProperty, widthAnimation);
        }

        public void HideLeftPanel()
        {
            // Animar el ocultamiento del panel izquierdo
            var widthAnimation = new DoubleAnimation
            {
                From = LeftPanel.Width,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            LeftPanel.BeginAnimation(WidthProperty, widthAnimation);
        }
    }
}
