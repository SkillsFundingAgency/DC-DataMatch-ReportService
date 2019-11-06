using Autofac;
using ESFA.DC.DataMatch.ReportService.Core;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Stateless;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests
{
    public sealed class AutoFacTest
    {
        [Fact]
        public void TestRegistrations()
        {
            ContainerBuilder builder = DIComposition.BuildNewContainer();
            ConfigurationRootModel configurationRoot = DICompositionServiceFabric.BuildContainer(builder, new TestConfigurationHelper());
            DIComposition.BuildContainer(builder, configurationRoot);
            DIComposition.RegisterServicesByYear(Constants.YEAR_1920, builder);
            DIComposition.BuildStorageContainerAzure(builder, configurationRoot.azureBlobStorageOptions);

            var c = builder.Build();

            using (var lifeTime = c.BeginLifetimeScope())
            {
                var messageHandler = lifeTime.Resolve<IMessageHandler<JobContextMessage>>();
                var entryPoint = lifeTime.Resolve<EntryPoint>();

                entryPoint.Should().NotBeNull();

                messageHandler.Should().NotBeNull();
            }
        }
    }
}
