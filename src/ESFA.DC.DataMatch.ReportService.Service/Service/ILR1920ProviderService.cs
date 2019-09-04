using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class ILR1920ProviderService : IILRProviderService
    {
        private readonly Func<IIlr1920ValidContext> _ilrValidContextFactory;

        public ILR1920ProviderService(
            ILogger logger,
            Func<IIlr1920ValidContext> ilrValidContextFactory)
        {
            _ilrValidContextFactory = ilrValidContextFactory;
        }

        public async Task<DataMatchILRInfo> GetILRInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchILRInfo = new DataMatchILRInfo()
            {
                UkPrn = ukPrn,
                DataMatchLearners = new List<DataMatchLearner>(),
            };

            cancellationToken.ThrowIfCancellationRequested();
            List<Learner> learnersList;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learnersList = await ilrContext.Learners
                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.AppFinRecords)
                    .Include(x => x.LearningDeliveries).ThenInclude(y => y.LearningDeliveryFAMs)
                    .Where(x => x.UKPRN == ukPrn && x.LearningDeliveries.Any(y => y.FundModel == Constants.ApprenticeshipsFundModel
                                                 && y.LearningDeliveryFAMs.Any(ldf => ldf.LearnDelFAMCode == "1" && ldf.LearnDelFAMType == "ACT")))

                    .Distinct().ToListAsync(cancellationToken);
            }

            foreach (var learner in learnersList)
            {
                var dataMatchLearner = new DataMatchLearner()
                {
                    UkPrn = learner.UKPRN,
                    LearnRefNumber = learner.LearnRefNumber,
                    Uln = learner.ULN,
                    DataMatchLearningDeliveries = learner.LearningDeliveries.Select(x => new DataMatchLearningDelivery()
                    {
                        UkPrn = ukPrn,
                        LearnRefNumber = x.LearnRefNumber,
                        LearnAimRef = x.LearnAimRef,
                        AimSeqNumber = x.AimSeqNumber,
                        LearnStartDate = x.LearnStartDate,
                        ProgType = x.ProgType,
                        StdCode = x.StdCode,
                        FworkCode = x.FworkCode,
                        PwayCode = x.PwayCode,
                        AppFinRecords = x.AppFinRecords.Select(y => new AppFinRecordInfo()
                        {
                            LearnRefNumber = y.LearnRefNumber,
                            AimSeqNumber = y.AimSeqNumber,
                            AFinType = y.AFinType,
                            AFinCode = y.AFinCode,
                            AFinDate = y.AFinDate,
                            AFinAmount = y.AFinAmount,
                        }).ToList(),
                    }).ToList(),
                };

                dataMatchILRInfo.DataMatchLearners.Add(dataMatchLearner);
            }

            return dataMatchILRInfo;
        }
    }
}
