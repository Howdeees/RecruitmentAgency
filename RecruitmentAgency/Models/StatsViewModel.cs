using System.Collections.Generic;

namespace RecruitmentAgency.Models
{
    public class StatsViewModel
    {
        public int TotalVacancies { get; set; }
        public int TotalApplications { get; set; }
        public int HiredCount { get; set; }
        public int TotalViews { get; set; }

        public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();

        public List<KeyValuePair<string, int>> TopVacanciesByApps { get; set; } = new();

        public List<KeyValuePair<string, int>> TopVacanciesByViews { get; set; } = new();

        public int ConversionRate => TotalApplications > 0 ? (HiredCount * 100 / TotalApplications) : 0;
    }
}