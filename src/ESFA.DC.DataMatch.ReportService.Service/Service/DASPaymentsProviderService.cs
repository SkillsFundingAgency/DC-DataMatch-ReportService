using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public sealed class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private readonly string _ZPROG001 = "ZPROG001";
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<DataMatchDataLockValidationErrorInfo> GetDataLockValidationErrorInfoForDataMatchReport(int collectionPeriod, int ukPrn, string[] learnRefNumbers, string collectionName, long? jobId, CancellationToken cancellationToken)
        {
            var dataMatchDataLockValidationErrorInfo = new DataMatchDataLockValidationErrorInfo()
            {
                DataLockValidationErrors = new List<DataLockValidationError>(),
            };

            bool IsILRSubmission = collectionName.CaseInsensitiveContains("ILR");
            var dataLockSourceId = IsILRSubmission ? 1 : 2;

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                var dataLockValidationErrors =
                    await (from dle in dasPaymentsContext.DataLockEvents
                           join ee in dasPaymentsContext.EarningEvents on dle.EarningEventId equals ee.EventId
                           join dlenpp in dasPaymentsContext.DataLockEventNonPayablePeriods on dle.EventId equals dlenpp.DataLockEventId
                           join dlenpf in dasPaymentsContext.DataLockEventNonPayablePeriodFailures on dlenpp.DataLockEventNonPayablePeriodId equals dlenpf.DataLockEventNonPayablePeriodId
                           where dle.Ukprn == ukPrn && dle.CollectionPeriod == collectionPeriod &&
                                 dlenpp.DeliveryPeriod == collectionPeriod &&
                                 dle.DataLockSourceId == dataLockSourceId &&
                                 dle.LearningAimReference.CaseInsensitiveEquals(_ZPROG001) &&
                                 dle.IsPayable == false
                           select new
                           {
                               dle.Ukprn,
                               dle.LearnerReferenceNumber,
                               dle.LearnerUln,
                               dlenpf.DataLockFailureId,
                               ee.LearningAimSequenceNumber,
                           }).Distinct().ToListAsync(cancellationToken);

                foreach (var dataLockValidationError in dataLockValidationErrors)
                {
                    dataMatchDataLockValidationErrorInfo.DataLockValidationErrors.Add(new DataLockValidationError()
                    {
                        UkPrn = dataLockValidationError.Ukprn,
                        LearnerReferenceNumber = dataLockValidationError.LearnerReferenceNumber,
                        AimSeqNumber = dataLockValidationError.LearningAimSequenceNumber,
                        LearnerUln = dataLockValidationError.LearnerUln,
                        RuleId = dataLockValidationError.DataLockFailureId,
                    });
                }
            }

            return dataMatchDataLockValidationErrorInfo;
        }

        public async Task<DataMatchDasApprenticeshipInfo> GetDasApprenticeshipInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchDasApprenticeshipInfo = new DataMatchDasApprenticeshipInfo()
            {
                UkPrn = ukPrn,
                DasApprenticeshipInfos = new List<DasApprenticeshipInfo>(),
            };

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                var dataMatchDasApprenticeshipPrices =
                     await (from a in dasPaymentsContext.Apprenticeships
                            join ape in dasPaymentsContext.ApprenticeshipPriceEpisodes on a.Id equals ape.ApprenticeshipId
                                into appPriceEpisodesJoin
                            from apej in appPriceEpisodesJoin.DefaultIfEmpty()
                            join ap in dasPaymentsContext.ApprenticeshipPauses on apej.Id equals ap.ApprenticeshipId
                                into appPriceEpisodePausesJoin
                            from apepj in appPriceEpisodePausesJoin.DefaultIfEmpty()
                            where a.Ukprn == ukPrn
                            select new DasApprenticeshipInfo()
                            {
                                UkPrn = ukPrn,
                                LearnerUln = a.Uln,
                                WithdrawnOnDate = a.StopDate,
                                PausedOnDate = apepj.PauseDate,
                                Cost = apej.Cost,
                                LegalEntityName = a.LegalEntityName,
                                ProgrammeType = a.ProgrammeType,
                                StandardCode = a.StandardCode,
                                FrameworkCode = a.FrameworkCode,
                                PathwayCode = a.PathwayCode,
                            }).Distinct().ToListAsync(cancellationToken);

                dataMatchDasApprenticeshipInfo.DasApprenticeshipInfos = dataMatchDasApprenticeshipPrices;
            }

            return dataMatchDasApprenticeshipInfo;
        }
    }
}