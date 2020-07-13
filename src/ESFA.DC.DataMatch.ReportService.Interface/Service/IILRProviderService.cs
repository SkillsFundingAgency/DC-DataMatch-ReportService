using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IILRProviderService
    {
        DateTime PriceEpisodeStartDateStart { get; }

        DateTime PriceEpisodeStartDateEnd { get; }

        Task<ICollection<DataMatchLearner>> GetILRInfoForDataMatchReportAsync(int ukPrn, List<long> learners, CancellationToken cancellationToken);

        Task<ICollection<AECApprenticeshipPriceEpisodeInfo>> GetFM36DataForDataMatchReportAsync(int ukPrn, CancellationToken cancellationToken);
    }
}
