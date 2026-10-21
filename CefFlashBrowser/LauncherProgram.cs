using CefFlashBrowser.FlashBrowser;
using CefFlashBrowser.Infrastructure;
using CefFlashBrowser.Singleton;
using CefSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace CefFlashBrowser
{
    internal static class LauncherProgram
    {
        private static Mutex _mutex;
        private static bool _cefStarted;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string pathName);

        [STAThread]
        private static void Main()
        {
            try
            {
                ClientPaths.Initialize();
                _mutex = new Mutex(true, "AvalonSeerClient", out bool isNewInstance);

                if (!isNewInstance)
                {
                    MsgReceiver.SendGlobalData(new byte[] { 1 });
                    return;
                }

                SetDllDirectory(ClientPaths.CefDirectory);
                AppDomain.CurrentDomain.AssemblyResolve += ResolveCefSharpAssembly;

                InitializeCefFlash();
                _cefStarted = true;
                ClientLogger.Info($"AvalonSeerClient {Assembly.GetExecutingAssembly().GetName().Version} started");

                var app = new LauncherApp();
                app.Run();
            }
            catch (Exception exception)
            {
                ClientLogger.Error("Unhandled application error", exception);
                MessageBox.Show(
                    exception.Message,
                    "Avalon 赛尔号客户端",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                if (_cefStarted)
                {
                    ShutdownCef();
                }

                _mutex?.Dispose();
            }
        }

        private static Assembly ResolveCefSharpAssembly(object sender, ResolveEventArgs e)
        {
            string assemblyName = new AssemblyName(e.Name).Name;
            string assemblyPath = Path.Combine(ClientPaths.CefDirectory, assemblyName + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }

        private static void InitializeCefFlash()
        {
            if (!File.Exists(ClientPaths.FlashPath))
            {
                throw new FileNotFoundException("未找到 Flash 插件。", ClientPaths.FlashPath);
            }

            Environment.SetEnvironmentVariable("ComSpec", ClientPaths.EmptyExePath);

            var settings = new CefFlashSettings
            {
                Locale = "zh-CN",
                LogFile = ClientPaths.CefLogPath,
                CachePath = ClientPaths.CacheDirectory,
                PpapiFlashPath = ClientPaths.FlashPath,
                PpapiFlashVersion = FileVersionInfo.GetVersionInfo(ClientPaths.FlashPath).FileVersion.Replace(',', '.'),
                EnableSystemFlash = true,
                BrowserSubprocessPath = ClientPaths.SubprocessPath
            };

#if !DEBUG
            settings.LogSeverity = LogSeverity.Error;
#endif

            settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";

            if (!Cef.Initialize(settings))
            {
                throw new InvalidOperationException("CEF 初始化失败。");
            }
        }

        private static void ShutdownCef()
        {
            Cef.Shutdown();
        }
    }
}
