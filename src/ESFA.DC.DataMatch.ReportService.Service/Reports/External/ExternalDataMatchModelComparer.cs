using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.External
{
    public class ExternalDataMatchModelComparer : IComparer<DataMatchModel>
    {
        public int Compare(DataMatchModel x, DataMatchModel y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (x == y)
            {
                return 0;
            }

            int cmp = string.CompareOrdinal(x.LearnRefNumber, y.LearnRefNumber);
            if (cmp != 0)
            {
                return cmp;
            }

            cmp = x.AimSeqNumber.GetValueOrDefault(0).CompareTo(y.AimSeqNumber.GetValueOrDefault(0));
            if (cmp != 0)
            {
                return cmp;
            }

            return string.CompareOrdinal(x.RuleName, y.RuleName);
        }
    }
}
