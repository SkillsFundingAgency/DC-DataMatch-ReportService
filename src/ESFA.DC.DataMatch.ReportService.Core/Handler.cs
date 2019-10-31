using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.DataMatch.ReportService.Core.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
using ESFA.DC.DataMatch.ReportService.Service;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ExecutionContext = ESFA.DC.Logging.ExecutionContext;

namespace ESFA.DC.DataMatch.ReportService.Core
{
    public sealed class Handler : IHandler
    {
        private readonly ILifetimeScope _parentLifeTimeScope;

        public Handler(ILifetimeScope parentLifeTimeScope)
        {
            _parentLifeTimeScope = parentLifeTimeScope;
        }

        public async Task<bool> HandleAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            using (var childLifeTimeScope = GetChildLifeTimeScope(reportServiceContext))
            {
                var executionContext = (ExecutionContext)childLifeTimeScope.Resolve<IExecutionContext>();
                executionContext.JobId = reportServiceContext.JobId.ToString();
                var logger = childLifeTimeScope.Resolve<ILogger>();
                logger.LogDebug("Started Data Match Report Service", jobIdOverride: reportServiceContext.JobId);
                var entryPoint = childLifeTimeScope.Resolve<EntryPoint>();
                var result = await entryPoint.Callback(reportServiceContext, cancellationToken);
                logger.LogDebug($"Completed Data Match Report Service with result-{result}", jobIdOverride: reportServiceContext.JobId);
                return result;
            }
        }

        public ILifetimeScope GetChildLifeTimeScope(IReportServiceContext reportServiceContext)
        {
            return _parentLifeTimeScope.BeginLifetimeScope(c =>
            {
                var azureBlobStorageOptions = _parentLifeTimeScope.Resolve<IAzureStorageOptions>();
                c.RegisterInstance(new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        reportServiceContext.Container))
                    .As<IAzureStorageKeyValuePersistenceServiceConfig>();
                DIComposition.RegisterServicesByYear(reportServiceContext.CollectionYear, c);
            });
        }
    }
}
