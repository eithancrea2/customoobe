using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using CustomOOBE.Models;

namespace CustomOOBE.Services
{
    public class WiFiService
    {
        public async Task<List<WiFiNetwork>> GetAvailableNetworksAsync()
        {
            return await Task.Run(() =>
            {
                var networks = new List<WiFiNetwork>();

                try
                {
                    // Usar netsh para obtener las redes disponibles
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "wlan show networks mode=Bssid",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    WiFiNetwork? currentNetwork = null;

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();

                        if (trimmedLine.StartsWith("SSID"))
                        {
                            if (currentNetwork != null)
                            {
                                networks.Add(currentNetwork);
                            }

                            var ssid = trimmedLine.Split(':')[1].Trim();
                            if (!string.IsNullOrEmpty(ssid))
                            {
                                currentNetwork = new WiFiNetwork { SSID = ssid };
                            }
                        }
                        else if (currentNetwork != null)
                        {
                            if (trimmedLine.StartsWith("Authentication"))
                            {
                                var auth = trimmedLine.Split(':')[1].Trim();
                                currentNetwork.RequiresPassword = auth != "Open";
                                currentNetwork.SecurityType = auth;
                            }
                            else if (trimmedLine.StartsWith("Signal"))
                            {
                                var signal = trimmedLine.Split(':')[1].Trim().Replace("%", "");
                                if (int.TryParse(signal, out int signalValue))
                                {
                                    currentNetwork.SignalStrength = signalValue;
                                }
                            }
                        }
                    }

                    if (currentNetwork != null)
                    {
                        networks.Add(currentNetwork);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al obtener redes WiFi: {ex.Message}");
                }

                return networks.OrderByDescending(n => n.SignalStrength).ToList();
            });
        }

        public async Task<bool> ConnectToNetworkAsync(string ssid, string password = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Crear perfil XML para la red
                    var profileXml = string.IsNullOrEmpty(password)
                        ? CreateOpenNetworkProfile(ssid)
                        : CreateSecureNetworkProfile(ssid, password);

                    // Guardar el perfil
                    var tempFile = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllText(tempFile, profileXml);

                    // Agregar el perfil
                    var addProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"wlan add profile filename=\"{tempFile}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    addProcess.Start();
                    addProcess.WaitForExit();

                    System.IO.File.Delete(tempFile);

                    // Conectar a la red
                    var connectProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"wlan connect name=\"{ssid}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    connectProcess.Start();
                    connectProcess.WaitForExit();

                    // Esperar un momento para que se conecte
                    System.Threading.Thread.Sleep(3000);

                    return IsConnectedToInternet();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al conectar a la red: {ex.Message}");
                    return false;
                }
            });
        }

        public bool IsConnectedToInternet()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ping",
                        Arguments = "8.8.8.8 -n 1 -w 1000",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public bool IsEthernetConnected()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT NetConnectionStatus FROM Win32_NetworkAdapter WHERE NetConnectionID LIKE '%Ethernet%' OR NetConnectionID LIKE '%Local Area Connection%'");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var status = obj["NetConnectionStatus"];
                    if (status != null && (ushort)status == 2) // 2 = Connected
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar Ethernet: {ex.Message}");
            }

            return false;
        }

        private string CreateOpenNetworkProfile(string ssid)
        {
            return $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig>
        <SSID>
            <name>{ssid}</name>
        </SSID>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>auto</connectionMode>
    <MSM>
        <security>
            <authEncryption>
                <authentication>open</authentication>
                <encryption>none</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
        </security>
    </MSM>
</WLANProfile>";
        }

        private string CreateSecureNetworkProfile(string ssid, string password)
        {
            return $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig>
        <SSID>
            <name>{ssid}</name>
        </SSID>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>auto</connectionMode>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPA2PSK</authentication>
                <encryption>AES</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>passPhrase</keyType>
                <protected>false</protected>
                <keyMaterial>{password}</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";
        }
    }
}
