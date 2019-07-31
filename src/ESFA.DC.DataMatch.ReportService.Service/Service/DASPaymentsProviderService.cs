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

        public async Task<DataMatchDataLockValidationErrorInfo> GetDataLockValidationErrorInfoForDataMatchReport(int collectionPeriod, int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchDataLockValidationErrorInfo = new DataMatchDataLockValidationErrorInfo()
            {
                DataLockValidationErrors = new List<DataLockValidationError>(),
            };

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                var dataLockValidationErrors =
                    await (from dle in dasPaymentsContext.DataLockEvents
                           join dlenpp in dasPaymentsContext.DataLockEventNonPayablePeriods on dle.EventId equals dlenpp.DataLockEventId
                           join dlenpf in dasPaymentsContext.DataLockEventNonPayablePeriodFailures on dlenpp.DataLockEventNonPayablePeriodId equals dlenpf.DataLockEventNonPayablePeriodId
                           where dle.Ukprn == ukPrn && dle.CollectionPeriod == collectionPeriod
                           select new
                            {
                                dle.Ukprn,
                                dle.LearnerReferenceNumber,
                                dle.LearningAimStandardCode,
                                dle.LearningAimFrameworkCode,
                                dle.LearningAimProgrammeType,
                                dle.LearningAimPathwayCode,
                                dle.LearnerUln,
                                dlenpf.DataLockFailureId,
                            }).Distinct().ToListAsync(cancellationToken);

                foreach (var dataLockValidationError in dataLockValidationErrors)
                {
                    dataMatchDataLockValidationErrorInfo.DataLockValidationErrors.Add(new DataLockValidationError()
                    {
                        UkPrn = dataLockValidationError.Ukprn,
                        LearnerReferenceNumber = dataLockValidationError.LearnerReferenceNumber,
                        StandardCode = dataLockValidationError.LearningAimStandardCode,
                        FrameworkCode = dataLockValidationError.LearningAimFrameworkCode,
                        ProgrammeType = dataLockValidationError.LearningAimProgrammeType,
                        PathwayCode = dataLockValidationError.LearningAimPathwayCode,
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
                DasApprenticeshipPriceInfos = new List<DasApprenticeshipPriceInfo>(),
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
                            select new DasApprenticeshipPriceInfo()
                            {
                                LearnerUln = a.Uln,
                                WithdrawnOnDate = a.StopDate,
                                PausedOnDate = apepj.PauseDate,
                                Cost = apej.Cost,
                                LegalEntityName = a.LegalEntityName,
                            }).Distinct().ToListAsync(cancellationToken);

                dataMatchDasApprenticeshipInfo.DasApprenticeshipPriceInfos = dataMatchDasApprenticeshipPrices;
            }

            return dataMatchDasApprenticeshipInfo;
        }
    }
}
