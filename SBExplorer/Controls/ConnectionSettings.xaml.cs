using SBExplorer.Models;
using SBExplorer.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SBExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ConnectionSettings.xaml
    /// </summary>
    public partial class ConnectionSettings : UserControl
    {
        private ConnectionConfig connectionConfig;
        private readonly ServiceBusExplorerService serviceBusExplorerService;

        public ConnectionSettings(ConnectionConfig connectionConfig)
        {
            serviceBusExplorerService = ServiceBusExplorerService.GetInstance();
            this.connectionConfig = connectionConfig;
            InitializeComponent();
        }

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveConnection.Invoke(this, connectionConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {

            try
            {
                connectionConfig.Description = TxtName.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtJsonBasePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.BaseJsonPath = TxtJsonBasePath.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtConnectionString_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.Key = TxtConnectionString.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtQueuesPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.QueueNamesPattern = TxtQueuesPattern.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtEndpoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.Endpoint = TxtEndpoint.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSharedKeyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.SharedKeyName = TxtSharedKeyName.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSharedkey_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                connectionConfig.SharedKey = TxtSharedkey.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIsolate_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _ = IsolateAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _ = RestoreAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportQueues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportQueueList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public delegate void RemoveConnectionHandler(object sender, ConnectionConfig connectionConfig);
        public event RemoveConnectionHandler RemoveConnection;

        #endregion

        #region Methods
        private void Load()
        {
            TxtJsonBasePath.Text = connectionConfig.BaseJsonPath;
            TxtName.Text = connectionConfig.Description;
            TxtConnectionString.Text = connectionConfig.Key;
            TxtQueuesPattern.Text = connectionConfig.QueueNamesPattern;
            TxtEndpoint.Text = connectionConfig.Endpoint;
            TxtSharedKeyName.Text = connectionConfig.SharedKeyName;
            TxtSharedkey.Text = connectionConfig.SharedKey;
            if (connectionConfig.Isolated)
            {
                BtnIsolate.Visibility = Visibility.Collapsed;
                BtnRestore.Visibility = Visibility.Visible;
            }
            else
            {
                BtnIsolate.Visibility = Visibility.Visible;
                BtnRestore.Visibility = Visibility.Collapsed;
            }
        }

        private async Task IsolateAsync()
        {
            GrdMain.IsEnabled = false;
            if (await serviceBusExplorerService.IsolateAsync(connectionConfig))
            {
                BtnIsolate.Visibility = Visibility.Collapsed;
                BtnRestore.Visibility = Visibility.Visible;
                connectionConfig.Isolated = true;
                serviceBusExplorerService.SaveConfig();
                MessageBox.Show("Queues isolated", "ServiceBus Explorer");
            }
            GrdMain.IsEnabled = true;
        }

        private async Task RestoreAsync()
        {
            GrdMain.IsEnabled = false;
            if (await serviceBusExplorerService.DeIsolateAsync(connectionConfig))
            {
                BtnIsolate.Visibility = Visibility.Visible;
                BtnRestore.Visibility = Visibility.Collapsed;
                connectionConfig.Isolated = false;
                serviceBusExplorerService.SaveConfig();
                MessageBox.Show("Queues restored, and isolated queues deleted", "ServiceBus Explorer");

            }
            GrdMain.IsEnabled = true;
        }

        private void ExportQueueList()
        {
            var queueList = string.Join("\n", connectionConfig.Queues.Select(q => q.QueueName));
            Clipboard.SetText(queueList);
            MessageBox.Show("Queues list sent to clipboard.", "ServiceBus Explorer");
        }
        #endregion        
    }
}
