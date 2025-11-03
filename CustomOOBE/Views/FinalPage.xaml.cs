using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CustomOOBE.Views
{
    public partial class FinalPage : Page
    {
        private readonly MainWindow _mainWindow;

        public FinalPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateProgressIndicator(6);

            // Animación de entrada
            await AnimateMessage(Message1);
            await System.Threading.Tasks.Task.Delay(2000);

            await AnimateMessage(Message2);
            await System.Threading.Tasks.Task.Delay(2000);

            await AnimateMessage(Message3);
            await System.Threading.Tasks.Task.Delay(3000);

            // Marcar setup como completado
            MarkSetupCompleted();

            // Permitir cerrar y finalizar
            _mainWindow.AllowClose();

            // Cerrar el OOBE
            Application.Current.Shutdown();
        }

        private System.Threading.Tasks.Task AnimateMessage(UIElement element)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            fadeIn.Completed += (s, e) => tcs.SetResult(true);
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            return tcs.Task;
        }

        private void MarkSetupCompleted()
        {
            try
            {
                var configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "CustomOOBE", "config.json");

                var config = new
                {
                    FirstRun = false,
                    SetupCompleted = true,
                    SetupDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                File.WriteAllText(configPath, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));

                // Eliminar la tarea programada
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = "/Delete /TN \"CustomOOBE\" /F",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al marcar setup completado: {ex.Message}");
            }
        }
    }
}
