using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace SBExplorer
{
    public class ServiceBusExplorer : BaseToolWindow<ServiceBusExplorer>
    {
        public override string GetTitle(int toolWindowId) => "ServiceBus Explorer";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new ServiceBusExplorerControl());
        }

        [Guid("f3063878-5e22-43c5-9026-d41091b3f185")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.StatusInformation;
            }
        }
    }
}
