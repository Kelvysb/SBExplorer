using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SBExplorer.Core.Models;
using SBExplorer.Core.Services;

namespace SBExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ServiceBusqueue.xaml
    /// </summary>
    public partial class ServiceBusQueue : UserControl
    {
        private readonly ServiceBusExplorerService serviceBusExplorerService;
        private readonly ConnectionConfig connection;
        private readonly QueueConfig queueConfig;

        public ServiceBusQueue(ConnectionConfig connection, QueueConfig queueConfig)
        {
            serviceBusExplorerService = SBExplorerPackage.Service;
            this.connection = connection;
            this.queueConfig = queueConfig;
            InitializeComponent();
        }

        #region Events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = GetQueueInfoAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenMessageWindow();
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
                _ = GetQueueInfoAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCreateQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = CreateQueueAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Events

        #region Methods

        public void Refresh()
        {
            _ = GetQueueInfoAsync();
        }

        private void OpenMessageWindow()
        {
            var message = new MessagesWindow(connection, queueConfig);
            message.Closed += (sender, e) => Refresh();
            message.Show();
        }

        private async Task GetQueueInfoAsync()
        {
            try
            {
                TxtConnectionName.Text = $"{queueConfig.QueueName} (loading)";
                BtnSendMessage.Visibility = Visibility.Collapsed;
                BtnRefresh.Visibility = Visibility.Collapsed;
                BtnCreateQueue.Visibility = Visibility.Collapsed;
                var queueInfo = await serviceBusExplorerService.GetQueueInfoAsync(connection.ConnectionString, queueConfig.QueueName);
                if (queueInfo != null)
                {
                    TxtConnectionName.Text = $"{queueConfig.QueueName} {queueInfo}";
                    BtnSendMessage.Visibility = Visibility.Visible;
                    BtnRefresh.Visibility = Visibility.Visible;
                    BtnCreateQueue.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtConnectionName.Text = $"{queueConfig.QueueName} (Not found)";
                    BtnSendMessage.Visibility = Visibility.Collapsed;
                    BtnRefresh.Visibility = Visibility.Collapsed;
                    BtnCreateQueue.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception)
            {
                TxtConnectionName.Text = $"{queueConfig.QueueName} (Error)";
            }
        }

        private async Task CreateQueueAsync()
        {
            var result = await serviceBusExplorerService.CreateQueueAsync(connection.ConnectionString, queueConfig.QueueName);
            if (result)
            {
                await GetQueueInfoAsync();
            }
        }

        #endregion Methods
    }
}