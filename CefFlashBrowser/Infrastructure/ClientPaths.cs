using System;
using System.IO;

namespace CefFlashBrowser.Infrastructure
{
    public static class ClientPaths
    {
        public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string AssetsDirectory { get; } = Path.Combine(BaseDirectory, "Assets");
        public static string CefDirectory { get; } = Path.Combine(AssetsDirectory, "CefSharp");
        public static string PluginDirectory { get; } = Path.Combine(AssetsDirectory, "Plugins");
        public static string CacheDirectory { get; } = Path.Combine(BaseDirectory, "Caches");
        public static string LogDirectory { get; } = Path.Combine(BaseDirectory, "Logs");

        public static string AppLogPath { get; } = Path.Combine(LogDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        public static string CefLogPath { get; } = Path.Combine(LogDirectory, $"cef_{DateTime.Now:yyyyMMdd}.log");
        public static string FlashPath { get; } = Path.Combine(PluginDirectory, "pepflashplayer.dll");
        public static string EmptyExePath { get; } = Path.Combine(AssetsDirectory, "EmptyExe", "CefFlashBrowser.EmptyExe.exe");
        public static string SubprocessPath { get; } = Path.Combine(CefDirectory, "CefSharp.BrowserSubprocess.exe");
        public static string ClientConfigurationPath { get; } = Path.Combine(BaseDirectory, "client.json");
        public static string PetCalculatorPath { get; } = Path.Combine(BaseDirectory, "Widget", "SeerPetCalculator", "seer_pet_calculator.exe");

        public static void Initialize()
        {
            Directory.CreateDirectory(CacheDirectory);
            Directory.CreateDirectory(LogDirectory);
        }
    }
}
