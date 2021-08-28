using SBExplorer.Controls;
using SBExplorer.Services;
using SBExplorer.ToolWindows;
using System.Windows;
using System.Windows.Controls;

namespace SBExplorer
{
    public partial class ServiceBusExplorerControl : UserControl
    {
        private readonly ServiceBusExplorerService serviceBusExplorerService;

        public ServiceBusExplorerControl()
        {
            serviceBusExplorerService = ServiceBusExplorerService.GetInstance();
            InitializeComponent();
            LoadConnections();
        }


        #region Events
        private void ServiceBusExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadConnections();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenSettings();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReloadAll();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Methods
        private void ReloadAll()
        {
            serviceBusExplorerService.LoadConfig();
            serviceBusExplorerService.UpdateConnections();
            LoadConnections();
        }

        private void LoadConnections()
        {
            StkConnections.Children.Clear();
            foreach (var connection in serviceBusExplorerService.Config.ConfigFile.Connections)
            {
                var connectionComponent = new ServiceBusConnection(connection);
                StkConnections.Children.Add(connectionComponent);
            }
        }

        private void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.ShowDialog();
            ReloadAll();
        }
        #endregion
    }
}
