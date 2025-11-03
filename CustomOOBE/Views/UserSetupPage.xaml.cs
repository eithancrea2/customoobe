using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CustomOOBE.Services;
using Microsoft.Win32;

namespace CustomOOBE.Views
{
    public partial class UserSetupPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly UserService _userService;
        private string _selectedAvatarPath = "";
        private readonly string[] _defaultAvatars;
        private string? _existingUser = null;
        private bool _isEditMode = false;

        public UserSetupPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userService = new UserService();

            // Crear avatares por defecto si no existen
            _defaultAvatars = GenerateDefaultAvatars();
            LoadAvatars();

            // Verificar si ya existe un usuario
            _existingUser = _userService.GetFirstNonSystemUser();
            if (_existingUser != null)
            {
                _isEditMode = true;
                UsernameTextBox.Text = _existingUser;
                UsernameTextBox.IsEnabled = false; // No permitir cambiar el nombre
                NextButton.IsEnabled = true; // Habilitar el botón de continuar en modo edición
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(1);

            // Mostrar el panel izquierdo con animación
            _mainWindow.ShowLeftPanel();

            // Animación de entrada
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase()
            };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            if (!_isEditMode)
            {
                UsernameTextBox.Focus();
            }
        }

        private string[] GenerateDefaultAvatars()
        {
            // Crear directorio de avatares si no existe
            var avatarsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Avatars");
            Directory.CreateDirectory(avatarsPath);

            // Generar avatares con colores diferentes
            var colors = new[]
            {
                "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8",
                "#F7DC6F", "#BB8FCE", "#85C1E2", "#F8B195", "#C06C84"
            };

            var avatars = new string[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                var avatarPath = Path.Combine(avatarsPath, $"avatar_{i}.png");
                if (!File.Exists(avatarPath))
                {
                    CreateColorAvatar(avatarPath, colors[i], i + 1);
                }
                avatars[i] = avatarPath;
            }

            return avatars;
        }

        private void CreateColorAvatar(string path, string colorHex, int number)
        {
            try
            {
                const int size = 200;
                var renderBitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);

                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorHex);
                    var brush = new SolidColorBrush(color);

                    // Dibujar círculo de fondo
                    context.DrawEllipse(brush, null, new Point(size / 2, size / 2), size / 2, size / 2);

                    // Dibujar número
                    var text = new FormattedText(
                        number.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        80,
                        Brushes.White,
                        VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                    context.DrawText(text, new Point((size - text.Width) / 2, (size - text.Height) / 2));
                }

                renderBitmap.Render(visual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using var stream = File.Create(path);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear avatar: {ex.Message}");
            }
        }

        private void LoadAvatars()
        {
            foreach (var avatarPath in _defaultAvatars)
            {
                var border = new Border
                {
                    Width = 80,
                    Height = 80,
                    CornerRadius = new CornerRadius(40),
                    Margin = new Thickness(10),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Transparent,
                    Tag = avatarPath
                };

                var image = new Image
                {
                    Source = new BitmapImage(new Uri(avatarPath)),
                    Stretch = Stretch.UniformToFill
                };

                border.Child = image;
                border.MouseLeftButtonDown += AvatarBorder_Click;

                AvatarPanel.Children.Add(border);
            }

            // Seleccionar el primer avatar por defecto
            if (AvatarPanel.Children.Count > 0 && AvatarPanel.Children[0] is Border firstBorder)
            {
                SelectAvatar(firstBorder);
            }
        }

        private void AvatarBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                SelectAvatar(border);
            }
        }

        private void SelectAvatar(Border selectedBorder)
        {
            // Deseleccionar todos
            foreach (var child in AvatarPanel.Children)
            {
                if (child is Border border)
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }

            // Seleccionar el nuevo
            var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentBrush"];
            selectedBorder.BorderBrush = accentBrush;

            _selectedAvatarPath = selectedBorder.Tag?.ToString() ?? "";

            // Actualizar imagen grande
            if (!string.IsNullOrEmpty(_selectedAvatarPath))
            {
                SelectedAvatarImage.Source = new BitmapImage(new Uri(_selectedAvatarPath));
            }
        }

        private void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Seleccionar imagen de perfil"
                };

                if (dialog.ShowDialog() == true)
                {
                    _selectedAvatarPath = dialog.FileName;
                    SelectedAvatarImage.Source = new BitmapImage(new Uri(_selectedAvatarPath));

                    // Deseleccionar avatares predefinidos
                    foreach (var child in AvatarPanel.Children)
                    {
                        if (child is Border border)
                        {
                            border.BorderBrush = Brushes.Transparent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si estamos en modo edición, no validar el nombre de usuario
            if (_isEditMode)
            {
                return;
            }

            var username = UsernameTextBox.Text.Trim();

            // Validar nombre de usuario
            if (string.IsNullOrWhiteSpace(username))
            {
                NextButton.IsEnabled = false;
                UsernameErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            // Validar caracteres permitidos
            if (!username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            {
                NextButton.IsEnabled = false;
                UsernameErrorText.Text = "Solo se permiten letras, números, guiones y guiones bajos";
                UsernameErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Verificar si el usuario ya existe
            if (_userService.UserExists(username))
            {
                NextButton.IsEnabled = false;
                UsernameErrorText.Text = "Este nombre de usuario ya existe";
                UsernameErrorText.Visibility = Visibility.Visible;
                return;
            }

            NextButton.IsEnabled = true;
            UsernameErrorText.Visibility = Visibility.Collapsed;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // No permitir volver atrás
            e.Handled = true;
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Por favor ingresa un nombre de usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NextButton.IsEnabled = false;
            BackButton.IsEnabled = false;

            try
            {
                bool success;

                if (_isEditMode)
                {
                    // Modo edición: actualizar usuario existente
                    NextButton.Content = "Actualizando usuario...";
                    success = await _userService.UpdateUserAsync(username, _selectedAvatarPath);
                }
                else
                {
                    // Modo creación: crear nuevo usuario
                    NextButton.Content = "Creando usuario...";
                    success = await _userService.CreateWindowsUserAsync(username);

                    if (success && !string.IsNullOrEmpty(_selectedAvatarPath))
                    {
                        // Establecer avatar
                        await _userService.SetUserAvatarAsync(username, _selectedAvatarPath);
                    }
                }

                if (success)
                {
                    // Guardar en configuración
                    var configPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "CustomOOBE", "config.json");

                    // Navegar a la siguiente página
                    NavigationService?.Navigate(new NetworkSetupPage(_mainWindow, username));
                }
                else
                {
                    MessageBox.Show(_isEditMode
                        ? "No se pudo actualizar el usuario. Por favor intenta de nuevo."
                        : "No se pudo crear el usuario. Por favor intenta con otro nombre.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NextButton.IsEnabled = true;
                    BackButton.IsEnabled = true;
                    NextButton.Content = "Continuar";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NextButton.IsEnabled = true;
                BackButton.IsEnabled = true;
                NextButton.Content = "Continuar";
            }
        }
    }
}
