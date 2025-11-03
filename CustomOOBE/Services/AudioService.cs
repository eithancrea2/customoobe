using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace CustomOOBE.Services
{
    public class AudioService
    {
        private MediaPlayer? _backgroundMusicPlayer;
        private double _musicVolume = 0.3; // Volumen bajo por defecto

        // Importar función de Windows para reproducir sonidos del sistema
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

        private const uint SND_FILENAME = 0x00020000;
        private const uint SND_ASYNC = 0x0001;
        private const uint SND_ALIAS = 0x00010000;

        public void PlayWindowsSound(string soundAlias)
        {
            try
            {
                // Reproducir sonido del sistema de Windows
                PlaySound(soundAlias, IntPtr.Zero, SND_ALIAS | SND_ASYNC);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir sonido: {ex.Message}");
            }
        }

        public void PlayNavigationSound()
        {
            // Sonido para navegación entre páginas
            PlayWindowsSound("SystemAsterisk");
        }

        public void PlaySuccessSound()
        {
            // Sonido para operaciones exitosas
            PlayWindowsSound("SystemExclamation");
        }

        public void PlayErrorSound()
        {
            // Sonido para errores
            PlayWindowsSound("SystemHand");
        }

        public void PlayClickSound()
        {
            // Sonido para clicks
            try
            {
                using var player = new SoundPlayer();
                // Usar sonido simple de click
                PlaySound("SystemDefault", IntPtr.Zero, SND_ALIAS | SND_ASYNC);
            }
            catch { }
        }

        public void StartBackgroundMusic(string? musicPath = null)
        {
            try
            {
                _backgroundMusicPlayer = new MediaPlayer();

                if (!string.IsNullOrEmpty(musicPath) && File.Exists(musicPath))
                {
                    // Usar música personalizada
                    _backgroundMusicPlayer.Open(new Uri(musicPath));
                }
                else
                {
                    // Sin música de fondo por defecto
                    return;
                }

                _backgroundMusicPlayer.Volume = _musicVolume;
                _backgroundMusicPlayer.MediaEnded += (s, e) =>
                {
                    // Repetir la música
                    _backgroundMusicPlayer.Position = TimeSpan.Zero;
                    _backgroundMusicPlayer.Play();
                };

                _backgroundMusicPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al iniciar música de fondo: {ex.Message}");
            }
        }

        public void SetMusicVolume(double volume)
        {
            _musicVolume = Math.Clamp(volume, 0, 1);
            if (_backgroundMusicPlayer != null)
            {
                _backgroundMusicPlayer.Volume = _musicVolume;
            }
        }

        public void ToggleMuteMusic()
        {
            if (_backgroundMusicPlayer != null)
            {
                _backgroundMusicPlayer.Volume = _backgroundMusicPlayer.Volume > 0 ? 0 : _musicVolume;
            }
        }

        public bool IsMusicMuted()
        {
            return _backgroundMusicPlayer?.Volume == 0;
        }

        public async System.Threading.Tasks.Task FadeOutMusicAsync(int durationMs = 2000)
        {
            if (_backgroundMusicPlayer == null) return;

            var initialVolume = _backgroundMusicPlayer.Volume;
            var steps = 20;
            var stepDuration = durationMs / steps;
            var volumeStep = initialVolume / steps;

            for (int i = 0; i < steps; i++)
            {
                _backgroundMusicPlayer.Volume -= volumeStep;
                await System.Threading.Tasks.Task.Delay(stepDuration);
            }

            _backgroundMusicPlayer.Stop();
            _backgroundMusicPlayer.Volume = initialVolume;
        }

        public void StopBackgroundMusic()
        {
            _backgroundMusicPlayer?.Stop();
            _backgroundMusicPlayer?.Close();
            _backgroundMusicPlayer = null;
        }

        public void Dispose()
        {
            StopBackgroundMusic();
        }
    }
}
