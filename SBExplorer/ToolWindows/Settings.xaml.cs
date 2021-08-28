using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using SBExplorer.Controls;
using SBExplorer.Core.Models;
using SBExplorer.Core.Services;

namespace SBExplorer.ToolWindows
{
    public partial class Settings : DialogWindow
    {
        private readonly ServiceBusExplorerService serviceBusExplorerService;
        private List<ConnectionSettings> connectionSettings;

        public Settings()
        {
            serviceBusExplorerService = SBExplorerPackage.Service;
            InitializeComponent();
        }

        #region Events

        private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Load();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void radLaunchSettings_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeSource(0);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void radAppSettings_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeSource(1);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddConfiguration();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DialogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Save();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveConnectionHandler(object sender, ConnectionConfig connectionConfig)
        {
            try
            {
                RemoveConnection(connectionConfig);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "ServiceBus Explorer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Events

        #region Methods

        private void Load()
        {
            connectionSettings = new List<ConnectionSettings>();
            StkConfigurations.Children.Clear();
            foreach (var connection in serviceBusExplorerService.Config.ConfigFile.Connections)
            {
                connectionSettings.Add(new ConnectionSettings(connection));
                connectionSettings.Last().RemoveConnection += RemoveConnectionHandler;
                StkConfigurations.Children.Add(connectionSettings.Last());
            }
            if (serviceBusExplorerService.Config.ConfigFile.Source == ConfigFileSource.LaunchSettings)
            {
                radLaunchSettings.IsChecked = true;
            }
            else
            {
                radAppSettings.IsChecked = true;
            }
        }

        private void AddConfiguration()
        {
            serviceBusExplorerService.Config.ConfigFile.Connections.Add(new ConnectionConfig());
            connectionSettings.Add(new ConnectionSettings(serviceBusExplorerService.Config.ConfigFile.Connections.Last()));
            connectionSettings.Last().RemoveConnection += RemoveConnectionHandler;
            StkConfigurations.Children.Add(connectionSettings.Last());
        }

        private void ChangeSource(int index)
        {
            if (index == 0)
            {
                serviceBusExplorerService.Config.ConfigFile.Source = ConfigFileSource.LaunchSettings;
            }
            else
            {
                serviceBusExplorerService.Config.ConfigFile.Source = ConfigFileSource.AppSettings;
            }
        }

        private void Save()
        {
            serviceBusExplorerService.SaveConfig();
        }

        private void RemoveConnection(ConnectionConfig connectionConfig)
        {
            serviceBusExplorerService.Config.ConfigFile.Connections.Remove(connectionConfig);
            Load();
        }

        #endregion Methods
    }
}