using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Autofac;
using Autofac.Integration.ServiceFabric;
using ESFA.DC.DataMatch.ReportService.Core;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
using ESFA.DC.ServiceFabric.Common.Config;
using ESFA.DC.ServiceFabric.Common.Config.Interface;

namespace ESFA.DC.DataMatch.ReportService.Stateless
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                IServiceFabricConfigurationService serviceFabricConfigurationService = new ServiceFabricConfigurationService();

                // License Aspose.Cells
                SoftwareLicenceSection softwareLicenceSection = serviceFabricConfigurationService.GetConfigSectionAs<SoftwareLicenceSection>(nameof(SoftwareLicenceSection));
                if (!string.IsNullOrEmpty(softwareLicenceSection.AsposeLicence))
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(softwareLicenceSection.AsposeLicence.Replace("&lt;", "<").Replace("&gt;", ">"))))
                    {
                        new Aspose.Cells.License().SetLicense(ms);
                    }
                }

                // Setup Autofac
                ContainerBuilder builder = DIComposition.BuildNewContainer();
                ConfigurationRootModel configurationRoot = DICompositionServiceFabric.BuildContainer(builder, serviceFabricConfigurationService);
                DIComposition.BuildContainer(builder, configurationRoot);
                DIComposition.BuildStorageContainerAzure(builder, configurationRoot.azureBlobStorageOptions);

                // Register the Autofac magic for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register the stateless service.
                builder.RegisterStatelessService<ServiceFabric.Common.Stateless>("ESFA.DC.DataMatch.ReportService.StatelessType");

                using (var container = builder.Build())
                {
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(ServiceFabric.Common.Stateless).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e + Environment.NewLine + (e.InnerException?.ToString() ?? "No inner exception"));
                throw;
            }
        }
    }
}
