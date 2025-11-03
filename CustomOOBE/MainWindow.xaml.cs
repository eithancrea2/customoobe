using System;
using System.Collections.Generic;
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
        private List<SecondaryMonitorOverlay> _secondaryOverlays = new List<SecondaryMonitorOverlay>();

        public MainWindow()
        {
            InitializeComponent();

            _keyboardBlocker = new KeyboardBlocker();
            _taskManagerBlocker = new TaskManagerBlocker();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            SetupProgressIndicator();
            PositionOnPrimaryMonitor();
            CreateSecondaryMonitorOverlays();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Activar bloqueo de teclas
            _keyboardBlocker.StartBlocking();
            _taskManagerBlocker.StartBlocking();

            // Iniciar animación de fondo
            StartBackgroundAnimation();

            // Navegar a la primera pantalla
            ContentFrame.Navigate(new WelcomePage(this));
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Desactivar bloqueos
            _keyboardBlocker.StopBlocking();
            _taskManagerBlocker.StopBlocking();
            _animationTimer.Stop();

            // Cerrar todos los overlays secundarios
            foreach (var overlay in _secondaryOverlays)
            {
                overlay.Close();
            }
            _secondaryOverlays.Clear();
        }

        private void PositionOnPrimaryMonitor()
        {
            try
            {
                // Obtener el monitor primario
                var primaryScreen = Screen.PrimaryScreen;
                if (primaryScreen != null)
                {
                    // Posicionar la ventana en el monitor primario
                    this.Left = primaryScreen.Bounds.Left;
                    this.Top = primaryScreen.Bounds.Top;
                    this.Width = primaryScreen.Bounds.Width;
                    this.Height = primaryScreen.Bounds.Height;

                    // Ajustar el ancho del panel izquierdo según la resolución
                    AdjustLeftPanelWidth(primaryScreen.Bounds.Width);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error positioning on primary monitor: {ex.Message}");
            }
        }

        private void AdjustLeftPanelWidth(int screenWidth)
        {
            // Calcular el ancho del panel izquierdo basado en la resolución
            // Para resoluciones pequeñas (< 1366px), usar 300px
            // Para resoluciones medianas (1366-1920px), usar 35% del ancho
            // Para resoluciones grandes (> 1920px), usar 500px max

            double leftPanelWidth;

            if (screenWidth < 1366)
            {
                leftPanelWidth = 300;
            }
            else if (screenWidth <= 1920)
            {
                leftPanelWidth = screenWidth * 0.35;
            }
            else
            {
                leftPanelWidth = 500;
            }

            // Actualizar el ancho del panel izquierdo
            var grid = this.Content as Grid;
            if (grid != null && grid.ColumnDefinitions.Count > 0)
            {
                grid.ColumnDefinitions[0].Width = new GridLength(leftPanelWidth);
            }
        }

        private void CreateSecondaryMonitorOverlays()
        {
            try
            {
                var screens = Screen.AllScreens;
                var primaryScreen = Screen.PrimaryScreen;

                foreach (var screen in screens)
                {
                    // Saltar el monitor primario
                    if (screen.Equals(primaryScreen))
                        continue;

                    // Crear un overlay para cada monitor secundario
                    var overlay = new SecondaryMonitorOverlay
                    {
                        Left = screen.Bounds.Left,
                        Top = screen.Bounds.Top,
                        Width = screen.Bounds.Width,
                        Height = screen.Bounds.Height,
                        WindowState = WindowState.Normal
                    };

                    overlay.Show();
                    _secondaryOverlays.Add(overlay);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating secondary overlays: {ex.Message}");
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
            var size = _random.Next(30, 150);
            var circle = new Ellipse
            {
                Width = size,
                Height = size,
                Opacity = 0.1
            };

            // Color basado en el tema actual
            var accentColor = (Color)Application.Current.Resources["AccentColor"];
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, accentColor.R, accentColor.G, accentColor.B), 0),
                    new GradientStop(Colors.Transparent, 1)
                }
            };
            circle.Fill = brush;

            var startX = _random.Next(0, (int)AnimationCanvas.ActualWidth);
            var startY = (int)AnimationCanvas.ActualHeight;

            Canvas.SetLeft(circle, startX);
            Canvas.SetTop(circle, startY);

            AnimationCanvas.Children.Add(circle);

            // Animación de movimiento
            var moveAnimation = new DoubleAnimation
            {
                From = startY,
                To = -size,
                Duration = TimeSpan.FromSeconds(_random.Next(8, 15)),
                EasingFunction = new QuadraticEase()
            };

            // Animación de movimiento horizontal
            var horizontalAnimation = new DoubleAnimation
            {
                From = startX,
                To = startX + _random.Next(-200, 200),
                Duration = TimeSpan.FromSeconds(_random.Next(8, 15)),
                EasingFunction = new SineEase()
            };

            // Animación de opacidad
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.15,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true
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
    }
}
