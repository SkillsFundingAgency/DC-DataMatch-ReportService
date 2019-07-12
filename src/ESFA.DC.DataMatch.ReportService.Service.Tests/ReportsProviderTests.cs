using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Service.Service;
using FluentAssertions;
using Moq;
using Xunit;
using ILogger = ESFA.DC.Logging.Interfaces.ILogger;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public class ReportsProviderTests
    {
        [Fact]
        public void ProvideReportsForContext_NoneRegistered()
        {
            var reportOne = "ReportOne";
            var reportTwo = "ReportTwo";
            var reportThree = "ReportThree";

            var reports = new List<IReport>();
            var loggerMock = new Mock<ILogger>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Tasks).Returns(new List<string>() { reportOne, reportTwo, reportThree });

            NewProvider(reports, loggerMock.Object).ProvideReportsForContext(reportServiceContext.Object).Should().BeEmpty();

            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportOne}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportTwo}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportThree}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Fact]
        public void ProvideReportsForContext_NoTasks()
        {
            var reportOneTaskName = "ReportOne";
            var reportTwoTaskName = "ReportTwo";
            var reportThreeTaskName = "ReportThree";

            var reportOneMock = new Mock<IReport>();
            reportOneMock.SetupGet(r => r.ReportTaskName).Returns(reportOneTaskName);
            var reportTwoMock = new Mock<IReport>();
            reportTwoMock.SetupGet(r => r.ReportTaskName).Returns(reportTwoTaskName);
            var reportThreeMock = new Mock<IReport>();
            reportThreeMock.SetupGet(r => r.ReportTaskName).Returns(reportThreeTaskName);

            var reports = new List<IReport>()
            {
                reportOneMock.Object,
                reportTwoMock.Object,
                reportThreeMock.Object,
            };

            var loggerMock = new Mock<ILogger>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Tasks).Returns(new List<string>());

            NewProvider(reports, loggerMock.Object).ProvideReportsForContext(reportServiceContext.Object).Should().BeEmpty();
        }

        [Fact]
        public void ProvideReportsForContext_Matching()
        {
            var reportOneTaskName = "ReportOne";
            var reportTwoTaskName = "ReportTwo";
            var reportThreeTaskName = "ReportThree";

            var reportOneMock = new Mock<IReport>();
            reportOneMock.SetupGet(r => r.ReportTaskName).Returns(reportOneTaskName);
            var reportTwoMock = new Mock<IReport>();
            reportTwoMock.SetupGet(r => r.ReportTaskName).Returns(reportTwoTaskName);
            var reportThreeMock = new Mock<IReport>();
            reportThreeMock.SetupGet(r => r.ReportTaskName).Returns(reportThreeTaskName);

            var reports = new List<IReport>()
            {
                reportOneMock.Object,
                reportTwoMock.Object,
                reportThreeMock.Object,
            };

            var loggerMock = new Mock<ILogger>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Tasks).Returns(new List<string>() { reportOneTaskName, reportTwoTaskName, reportThreeTaskName });

            var matchingReports = NewProvider(reports, loggerMock.Object).ProvideReportsForContext(reportServiceContext.Object);

            matchingReports.Should().Contain(reportOneMock.Object);
            matchingReports.Should().Contain(reportTwoMock.Object);
            matchingReports.Should().Contain(reportThreeMock.Object);
        }

        [Fact]
        public void ProvideReportsForContext_NoneMatching()
        {
            var reportOneTaskName = "ReportOne";
            var reportTwoTaskName = "ReportTwo";
            var reportThreeTaskName = "ReportThree";
            var reportFourTaskName = "ReportFour";
            var reportFiveTaskName = "ReportFive";
            var reportSixTaskName = "ReportSix";

            var reportOneMock = new Mock<IReport>();
            reportOneMock.SetupGet(r => r.ReportTaskName).Returns(reportOneTaskName);
            var reportTwoMock = new Mock<IReport>();
            reportTwoMock.SetupGet(r => r.ReportTaskName).Returns(reportTwoTaskName);
            var reportThreeMock = new Mock<IReport>();
            reportThreeMock.SetupGet(r => r.ReportTaskName).Returns(reportThreeTaskName);

            var reports = new List<IReport>()
            {
                reportOneMock.Object,
                reportTwoMock.Object,
                reportThreeMock.Object,
            };

            var loggerMock = new Mock<ILogger>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Tasks).Returns(new List<string>() { reportFourTaskName, reportFiveTaskName, reportSixTaskName });

            NewProvider(reports, loggerMock.Object).ProvideReportsForContext(reportServiceContext.Object).Should().BeEmpty();

            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportFourTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportFiveTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportSixTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Fact]
        public void ProvideReportsForContext_SomeMatching()
        {
            var reportOneTaskName = "ReportOne";
            var reportTwoTaskName = "ReportTwo";
            var reportThreeTaskName = "ReportThree";
            var reportFourTaskName = "ReportFour";
            var reportFiveTaskName = "ReportFive";
            var reportSixTaskName = "ReportSix";

            var reportOneMock = new Mock<IReport>();
            reportOneMock.SetupGet(r => r.ReportTaskName).Returns(reportOneTaskName);
            var reportTwoMock = new Mock<IReport>();
            reportTwoMock.SetupGet(r => r.ReportTaskName).Returns(reportTwoTaskName);
            var reportThreeMock = new Mock<IReport>();
            reportThreeMock.SetupGet(r => r.ReportTaskName).Returns(reportThreeTaskName);

            var reports = new List<IReport>()
            {
                reportOneMock.Object,
                reportTwoMock.Object,
                reportThreeMock.Object,
            };

            var loggerMock = new Mock<ILogger>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext
                .SetupGet(c => c.Tasks)
                .Returns(new List<string>()
                {
                    reportOneTaskName,
                    reportTwoTaskName,
                    reportThreeTaskName,
                    reportFourTaskName,
                    reportFiveTaskName,
                    reportSixTaskName
                });

            var matchingReports = NewProvider(reports, loggerMock.Object).ProvideReportsForContext(reportServiceContext.Object);

            matchingReports.Should().Contain(reportOneMock.Object);
            matchingReports.Should().Contain(reportTwoMock.Object);
            matchingReports.Should().Contain(reportThreeMock.Object);

            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportFourTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportFiveTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            loggerMock.Verify(l => l.LogWarning($"Missing Report Task - {reportSixTaskName}", null, -1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        private ReportsProvider NewProvider(IList<IReport> reports = null, ILogger logger = null)
        {
            return new ReportsProvider(reports, logger);
        }
    }
}
