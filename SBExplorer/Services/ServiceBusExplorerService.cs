using Humanizer;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SBExplorer.Extensions;
using SBExplorer.Helpers;
using SBExplorer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SBExplorer.Services
{
    public class ServiceBusExplorerService
    {
        private static ServiceBusExplorerService instance;

        public IVsSolution Solution { get; private set; }
        public string WorkDirectory { get; private set; }
        public string SolutionFolder { get; private set; }
        public string SolutionFile { get; private set; }
        public string UserOptsFile { get; private set; }
        public string ConfigFolder { get; private set; }
        public string ConfigPath { get; private set; }
        public SBExplorerConfig Config { get; private set; }

        private ServiceBusExplorerService(IVsSolution solutionInfo)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Solution = solutionInfo;
            _ = solutionInfo.GetSolutionInfo(
                out var solutionFolder,
                out var solutionFile,
                out var userOptsFile);
            WorkDirectory = solutionFolder;
            SolutionFolder = PathHelper.MakeRelativePath(WorkDirectory, solutionFolder);
            SolutionFile = Path.Combine(SolutionFolder, Path.GetFileName(solutionFile));
            UserOptsFile = PathHelper.MakeRelativePath(WorkDirectory, userOptsFile);
            LoadConfig();
            UpdateConnections();
        }

        public static void Initialize(IVsSolution solutionInfo)
        {
            instance = new ServiceBusExplorerService(solutionInfo);
        }

        public static ServiceBusExplorerService GetInstance()
        {
            return instance;
        }

        public async Task<QueueInfo> GetQueueInfoAsync(string connectionString, string queueName)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                var runtimeInfo = await queueManagement.GetQueueRuntimeInfoAsync(queueName);
                return new QueueInfo
                {
                    ActiveMessagesCount = runtimeInfo.MessageCountDetails.ActiveMessageCount,
                    DeadLetterCount = runtimeInfo.MessageCountDetails.DeadLetterMessageCount,
                    ScheduledMessagesCount = runtimeInfo.MessageCountDetails.ScheduledMessageCount
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreateQueueAsync(string connectionString, string queueName)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (!await queueManagement.QueueExistsAsync(queueName))
                {
                    await queueManagement.CreateQueueAsync(queueName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteQueueAsync(string connectionString, string queueName)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName))
                {
                    await queueManagement.DeleteQueueAsync(queueName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ClearQueueAsync(string connectionString, string queueName)

        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName))
                {
                    await queueManagement.DeleteQueueAsync(queueName);
                    await queueManagement.CreateQueueAsync(queueName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendMessageAsync(string connectionString, string queueName, string message)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName))
                {
                    var sender = new MessageSender(connectionString, queueName);
                    var messageObject = new Message(System.Text.Encoding.UTF8.GetBytes(message));
                    await sender.SendAsync(messageObject);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> ReceiveDeadLetterMessageAsync(string connectionString, string queueName, bool receiveAndDelete)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName))
                {
                    var receiver = new MessageReceiver(connectionString, $"{queueName}/$deadletterqueue", receiveAndDelete ? ReceiveMode.ReceiveAndDelete : ReceiveMode.PeekLock);
                    Message message = null;
                    if (receiveAndDelete)
                    {
                        message = await receiver.ReceiveAsync();
                    }
                    else
                    {
                        message = await receiver.PeekAsync();
                    }
                    if (message != null)
                    {
                        return System.Text.Encoding.UTF8.GetString(message.Body);
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> ReceiveMessageAsync(string connectionString, string queueName, bool receiveAndDelete)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName) && await GetMessageCountAsync(queueManagement, queueName) > 0)
                {
                    var receiver = new MessageReceiver(connectionString, queueName, receiveAndDelete ? ReceiveMode.ReceiveAndDelete : ReceiveMode.PeekLock);
                    Message message = null;
                    if (receiveAndDelete)
                    {
                        message = await receiver.ReceiveAsync();
                    }
                    else
                    {
                        message = await receiver.PeekAsync();
                    }
                    if (message != null)
                    {
                        return System.Text.Encoding.UTF8.GetString(message.Body);
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> ClearDeadLetterAsync(string connectionString, string queueName)
        {
            try
            {
                var queueManagement = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
                if (await queueManagement.QueueExistsAsync(queueName))
                {
                    var receiver = new MessageReceiver(connectionString, $"{queueName}/$deadletterqueue", ReceiveMode.ReceiveAndDelete);
                    while (await receiver.ReceiveAsync(1000, TimeSpan.FromSeconds(2)) != null)
                    {
                        _ = await receiver.ReceiveAsync(1000, TimeSpan.FromSeconds(2));
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> IsolateAsync(ConnectionConfig connection)
        {
            try
            {
                var isolationName = Environment.UserName.Humanize().Underscore();
                var configFileString = File.ReadAllText(Path.Combine(WorkDirectory, Config.ConfigFile.FilePath));
                JObject configFile = JObject.Parse(configFileString);

                foreach (var queue in connection.Queues)
                {
                    configFile
                        .SelectToken(queue.QueuePath)
                        .Replace($"{isolationName}_{queue.QueueName}");
                    await CreateQueueAsync(connection.ConnectionString, $"{isolationName}_{queue.QueueName}");
                }

                File.WriteAllText(Path.Combine(WorkDirectory, Config.ConfigFile.FilePath), configFile.ToString(Formatting.Indented));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeIsolateAsync(ConnectionConfig connection)
        {
            try
            {
                var isolationName = Environment.UserName.Humanize().Underscore();
                var configFileString = File.ReadAllText(Path.Combine(WorkDirectory, Config.ConfigFile.FilePath));
                JObject configFile = JObject.Parse(configFileString);

                foreach (var queue in connection.Queues)
                {
                    configFile
                        .SelectToken(queue.QueuePath)
                        .Replace(queue.QueueName.Replace($"{isolationName}_", ""));
                    await DeleteQueueAsync(connection.ConnectionString, $"{isolationName}_{queue.QueueName}");
                }

                File.WriteAllText(Path.Combine(WorkDirectory, Config.ConfigFile.FilePath), configFile.ToString(Formatting.Indented));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void LoadConfig()
        {
            ConfigFolder = Path.Combine(SolutionFolder, ".sbexplorer");
            if (!Directory.Exists(Path.Combine(WorkDirectory, ConfigFolder)))
            {
                Directory.CreateDirectory(Path.Combine(WorkDirectory, ConfigFolder));
            }
            ConfigPath = Path.Combine(ConfigFolder, "sbexplorer.json");
            if (File.Exists(Path.Combine(WorkDirectory, ConfigPath)))
            {
                Config = JsonConvert.DeserializeObject<SBExplorerConfig>(File.ReadAllText(Path.Combine(WorkDirectory, ConfigPath)));
            }
            else
            {
                Config = GetDefaultConfig();
                SaveConfig();
            }
        }

        public void UpdateConnections()
        {
            if (string.IsNullOrEmpty(Config.ConfigFile.FilePath))
            {
                if (Config.ConfigFile.Source == ConfigFileSource.LaunchSettings)
                {
                    Config.ConfigFile.FilePath = FindFileRecursive("launchSettings.json");
                }
                else
                {
                    Config.ConfigFile.FilePath = FindFileRecursive("appsettings.json");
                }
            }
            var configFileString = File.ReadAllText(Path.Combine(WorkDirectory, Config.ConfigFile.FilePath));
            JObject configFile = JObject.Parse(configFileString);

            foreach (var connectionConfig in Config.ConfigFile.Connections)
            {
                var variables = configFile.SelectTokens(connectionConfig.BaseJsonPath);
                ConnectionConfig connection =
                    !connectionConfig.ConnectionstringFullPathMode
                    ? GetConnection(connectionConfig, variables)
                    : GetConnectionByPath(connectionConfig, configFile);

                if (connection == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(connectionConfig.Description))
                {
                    connectionConfig.Description = connection.Description;
                }

                connectionConfig.ConnectionString = connection.ConnectionString;

                List<QueueConfig> queues = GetQueues(connectionConfig, variables);

                MergeQueues(connectionConfig, queues);
            }
            SaveConfig();
        }

        public void SaveConfig()
        {
            File.WriteAllText(Path.Combine(WorkDirectory, ConfigPath), JsonConvert.SerializeObject(Config));
        }

        public string FindFileRecursive(string file)
        {
            return FindFileRecursive(SolutionFolder, file);
        }

        public string FindFileRecursive(string path, string file)
        {
            var rootedPath = Path.IsPathRooted(path) ? path : Path.Combine(WorkDirectory, path);
            var files = Directory.GetFiles(rootedPath);
            var directories = Directory.GetDirectories(rootedPath);

            if (files.Any(f => Path.GetFileName(f).StartsWith(file)))
            {
                return Path.Combine(path, file);
            }

            foreach (var directory in directories)
            {
                var result = FindFileRecursive(directory, file);
                if (!string.IsNullOrEmpty(result))
                {
                    return Path.IsPathRooted(result) ? PathHelper.MakeRelativePath(WorkDirectory, result) : result;
                }
            }
            return string.Empty;
        }

        private static void MergeQueues(ConnectionConfig connectionsConfig, List<QueueConfig> queues)
        {
            if (connectionsConfig.Queues == null)
            {
                connectionsConfig.Queues = new List<QueueConfig>();
            }
            foreach (var queue in queues)
            {
                var existing = connectionsConfig.Queues.FirstOrDefault(q => q.Key.Equals(queue.Key, StringComparison.InvariantCultureIgnoreCase));
                if (existing != null)
                {
                    existing.QueueName = queue.QueueName;
                    existing.QueuePath = queue.QueuePath;
                }
                else
                {
                    connectionsConfig.Queues.Add(queue);
                }
            }
            connectionsConfig.Queues.RemoveAll(q => !queues.Any(a => a.Key == q.Key));
        }

        private static List<QueueConfig> GetQueues(ConnectionConfig connectionConfig, IEnumerable<JToken> variables)
        {
            var result = GetDirectKeyQueues(connectionConfig, variables);
            result.AddRange(GetSubPathQueues(connectionConfig, variables));
            return result;
        }

        private static List<QueueConfig> GetSubPathQueues(ConnectionConfig connectionConfig, IEnumerable<JToken> variables)
        {
            var result = new List<QueueConfig>();

            var subMatchPaths = variables
                            .Where(t => CheckKey(connectionConfig, t, true))
                            .ToList();

            foreach (var item in subMatchPaths)
            {
                result.AddRange(item
                    .SelectMany(t => t.Children())
                    .Select(q =>
                    {
                        return new QueueConfig
                        {
                            Key = q.GetKey(),
                            QueueName = q.ToString(),
                            Description = q.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase),
                            QueuePath = q.Path,
                            NotifyChanges = false,
                            ReceiveAndDelete = true
                        };
                    }));
            }

            return result;
        }

        private static List<QueueConfig> GetDirectKeyQueues(ConnectionConfig connectionConfig, IEnumerable<JToken> variables)
        {
            return variables
                    .Where(t => CheckKey(connectionConfig, t, false))
                    .Select(q =>
                    {
                        return new QueueConfig
                        {
                            Key = q.GetKey(),
                            QueueName = q.ToString(),
                            Description = q.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase),
                            QueuePath = q.Path,
                            NotifyChanges = false,
                            ReceiveAndDelete = true
                        };
                    })
                    .ToList();
        }

        private static bool CheckKey(ConnectionConfig connectionConfig, JToken token, bool isSubPath)
        {
            var result = false;
            var key = token.GetKey();
            result = Regex.IsMatch(key, connectionConfig.QueueNamesPattern);
            result = result && ((token.Children().Any() && isSubPath) || (!token.Children().Any() && !isSubPath));
            if (result && !string.IsNullOrEmpty(connectionConfig.QueueNamesContains))
            {
                var containValues = connectionConfig.QueueNamesContains.Split(',');
                result = containValues.All(c => key.Contains(c));
            }
            if (result && !string.IsNullOrEmpty(connectionConfig.QueueNamesNotContains))
            {
                var containValues = connectionConfig.QueueNamesNotContains.Split(',');
                result = containValues.All(c => !key.Contains(c));
            }
            return result;
        }

        private static ConnectionConfig GetConnection(ConnectionConfig connectionsConfig, IEnumerable<JToken> variables)
        {
            var result = variables
                .Where(t => t.GetKey().Equals(connectionsConfig.Key, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => new ConnectionConfig
                {
                    ConnectionString = c.ToString(),
                    Description = c.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase)
                })
                .FirstOrDefault();

            if (result == null)
            {
                result = variables
                .Where(t => t.GetKey().Equals(connectionsConfig.Endpoint, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => new ConnectionConfig
                {
                    ConnectionString = GetConnectionString(connectionsConfig, variables),
                    Description = c.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase)
                })
                .FirstOrDefault();
            }

            return result;
        }

        private static string GetConnectionString(ConnectionConfig connectionsConfig, IEnumerable<JToken> variables)
        {
            var endpoint = variables
                 .Where(t => t.GetKey().Equals(connectionsConfig.Endpoint, StringComparison.InvariantCultureIgnoreCase))
                 .Select(c => c.ToString())
                 .FirstOrDefault();

            var sharedKeyName = variables
                .Where(t => t.GetKey().Equals(connectionsConfig.SharedKeyName, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => c.ToString())
                .FirstOrDefault();

            var sharedKey = variables
                .Where(t => t.GetKey().Equals(connectionsConfig.SharedKey, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => c.ToString())
                .FirstOrDefault();

            return string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(sharedKeyName) || string.IsNullOrEmpty(sharedKey)
                ? ""
                : $"Endpoint={endpoint};SharedAccessKeyName={sharedKeyName};SharedAccessKey={sharedKey}";
        }

        private static string GetConnectionStringByPath(ConnectionConfig connectionsConfig, JObject root)
        {
            var endpoint = root.SelectTokens(connectionsConfig.Endpoint)
                 .Select(c => c.ToString())
                 .FirstOrDefault();

            var sharedKeyName = root.SelectTokens(connectionsConfig.SharedKeyName)
                .Select(c => c.ToString())
                .FirstOrDefault();

            var sharedKey = root.SelectTokens(connectionsConfig.SharedKey)
                .Select(c => c.ToString())
                .FirstOrDefault();

            return string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(sharedKeyName) || string.IsNullOrEmpty(sharedKey)
                ? ""
                : $"Endpoint={endpoint};SharedAccessKeyName={sharedKeyName};SharedAccessKey={sharedKey}";
        }

        private ConnectionConfig GetConnectionByPath(ConnectionConfig connectionsConfig, JObject root)
        {
            var result = root.SelectTokens(connectionsConfig.Key)
                .Select(c => new ConnectionConfig
                {
                    ConnectionString = c.ToString(),
                    Description = c.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase)
                })
                .FirstOrDefault();

            if (result == null)
            {
                result = root.SelectTokens(connectionsConfig.Endpoint)
                .Select(c => new ConnectionConfig
                {
                    ConnectionString = GetConnectionStringByPath(connectionsConfig, root),
                    Description = c.GetKey().Humanize().Transform(To.LowerCase, To.TitleCase)
                })
                .FirstOrDefault();
            }

            return result;
        }

        private SBExplorerConfig GetDefaultConfig()
        {
            return new SBExplorerConfig
            {
                ConfigFile = new ConfigFile
                {
                    Source = ConfigFileSource.LaunchSettings,
                    FilePath = FindFileRecursive("launchSettings.json"),
                    Connections = new List<ConnectionConfig>
                    {
                        new ConnectionConfig
                        {
                            BaseJsonPath = "$..environmentVariables.*",
                            Key = "SERVICEBUS-CONNECTION-STRING",
                            Endpoint = "SERVICE_BUS_ENDPOINT",
                            SharedKeyName = "SERVICE_BUS_SHARED_ACCESS_KEY_NAME",
                            SharedKey = "SERVICE_BUS_SHARED_ACCESS_KEY",
                            QueueNamesPattern = "((.*)-QUEUE-NAME|SERVICE_BUS_QUEUE_(.*))",
                            AutoConnect = true,
                            AutoCreateMissingQueues = false,
                            Description = "",
                            ConnectionString = "",
                            Queues = new List<QueueConfig>()
                        }
                    }
                },
                DefaultAutoConnect = true,
                DefaultAutoCreateMissingQueues = false
            };
        }

        private async Task<long> GetMessageCountAsync(ManagementClient queueManagement, string queueName)
        {
            var runtimeInfo = await queueManagement.GetQueueRuntimeInfoAsync(queueName);
            return runtimeInfo.MessageCountDetails.ActiveMessageCount;
        }
    }
}