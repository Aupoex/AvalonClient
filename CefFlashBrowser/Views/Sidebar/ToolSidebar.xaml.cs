using System.Windows;

namespace CefFlashBrowser.Views.Sidebar
{
    public partial class ToolSidebar : Window
    {
        private const double CollapsedWidth = 44;
        private const double ExpandedWidth = 300;

        public ToolSidebar()
        {
            InitializeComponent();
        }

        private void SidebarToggleClick(object sender, RoutedEventArgs e)
        {
            bool expanded = sidebarToggle.IsChecked == true;

            Width = expanded ? ExpandedWidth : CollapsedWidth;
            Height = expanded ? 560 : 44;
            sidebarBackground.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            sidebarTitle.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            moduleContainer.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            sidebarToggle.Content = expanded ? "<" : "☰";
            sidebarToggle.ToolTip = expanded ? "收起工具侧边栏" : "打开工具侧边栏";
        }
    }
}
