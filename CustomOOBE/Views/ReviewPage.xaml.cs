using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CustomOOBE.Models;
using CustomOOBE.Services;

namespace CustomOOBE.Views
{
    public partial class ReviewPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly string _username;
        private readonly DatabaseService _databaseService;
        private int _rating = 0;
        private readonly Button[] _stars;

        public ReviewPage(MainWindow mainWindow, string username)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _username = username;

            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CustomOOBE", "reviews.db");
            _databaseService = new DatabaseService(dbPath);

            _stars = new[] { Star1, Star2, Star3, Star4, Star5 };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(5);
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int rating))
            {
                _rating = rating;
                UpdateStars();
                SubmitButton.IsEnabled = true;
            }
        }

        private void UpdateStars()
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].Opacity = i < _rating ? 1.0 : 0.3;
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e) =>
            NavigationService?.Navigate(new FinalPage(_mainWindow));

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Enviando...";

            var review = new Review
            {
                Date = DateTime.Now,
                Rating = _rating,
                Comment = CommentBox.Text.Trim(),
                ComputerName = Environment.MachineName,
                Username = _username
            };

            await _databaseService.SaveReviewAsync(review);

            NavigationService?.Navigate(new FinalPage(_mainWindow));
        }
    }
}
