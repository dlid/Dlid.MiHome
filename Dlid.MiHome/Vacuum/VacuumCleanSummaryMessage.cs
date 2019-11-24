using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Vacuum
{
    public class VacuumCleanSummaryMessage
    {

        public TimeSpan TotalCleaningTime { get; set; }

        public  int TotalArea { get; set; }

        public int TotalCleanups { get; set; }

        public int[] CleaningRecordIds { get; set; }

    }
}
