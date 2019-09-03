using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public sealed class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<DataMatchDataLockValidationErrorInfo> GetDataLockValidationErrorInfoForDataMatchReport(int collectionPeriod, int ukPrn, string collectionName, CancellationToken cancellationToken)
        {
            DataMatchDataLockValidationErrorInfo dataMatchDataLockValidationErrorInfo = new DataMatchDataLockValidationErrorInfo
            {
                DataLockValidationErrors = new List<DataLockValidationError>()
            };

            int dataLockSourceId = collectionName.StartsWith(Constants.ILR, StringComparison.OrdinalIgnoreCase) ? 1 : 2;

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                List<DataLockValidationError> dataLockValidationErrors =
                    await (from dle in dasPaymentsContext.DataLockEvents
                           join ee in dasPaymentsContext.EarningEvents on dle.EarningEventId equals ee.EventId
                           join dlenpp in dasPaymentsContext.DataLockEventNonPayablePeriods on dle.EventId equals dlenpp.DataLockEventId
                           join dlenpf in dasPaymentsContext.DataLockEventNonPayablePeriodFailures on dlenpp.DataLockEventNonPayablePeriodId equals dlenpf.DataLockEventNonPayablePeriodId
                           where (ukPrn == -1 || dle.Ukprn == ukPrn)
                                 && (collectionPeriod == -1 || (dle.CollectionPeriod == collectionPeriod && dlenpp.DeliveryPeriod == collectionPeriod))
                                 && dle.DataLockSourceId == dataLockSourceId
                                 && dle.LearningAimReference == Constants.ZPROG001
                                 && dle.IsPayable == false
                           select new DataLockValidationError
                           {
                               UkPrn = dle.Ukprn,
                               LearnerReferenceNumber = dle.LearnerReferenceNumber,
                               LearnerUln = dle.LearnerUln,
                               RuleId = dlenpf.DataLockFailureId,
                               AimSeqNumber = ee.LearningAimSequenceNumber,
                           })
                        .Distinct()
                        .ToListAsync(cancellationToken);

                dataMatchDataLockValidationErrorInfo.DataLockValidationErrors.AddRange(dataLockValidationErrors);
            }

            return dataMatchDataLockValidationErrorInfo;
        }

        public async Task<DataMatchDasApprenticeshipInfo> GetDasApprenticeshipInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchDasApprenticeshipInfo = new DataMatchDasApprenticeshipInfo
            {
                UkPrn = ukPrn,
                DasApprenticeshipInfos = new List<DasApprenticeshipInfo>()
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
                            select new DasApprenticeshipInfo
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

                dataMatchDasApprenticeshipInfo.DasApprenticeshipInfos.AddRange(dataMatchDasApprenticeshipPrices);
            }

            return dataMatchDasApprenticeshipInfo;
        }
    }
}