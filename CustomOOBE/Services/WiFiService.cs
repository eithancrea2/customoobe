using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using CustomOOBE.Models;
using SimpleWifi;

namespace CustomOOBE.Services
{
    public class WiFiService
    {
        private readonly Wifi _wifi;

        public WiFiService()
        {
            _wifi = new Wifi();
        }

        public async Task<List<WiFiNetwork>> GetAvailableNetworksAsync()
        {
            return await Task.Run(() =>
            {
                var networks = new List<WiFiNetwork>();

                try
                {
                    var accessPoints = _wifi.GetAccessPoints();

                    foreach (var ap in accessPoints)
                    {
                        networks.Add(new WiFiNetwork
                        {
                            SSID = ap.Name,
                            SignalStrength = (int)ap.SignalStrength,
                            RequiresPassword = ap.IsSecure,
                            SecurityType = ap.IsSecure ? "WPA2" : "Open",
                            IsConnected = ap.IsConnected
                        });
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
                    var accessPoints = _wifi.GetAccessPoints().Where(ap => ap.Name == ssid).ToList();
                    
                    if (!accessPoints.Any())
                    {
                        Debug.WriteLine($"Red {ssid} no encontrada");
                        return false;
                    }

                    var accessPoint = accessPoints.First();
                    AuthRequest authRequest;

                    if (accessPoint.IsSecure && !string.IsNullOrEmpty(password))
                    {
                        authRequest = new AuthRequest(accessPoint)
                        {
                            Password = password
                        };
                    }
                    else
                    {
                        authRequest = new AuthRequest(accessPoint);
                    }

                    var connected = accessPoint.Connect(authRequest);

                    if (connected)
                    {
                        // Esperar un momento para que se establezca la conexión
                        System.Threading.Thread.Sleep(3000);
                        return IsConnectedToInternet();
                    }

                    return false;
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

        public bool IsWiFiConnected()
        {
            try
            {
                var accessPoints = _wifi.GetAccessPoints();
                return accessPoints.Any(ap => ap.IsConnected);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar WiFi: {ex.Message}");
                return false;
            }
        }
    }
}
