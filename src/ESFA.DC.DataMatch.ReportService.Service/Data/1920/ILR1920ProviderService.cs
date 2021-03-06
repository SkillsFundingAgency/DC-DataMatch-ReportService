﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Data
{
    public class ILR1920ProviderService : IILRProviderService
    {
        private const int PageSize = 1000;

        private readonly Func<IIlr1920ValidContext> _ilrValidContextFactory;
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public ILR1920ProviderService(Func<IIlr1920ValidContext> ilrValidContextFactory, Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
        {
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public DateTime PriceEpisodeStartDateStart { get; } = new DateTime(2019, 08, 01);

        public DateTime PriceEpisodeStartDateEnd { get; } = new DateTime(2020, 07, 31);

        public async Task<ICollection<DataMatchLearner>> GetILRInfoForDataMatchReportAsync(int ukPrn, List<long> learners, CancellationToken cancellationToken)
        {
            var dataMatchLearners = new List<DataMatchLearner>();

            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrValidContextFactory())
            {
                int count = learners.Count;

                for (int i = 0; i < count; i += PageSize)
                {
                    var learnerUlnPage = learners.Skip(i).Take(PageSize).ToList();

                    List<DataMatchLearner> learnersList = await ilrContext.Learners
                        .Where(x => x.UKPRN == ukPrn
                                    && learnerUlnPage.Contains(x.ULN)
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

        public async Task<ICollection<AECApprenticeshipPriceEpisodeInfo>> GetFM36DataForDataMatchReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                return await ilrContext.AEC_ApprenticeshipPriceEpisodes
                    .Where(x => x.UKPRN == ukPrn &&
                                x.EpisodeStartDate >= PriceEpisodeStartDateStart &&
                                x.EpisodeStartDate <= PriceEpisodeStartDateEnd)
                    .Select(pe => new AECApprenticeshipPriceEpisodeInfo
                    {
                        UkPrn = pe.UKPRN,
                        AimSequenceNumber = (int)pe.PriceEpisodeAimSeqNumber,
                        LearnRefNumber = pe.LearnRefNumber,
                        PriceEpisodeActualEndDate = pe.PriceEpisodeActualEndDate,
                        PriceEpisodeAgreeId = pe.PriceEpisodeAgreeId,
                        EpisodeStartDate = pe.EpisodeStartDate,
                        EffectiveTnpStartDate = pe.EpisodeEffectiveTNPStartDate,
                    }).ToListAsync(cancellationToken);
            }
        }
    }
}
