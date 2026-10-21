using CefFlashBrowser.Infrastructure;
using CefFlashBrowser.Singleton;
using CefFlashBrowser.Views.Launcher;
using System.Windows;

namespace CefFlashBrowser
{
    public sealed class LauncherApp : Application
    {
        private readonly MsgReceiver _messageReceiver;
        private GameWindow _gameWindow;

        public LauncherApp()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            _messageReceiver = new MsgReceiver();
            _messageReceiver.ReceivedData += ReceivedDataHandler;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShowGameWindow();
        }

        private void ReceivedDataHandler(object sender, ReceivedDataEventArgs e)
        {
            Dispatcher.InvokeAsync(ShowGameWindow);
        }

        private void ShowGameWindow()
        {
            if (_gameWindow == null)
            {
                _gameWindow = new GameWindow();
                _gameWindow.Closed += delegate { _gameWindow = null; };
                MainWindow = _gameWindow;
                _gameWindow.Show();
                return;
            }

            if (_gameWindow.WindowState == WindowState.Minimized)
            {
                _gameWindow.WindowState = WindowState.Normal;
            }

            _gameWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _messageReceiver.ReceivedData -= ReceivedDataHandler;
            ClientLogger.Info("Application exited");
            base.OnExit(e);
        }
    }
}
