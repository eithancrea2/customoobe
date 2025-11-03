using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CustomOOBE.Services
{
    public class UserService
    {
        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string pszProfilePath,
            uint cchProfilePath);

        public async Task<bool> CreateWindowsUserAsync(string username, string password = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Crear usuario usando net user
                    var createProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "net",
                            Arguments = string.IsNullOrEmpty(password)
                                ? $"user \"{username}\" /add"
                                : $"user \"{username}\" \"{password}\" /add",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            Verb = "runas"
                        }
                    };

                    createProcess.Start();
                    string output = createProcess.StandardOutput.ReadToEnd();
                    string error = createProcess.StandardError.ReadToEnd();
                    createProcess.WaitForExit();

                    if (createProcess.ExitCode != 0)
                    {
                        Debug.WriteLine($"Error al crear usuario: {error}");
                        return false;
                    }

                    // Agregar usuario al grupo de Administradores (opcional)
                    var addToGroupProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "net",
                            Arguments = $"localgroup Administrators \"{username}\" /add",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            Verb = "runas"
                        }
                    };

                    addToGroupProcess.Start();
                    addToGroupProcess.WaitForExit();

                    Debug.WriteLine($"Usuario '{username}' creado exitosamente");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al crear usuario de Windows: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> SetUserAvatarAsync(string username, string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Obtener el SID del usuario
                    var sid = GetUserSID(username);
                    if (string.IsNullOrEmpty(sid))
                    {
                        Debug.WriteLine("No se pudo obtener el SID del usuario");
                        return false;
                    }

                    // Ruta donde Windows guarda las imágenes de perfil
                    var profilePicturePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "Microsoft", "User Account Pictures", $"{username}.jpg");

                    // Crear directorio si no existe
                    var directory = Path.GetDirectoryName(profilePicturePath);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Copiar la imagen
                    File.Copy(imagePath, profilePicturePath, true);

                    // Actualizar el registro
                    var keyPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\AccountPicture\Users\{sid}";
                    using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath))
                    {
                        key?.SetValue("Image192", profilePicturePath);
                        key?.SetValue("Image448", profilePicturePath);
                    }

                    Debug.WriteLine($"Avatar establecido para usuario '{username}'");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al establecer avatar: {ex.Message}");
                    return false;
                }
            });
        }

        public string GetUserSID(string username)
        {
            try
            {
                var account = new NTAccount(username);
                var sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
                return sid.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener SID: {ex.Message}");
                return "";
            }
        }

        public bool UserExists(string username)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "net",
                        Arguments = "user",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains(username);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetAutoLoginAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    const string regPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";

                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue("AutoAdminLogon", "1");
                            key.SetValue("DefaultUserName", username);
                            key.SetValue("DefaultPassword", password);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al configurar auto-login: {ex.Message}");
                }
                return false;
            });
        }

        public string? GetFirstNonSystemUser()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "net",
                        Arguments = "user",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Usuarios del sistema que debemos ignorar
                string[] systemUsers = { "Administrator", "DefaultAccount", "Guest", "WDAGUtilityAccount" };

                // Parsear la salida del comando net user
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var users = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var user in users)
                    {
                        var trimmedUser = user.Trim();
                        if (!string.IsNullOrEmpty(trimmedUser) &&
                            !systemUsers.Any(su => su.Equals(trimmedUser, StringComparison.OrdinalIgnoreCase)) &&
                            !line.StartsWith("-") &&
                            !line.Contains("comando") &&
                            !line.Contains("completó"))
                        {
                            return trimmedUser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener usuarios: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> UpdateUserAsync(string username, string newAvatarPath)
        {
            // Actualizar solo el avatar del usuario existente
            if (!string.IsNullOrEmpty(newAvatarPath))
            {
                return await SetUserAvatarAsync(username, newAvatarPath);
            }
            return true;
        }
    }
}
