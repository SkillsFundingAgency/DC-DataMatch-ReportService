using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Comparer
{
    public sealed class DataMatchModelComparer : IComparer<DataMatchModel>
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

            if (x.AimSeqNumber > y.AimSeqNumber)
            {
                return 1;
            }

            if (x.AimSeqNumber < y.AimSeqNumber)
            {
                return -1;
            }

            return string.CompareOrdinal(x.RuleName, y.RuleName);
        }
    }
}
