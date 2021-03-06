﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Data
{
    public sealed class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<ICollection<DataLockValidationError>> GetDataLockValidationErrorInfoForUkprnAsync(int collectionPeriod, int ukPrn, string collectionYear, CancellationToken cancellationToken)
        {
            var academicYear = Convert.ToInt32(collectionYear);

            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                return await dasPaymentsContext.DataMatchReport
                    .Where(x => x.UkPrn == ukPrn
                                && x.AcademicYear == academicYear
                                && x.CollectionPeriod == collectionPeriod)
                    .Select(x => new DataLockValidationError
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
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ICollection<DataLockValidationError>> GetDataLockValidationErrorInfoForAllUkprnsAsync(int collectionPeriod, string collectionYear, CancellationToken cancellationToken)
        {
            var academicYear = Convert.ToInt32(collectionYear);

            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                return await dasPaymentsContext.DataMatchReport
                    .Where(x => x.AcademicYear == academicYear && x.CollectionPeriod == collectionPeriod)
                    .Select(x => new DataLockValidationError
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
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ICollection<DasApprenticeshipInfo>> GetDasApprenticeshipInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchDasApprenticeshipInfo = new List<DasApprenticeshipInfo>();

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                var dataMatchDasApprenticeshipPrices =
                     await (from a in dasPaymentsContext.Apprenticeships
                            join ape in dasPaymentsContext.ApprenticeshipPriceEpisodes on a.Id equals ape.ApprenticeshipId
                                into appPriceEpisodesJoin
                            from apej in appPriceEpisodesJoin.DefaultIfEmpty()
                            join ap in dasPaymentsContext.ApprenticeshipPauses on a.Id equals ap.ApprenticeshipId
                                into appPriceEpisodePausesJoin
                            from apepj in appPriceEpisodePausesJoin.DefaultIfEmpty()
                            where a.Ukprn == ukPrn
                            select new DasApprenticeshipInfo
                            {
                                UkPrn = ukPrn,
                                LearnerUln = a.Uln,
                                WithdrawnOnDate = a.StopDate,
                                PausedOnDate = apepj != null ? apepj.PauseDate : (DateTime?)null,
                                Cost = apej != null ? apej.Cost : 0,
                                LegalEntityName = a.LegalEntityName,
                                ProgrammeType = a.ProgrammeType,
                                StandardCode = a.StandardCode,
                                FrameworkCode = a.FrameworkCode,
                                PathwayCode = a.PathwayCode,
                            }).Distinct().ToListAsync(cancellationToken);

                dataMatchDasApprenticeshipInfo.AddRange(dataMatchDasApprenticeshipPrices);
            }

            return dataMatchDasApprenticeshipInfo;
        }
    }
}