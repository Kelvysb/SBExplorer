using SBExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SBExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ServiceBusConnection.xaml
    /// </summary>
    public partial class ServiceBusConnection : UserControl
    {
        private readonly ConnectionConfig connection;
        public ServiceBusConnection(ConnectionConfig connection)
        {
            this.connection = connection;
            InitializeComponent();
        }

        private List<ServiceBusQueue> serviceBusQueues;

        #region Events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadConnection();
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
                RefreshAll();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Methods

        private void LoadConnection()
        {
            TxtConnectionName.Text = connection.Description;
            StkQueues.Children.Clear();
            serviceBusQueues = new List<ServiceBusQueue>();
            if (connection.Queues == null || !connection.Queues.Any()) return;
            foreach (var queue in connection.Queues)
            {
                serviceBusQueues.Add(new ServiceBusQueue(connection, queue));
                StkQueues.Children.Add(serviceBusQueues.Last());
            }
        }

        private void RefreshAll()
        {
            foreach (var queue in serviceBusQueues)
            {
                queue.Refresh();
            }
        }
        #endregion

    }
}
