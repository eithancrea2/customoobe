using System;

namespace CustomOOBE.Models
{
    public class OOBEConfig
    {
        public string ComputerName { get; set; } = "Mi Equipo Premium";
        public bool FirstRun { get; set; } = true;
        public bool SetupCompleted { get; set; } = false;
        public string Theme { get; set; } = "auto";
        public string DatabasePath { get; set; } = "";
        public string Username { get; set; } = "";
        public string AvatarPath { get; set; } = "";
        public bool NetworkConfigured { get; set; } = false;
        public DateTime SetupDate { get; set; }
    }

    public class WiFiNetwork
    {
        public string SSID { get; set; } = "";
        public int SignalStrength { get; set; }
        public bool RequiresPassword { get; set; }
        public bool IsConnected { get; set; }
        public string SecurityType { get; set; } = "";
    }

    public class SoftwarePackage
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string IconPath { get; set; } = "";
        public bool IsSelected { get; set; }
        public long SizeInMB { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public string ComputerName { get; set; } = "";
        public string Username { get; set; } = "";
    }

    public class WallpaperOption
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string ThumbnailPath { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
