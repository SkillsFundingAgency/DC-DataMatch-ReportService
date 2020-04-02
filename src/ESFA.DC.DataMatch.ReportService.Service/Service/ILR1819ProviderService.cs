using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class ILR1819ProviderService : IILRProviderService
    {
        private readonly Func<IIlr1819ValidContext> _ilrValidContextFactory;

        public ILR1819ProviderService(
            ILogger logger,
            Func<IIlr1819ValidContext> ilrValidContextFactory)
        {
            _ilrValidContextFactory = ilrValidContextFactory;
        }

        public async Task<ICollection<DataMatchLearner>> GetILRInfoForDataMatchReport(
            int ukPrn,
            List<long> learners,
            CancellationToken cancellationToken)
        {
            var dataMatchLearners = new List<DataMatchLearner>();

            cancellationToken.ThrowIfCancellationRequested();
            using (var ilrContext = _ilrValidContextFactory())
            {
                int count = learners.Count;
                int pageSize = 1000;

                for (int i = 0; i < count; i += pageSize)
                {
                    List<DataMatchLearner> learnersList = await ilrContext.Learners
                        .Where(x => x.UKPRN == ukPrn
                                    && learners.Skip(i).Take(pageSize).Contains(x.ULN)
                                    && x.LearningDeliveries.Any(y =>
                                        y.FundModel == Constants.ApprenticeshipsFundModel
                                        && y.LearningDeliveryFAMs.Any(ldf =>
                                            ldf.LearnDelFAMCode == Constants.LearnDelFAMCode &&
                                            ldf.LearnDelFAMType == Constants.LearnDelFAMType_ACT)))
                        .Select(l => new DataMatchLearner
                        {
                            UkPrn = l.UKPRN,
                            LearnRefNumber = l.LearnRefNumber,
                            Uln = l.ULN,
                            DataMatchLearningDeliveries = l.LearningDeliveries.Select(x => new DataMatchLearningDelivery
                            {
                                LearnAimRef = x.LearnAimRef,
                                AimSeqNumber = x.AimSeqNumber,
                                LearnStartDate = x.LearnStartDate,
                                ProgType = x.ProgType,
                                StdCode = x.StdCode,
                                FworkCode = x.FworkCode,
                                PwayCode = x.PwayCode,
                                AppFinRecords = x.AppFinRecords.Select(y => new AppFinRecordInfo()
                                {
                                    AFinType = y.AFinType,
                                    AFinCode = y.AFinCode,
                                    AFinDate = y.AFinDate,
                                    AFinAmount = y.AFinAmount,
                                }).ToList(),
                            }).ToList(),
                        })
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    dataMatchLearners.AddRange(learnersList);
                }
            }

            return dataMatchLearners;
        }
    }
}
