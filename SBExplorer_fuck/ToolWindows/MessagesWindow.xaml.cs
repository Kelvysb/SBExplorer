﻿using Microsoft.VisualStudio.PlatformUI;
using SBExplorer.Models;
using SBExplorer.Services;
using System.Threading.Tasks;
using System.Windows;

namespace SBExplorer
{
    public partial class MessagesWindow : DialogWindow
    {
        private readonly ServiceBusExplorerService serviceBusExplorerService;
        private readonly ConnectionConfig connection;
        private readonly QueueConfig queueConfig;

        public MessagesWindow(ConnectionConfig connection, QueueConfig queueConfig)
        {
            serviceBusExplorerService = ServiceBusExplorerService.GetInstance();
            this.connection = connection;
            this.queueConfig = queueConfig;
            InitializeComponent();
        }

        #region Events

        private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtSend.Text = queueConfig.LastMessage;
                _ = GetQueueInfoAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = SendMessageAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDefaultMessage();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDefaultMessage();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = ReceiveMessageAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearQueue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = ClearQueueAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearDeadLetter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = ClearDeadLetterAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReceiveDeadLetter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = ReceiveDeadLetterAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCopytoSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtSend.Text = TxtReceive.Text;
                TxtReceive.Text = "";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Methods
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(TxtSend.Text))
            {
                MessageBox.Show("Message cannot be empty.", "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            GrdMain.IsEnabled = false;
            if (await serviceBusExplorerService.SendMessageAsync(connection.ConnectionString, queueConfig.QueueName, TxtSend.Text))
            {
                MessageBox.Show($"Message sent to {queueConfig.QueueName}.", "ServiceBus Explorer");
                await GetQueueInfoAsync();
                queueConfig.LastMessage = TxtSend.Text;
                serviceBusExplorerService.SaveConfig();
            }
            GrdMain.IsEnabled = true;
        }

        private async Task ReceiveMessageAsync()
        {
            GrdMain.IsEnabled = false;
            TxtReceive.Text =
                await serviceBusExplorerService.ReceiveMessageAsync(connection.ConnectionString, queueConfig.QueueName, ChkReceiveandDelete.IsChecked.GetValueOrDefault());
            await GetQueueInfoAsync();
            GrdMain.IsEnabled = true;
        }

        private async Task ReceiveDeadLetterAsync()
        {
            GrdMain.IsEnabled = false;
            TxtReceive.Text = await serviceBusExplorerService.ReceiveDeadLetterMessageAsync(connection.ConnectionString, queueConfig.QueueName, ChkReceiveandDelete.IsChecked.GetValueOrDefault());
            await GetQueueInfoAsync();
            GrdMain.IsEnabled = true;
        }

        private async Task ClearQueueAsync()
        {
            if (await serviceBusExplorerService.ClearQueueAsync(connection.ConnectionString, queueConfig.QueueName))
            {
                MessageBox.Show($"Queue {queueConfig.QueueName} cleared.", "ServiceBus Explorer");
                await GetQueueInfoAsync();
            }
        }

        private void LoadDefaultMessage()
        {
            TxtSend.Text = queueConfig.DefaultMessage;
        }

        private void SaveDefaultMessage()
        {
            queueConfig.DefaultMessage = TxtSend.Text;
            serviceBusExplorerService.SaveConfig();
        }

        private async Task GetQueueInfoAsync()
        {
            Title = $"{queueConfig.QueueName} (loading)";
            var queueInfo = await serviceBusExplorerService.GetQueueInfoAsync(connection.ConnectionString, queueConfig.QueueName);
            if (queueInfo != null)
            {
                Title = $"{queueConfig.QueueName} {queueInfo}";
            }
            else
            {
                Title = $"{queueConfig.QueueName} (Not found)";
            }
        }

        private async Task ClearDeadLetterAsync()
        {
            GrdMain.IsEnabled = false;
            if (await serviceBusExplorerService.ClearDeadLetterAsync(connection.ConnectionString, queueConfig.QueueName))
            {
                MessageBox.Show($"Dead letter of {queueConfig.QueueName} queue cleared.", "ServiceBus Explorer");
                await GetQueueInfoAsync();
            }
            GrdMain.IsEnabled = true;
        }
        #endregion


    }
}
