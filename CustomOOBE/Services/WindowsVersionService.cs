using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CustomOOBE.Services
{
    public class WindowsVersionService
    {
        public enum WindowsVersion
        {
            Unknown,
            Windows10,
            Windows11
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        public WindowsVersion GetWindowsVersion()
        {
            try
            {
                var versionInfo = new OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };

                if (RtlGetVersion(ref versionInfo) == 0)
                {
                    // Windows 11 tiene build 22000 o superior
                    // Windows 10 tiene builds desde 10240 hasta 19045
                    if (versionInfo.dwMajorVersion == 10 && versionInfo.dwBuildNumber >= 22000)
                    {
                        return WindowsVersion.Windows11;
                    }
                    else if (versionInfo.dwMajorVersion == 10)
                    {
                        return WindowsVersion.Windows10;
                    }
                }

                // Método alternativo usando el registro
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        var currentBuild = key.GetValue("CurrentBuild")?.ToString();
                        if (!string.IsNullOrEmpty(currentBuild) && int.TryParse(currentBuild, out int buildNumber))
                        {
                            if (buildNumber >= 22000)
                                return WindowsVersion.Windows11;
                            else if (buildNumber >= 10240)
                                return WindowsVersion.Windows10;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al detectar versión de Windows: {ex.Message}");
            }

            return WindowsVersion.Unknown;
        }

        public string GetWindowsVersionString()
        {
            var version = GetWindowsVersion();
            return version switch
            {
                WindowsVersion.Windows11 => "Windows 11",
                WindowsVersion.Windows10 => "Windows 10",
                _ => "Windows"
            };
        }

        public bool IsWindows11()
        {
            return GetWindowsVersion() == WindowsVersion.Windows11;
        }

        public bool IsWindows10()
        {
            return GetWindowsVersion() == WindowsVersion.Windows10;
        }
    }
}
