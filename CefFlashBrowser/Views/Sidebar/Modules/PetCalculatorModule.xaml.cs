using CefFlashBrowser.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CefFlashBrowser.Views.Sidebar.Modules
{
    public partial class PetCalculatorModule : UserControl
    {
        public PetCalculatorModule()
        {
            InitializeComponent();
        }

        private void OpenCalculatorClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ClientPaths.PetCalculatorPath))
            {
                MessageBox.Show(
                    $"未找到精灵计算器：\n{ClientPaths.PetCalculatorPath}",
                    "Avalon 赛尔号客户端",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(ClientPaths.PetCalculatorPath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(ClientPaths.PetCalculatorPath)
                });
            }
            catch (Exception exception)
            {
                ClientLogger.Error("Failed to start Seer Pet Calculator", exception);
                MessageBox.Show(
                    $"精灵计算器启动失败：\n{exception.Message}",
                    "Avalon 赛尔号客户端",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
