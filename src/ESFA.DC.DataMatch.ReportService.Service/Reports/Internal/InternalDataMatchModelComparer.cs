using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.Internal
{
    public sealed class InternalDataMatchModelComparer : IComparer<InternalDataMatchModel>
    {
        public int Compare(InternalDataMatchModel x, InternalDataMatchModel y)
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

            int cmp = string.CompareOrdinal(x.Collection, y.Collection);
            if (cmp != 0)
            {
                return cmp;
            }

            cmp = x.Ukprn.CompareTo(y.Ukprn);
            if (cmp != 0)
            {
                return cmp;
            }

            return string.CompareOrdinal(x.LearnRefNumber, y.LearnRefNumber);
        }
    }
}
