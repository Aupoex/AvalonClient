using CefFlashBrowser.Infrastructure;
using CefFlashBrowser.Views.Sidebar;
using CefSharp;
using CefSharp.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using FlashHandlers = CefFlashBrowser.FlashBrowser.Handlers;

namespace CefFlashBrowser.Views.Launcher
{
    public partial class GameWindow : Window
    {
        private sealed class GameRequestHandler : FlashHandlers.RequestHandler
        {
            private readonly GameWindow _window;

            public GameRequestHandler(GameWindow window)
            {
                _window = window;
            }

            public override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
            {
                if (!frame.IsMain || _window.IsGameUrl(request.Url))
                {
                    return false;
                }

                if (userGesture)
                {
                    _window.OpenExternalUrl(request.Url);
                }

                return true;
            }

            public override bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
            {
                _window.HandleNewWindowUrl(targetUrl, userGesture);
                return true;
            }
        }

        private sealed class GameLifeSpanHandler : FlashHandlers.LifeSpanHandler
        {
            private readonly GameWindow _window;

            public GameLifeSpanHandler(GameWindow window)
            {
                _window = window;
            }

            public override bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                if (!_window._isClosed && (browser.IsDisposed || !browser.IsPopup))
                {
                    _window.Dispatcher.Invoke(delegate
                    {
                        _window._doClose = true;
                        _window.Close();
                    });
                }

                return false;
            }

            public override bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                _window.HandleNewWindowUrl(targetUrl, userGesture);
                newBrowser = null;
                return true;
            }
        }

        private sealed class EmptyContextMenuHandler : FlashHandlers.ContextMenuHandler
        {
            public override void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.Clear();
            }
        }

        private sealed class DisableDragHandler : FlashHandlers.DragHandler
        {
            public override bool OnDragEnter(IWebBrowser chromiumWebBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask)
            {
                return true;
            }
        }

        private readonly Uri _gameOrigin;
        private ToolSidebar _toolSidebar;
        private bool _doClose;
        private bool _isClosed;

        public GameWindow()
        {
            InitializeComponent();

            var configuration = ClientConfiguration.Load();
            _gameOrigin = new Uri(configuration.GameUrl);

            browser.RequestHandler = new GameRequestHandler(this);
            browser.LifeSpanHandler = new GameLifeSpanHandler(this);
            browser.MenuHandler = new EmptyContextMenuHandler();
            browser.DragHandler = new DisableDragHandler();
            browser.Address = configuration.GameUrl;

            Loaded += GameWindowLoaded;
            LocationChanged += delegate { PositionToolSidebar(); };
            StateChanged += GameWindowStateChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!browser.IsDisposed && !_doClose)
            {
                var cefBrowser = browser.GetBrowser();
                if (cefBrowser == null)
                {
                    _doClose = true;
                }
                else
                {
                    cefBrowser.CloseBrowser(true);
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _isClosed = true;
            _toolSidebar?.Close();
            base.OnClosed(e);
        }

        private void GameWindowLoaded(object sender, RoutedEventArgs e)
        {
            _toolSidebar = new ToolSidebar { Owner = this };
            _toolSidebar.Show();
            PositionToolSidebar();
        }

        private void GameWindowStateChanged(object sender, EventArgs e)
        {
            if (_toolSidebar == null)
            {
                return;
            }

            if (WindowState == WindowState.Minimized)
            {
                _toolSidebar.Hide();
            }
            else
            {
                _toolSidebar.Show();
                PositionToolSidebar();
            }
        }

        private void PositionToolSidebar()
        {
            if (_toolSidebar == null || !browser.IsLoaded)
            {
                return;
            }

            Point screenPoint = browser.PointToScreen(new Point(browser.ActualWidth, 0));
            PresentationSource source = PresentationSource.FromVisual(browser);
            if (source?.CompositionTarget != null)
            {
                screenPoint = source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
            }

            _toolSidebar.Left = screenPoint.X;
            _toolSidebar.Top = screenPoint.Y;
        }

        private bool IsGameUrl(string url)
        {
            if (string.Equals(url, "about:blank", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri candidate))
            {
                return false;
            }

            return string.Equals(candidate.Scheme, _gameOrigin.Scheme, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.Host, _gameOrigin.Host, StringComparison.OrdinalIgnoreCase)
                && candidate.Port == _gameOrigin.Port;
        }

        private void HandleNewWindowUrl(string url, bool userGesture)
        {
            Dispatcher.InvokeAsync(delegate
            {
                if (IsGameUrl(url))
                {
                    browser.Load(url);
                }
                else if (userGesture)
                {
                    OpenExternalUrl(url);
                }
            });
        }

        private void OpenExternalUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                ClientLogger.Error($"Failed to open external URL: {url}", exception);
            }
        }
    }
}
