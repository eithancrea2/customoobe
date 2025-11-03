using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CustomOOBE.Models;
using CustomOOBE.Services;

namespace CustomOOBE.Views
{
    public partial class ThemeSetupPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly string _username;
        private readonly ThemeService _themeService;
        private readonly List<SoftwarePackage>? _softwareToInstall;
        private bool _isDarkTheme;
        private string _selectedWallpaper = "";
        private string _selectedLockScreen = "";

        public ThemeSetupPage(MainWindow mainWindow, string username, List<SoftwarePackage>? softwareToInstall = null)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _username = username;
            _softwareToInstall = softwareToInstall;
            _themeService = new ThemeService();
            _isDarkTheme = _themeService.IsDarkTime();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(4);
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            LoadWallpapers();
            LoadLockScreens();
            UpdateThemeButtons();
        }

        private void LoadWallpapers()
        {
            var wallpapersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Wallpapers");
            Directory.CreateDirectory(wallpapersPath);

            // Crear wallpapers de ejemplo si no existen
            var colors = new[] { "#667EEA", "#764BA2", "#F093FB", "#4FACFE", "#43E97B", "#FA709A" };
            for (int i = 0; i < colors.Length; i++)
            {
                var wallpaperPath = Path.Combine(wallpapersPath, $"wallpaper_{i}.png");
                if (!File.Exists(wallpaperPath))
                    CreateGradientWallpaper(wallpaperPath, colors[i], colors[(i + 1) % colors.Length]);

                AddWallpaperOption(wallpaperPath);
            }
        }

        private void CreateGradientWallpaper(string path, string color1, string color2)
        {
            try
            {
                var renderBitmap = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    var brush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop((Color)ColorConverter.ConvertFromString(color1), 0),
                            new GradientStop((Color)ColorConverter.ConvertFromString(color2), 1)
                        }
                    };
                    context.DrawRectangle(brush, null, new Rect(0, 0, 1920, 1080));
                }
                renderBitmap.Render(visual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                using var stream = File.Create(path);
                encoder.Save(stream);
            }
            catch { }
        }

        private void AddWallpaperOption(string path)
        {
            var border = new Border
            {
                Width = 150,
                Height = 85,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Transparent,
                Tag = path
            };

            var image = new Image { Source = new BitmapImage(new Uri(path)), Stretch = Stretch.UniformToFill };
            border.Child = image;
            border.MouseLeftButtonDown += (s, e) => SelectWallpaper(border);
            WallpapersPanel.Children.Add(border);

            if (WallpapersPanel.Children.Count == 1) SelectWallpaper(border);
        }

        private void LoadLockScreens()
        {
            var lockScreenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "LockScreens");
            Directory.CreateDirectory(lockScreenPath);

            // Crear imágenes de pantalla de bloqueo si no existen
            var colors = new[] { "#1E3A8A", "#7C2D12", "#14532D", "#581C87", "#831843", "#422006" };
            for (int i = 0; i < colors.Length; i++)
            {
                var imagePath = Path.Combine(lockScreenPath, $"lockscreen_{i}.png");
                if (!File.Exists(imagePath))
                    CreateGradientWallpaper(imagePath, colors[i], colors[(i + 1) % colors.Length]);

                AddLockScreenOption(imagePath);
            }
        }

        private void AddLockScreenOption(string path)
        {
            var border = new Border
            {
                Width = 150,
                Height = 85,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Transparent,
                Tag = path
            };

            var image = new Image { Source = new BitmapImage(new Uri(path)), Stretch = Stretch.UniformToFill };
            border.Child = image;
            border.MouseLeftButtonDown += (s, e) => SelectLockScreen(border);
            LockScreenPanel.Children.Add(border);

            if (LockScreenPanel.Children.Count == 1) SelectLockScreen(border);
        }

        private void SelectWallpaper(Border selected)
        {
            foreach (var child in WallpapersPanel.Children)
                if (child is Border b) b.BorderBrush = Brushes.Transparent;

            selected.BorderBrush = (SolidColorBrush)Application.Current.Resources["AccentBrush"];
            _selectedWallpaper = selected.Tag?.ToString() ?? "";

            // Actualizar preview
            UpdatePreview(_selectedWallpaper, "Fondo de escritorio");
        }

        private void SelectLockScreen(Border selected)
        {
            foreach (var child in LockScreenPanel.Children)
                if (child is Border b) b.BorderBrush = Brushes.Transparent;

            selected.BorderBrush = (SolidColorBrush)Application.Current.Resources["AccentBrush"];
            _selectedLockScreen = selected.Tag?.ToString() ?? "";

            // Actualizar preview
            UpdatePreview(_selectedLockScreen, "Pantalla de bloqueo");
        }

        private void UpdatePreview(string imagePath, string type)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                PreviewImage.Source = new BitmapImage(new Uri(imagePath));
                PreviewTypeText.Text = type;
            }
        }

        private void UpdateThemeButtons()
        {
            var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentBrush"];
            var secondaryBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"];

            LightThemeBtn.Background = _isDarkTheme ? secondaryBrush : accentBrush;
            DarkThemeBtn.Background = _isDarkTheme ? accentBrush : secondaryBrush;
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = false;
            App.ApplyTheme("Light");
            UpdateThemeButtons();
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = true;
            App.ApplyTheme("Dark");
            UpdateThemeButtons();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextButton.IsEnabled = false;
            NextButton.Content = "Aplicando...";

            await _themeService.ApplyWindowsThemeAsync(_isDarkTheme);

            if (!string.IsNullOrEmpty(_selectedWallpaper))
                await _themeService.SetWallpaperAsync(_selectedWallpaper);

            if (!string.IsNullOrEmpty(_selectedLockScreen))
                await _themeService.SetLockScreenAsync(_selectedLockScreen);

            NavigationService?.Navigate(new ReviewPage(_mainWindow, _username, _softwareToInstall));
        }
    }
}
