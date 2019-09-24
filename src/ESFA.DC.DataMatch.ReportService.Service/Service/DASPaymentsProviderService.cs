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

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                var dataLockValidationErrors = await dasPaymentsContext.DataMatchReport
                    .Where(x => (ukPrn == -1 || x.UkPrn == ukPrn) &&
                                (collectionPeriod == -1 || (x.CollectionPeriod == collectionPeriod && x.DeliveryPeriod == collectionPeriod)))
                    .Distinct()
                    .Select(x => new DataLockValidationError()
                    {
                        UkPrn = x.UkPrn,
                        LearnerReferenceNumber = x.LearnerReferenceNumber,
                        LearnerUln = x.LearnerUln,
                        RuleId = x.DataLockFailureId,
                        AimSeqNumber = x.LearningAimSequenceNumber,
                        Collection = x.DataLockSourceId == Constants.SubmissionInMonth ? Constants.ILR : Constants.PeriodEnd,
                        CollectionPeriod = x.CollectionPeriod,
                        LastSubmission = x.IlrSubmissionDateTime
                    })
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