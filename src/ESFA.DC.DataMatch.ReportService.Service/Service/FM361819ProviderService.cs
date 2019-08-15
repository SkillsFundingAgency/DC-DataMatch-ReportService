using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class FM361819ProviderService : IFM36ProviderService
    {
        private readonly Func<IIlr1819RulebaseContext> _ilrRulebaseContextFactory;

        public FM361819ProviderService(Func<IIlr1819RulebaseContext> ilrRulebaseContextFactory)
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
                    .Where(x => x.UKPRN == ukPrn)
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
