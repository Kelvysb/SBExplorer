using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SBExplorer.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace SBExplorer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(ServiceBusExplorer.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.SBExplorerString)]
    public sealed class SBExplorerPackage : ToolkitPackage
    {
        public static ServiceBusExplorerService Service { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var solutionService = (SVsSolution)await GetServiceAsync(typeof(SVsSolution));
            var solutionInterface = solutionService as IVsSolution;
            _ = solutionInterface.GetSolutionInfo(
                out var solutionFolder,
                out var solutionFile,
                out var userOptsFile);
            Service = new ServiceBusExplorerService(solutionFolder, solutionFile);
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
        }
    }
}