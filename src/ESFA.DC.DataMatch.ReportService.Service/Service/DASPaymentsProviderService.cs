using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.DASPayments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public sealed class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<List<DasApprenticeshipInfo>> GetApprenticeshipsInfoAsync(long ukPrn, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (IDASPaymentsContext context = _dasPaymentsContextFactory())
            {
                var apprenticeshipsData = await
                    (from a in context.Apprenticeships
                        join ap in context.ApprenticeshipPauses on a.Id equals ap.ApprenticeshipId
                        join ape in context.ApprenticeshipPriceEpisodes on a.Id equals ape.ApprenticeshipId
                        join dlenppf in context.DataLockEventNonPayablePeriodFailures on a.Id equals dlenppf
                            .ApprenticeshipId
                        join dlenpp in context.DataLockEventNonPayablePeriods on dlenppf.DataLockEventNonPayablePeriodId
                            equals dlenpp.DataLockEventNonPayablePeriodId
                        join dle in context.DataLockEvents on dlenpp.DataLockEventId equals dle.EventId
                        join ee in context.EarningEvents on dle.EarningEventId equals ee.EventId
                        where a.Ukprn == ukPrn
                        select new DasApprenticeshipInfo()
                        {
                            AppreticeshipId = a.Id,
                            AgreementId = a.AgreementId,
                            AgreedOnDate = a.AgreedOnDate,
                            Uln = a.Uln,
                            UkPrn = a.Ukprn,
                            EstimatedStartDate = a.EstimatedStartDate,
                            EstimatedEndDate = a.EstimatedEndDate,
                            StandardCode = a.StandardCode,
                            ProgrammeType = a.ProgrammeType,
                            FrameworkCode = a.FrameworkCode,
                            PathwayCode = a.PathwayCode,
                            StopDate = a.StopDate,
                            LegalEntityName = a.LegalEntityName,
                            PauseDate = ap.PauseDate,
                            Cost = ape.Cost,
                            RuleId = dlenppf.DataLockFailureId,
                            LearnerReferenceNumber = dle.LearnerReferenceNumber,
                            AimSequenceNumber = ee.LearningAimSequenceNumber
                        }).ToListAsync(cancellationToken);

                return apprenticeshipsData;
            }
        }
    }
}
