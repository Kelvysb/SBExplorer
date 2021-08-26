using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SBExplorer
{
    [Command(PackageIds.SBExplorer)]
    internal sealed class ServiceBusExplorerCommand : BaseCommand<ServiceBusExplorerCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return ServiceBusExplorer.ShowAsync();
        }
    }
}
