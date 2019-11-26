using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class FM361920ProviderService : IFM36ProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public FM361920ProviderService(Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public async Task<DataMatchRulebaseInfo> GetFM36DataForDataMatchReport(int ukPrn, CancellationToken cancellationToken)
        {
            var dataMatchRulebaseInfo = new DataMatchRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>(),
            };

            cancellationToken.ThrowIfCancellationRequested();
            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                var aecApprenticeshipPriceEpisodeInfos = await ilrContext.AEC_ApprenticeshipPriceEpisodes
                    .Where(x => x.UKPRN == ukPrn &&
                                x.EpisodeStartDate >= Constants.PriceEpisodeStartDateStart1920 &&
                                x.EpisodeStartDate <= Constants.PriceEpisodeStartDateEnd1920)
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

                dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.AddRange(aecApprenticeshipPriceEpisodeInfos);
            }

            return dataMatchRulebaseInfo;
        }
    }
}
