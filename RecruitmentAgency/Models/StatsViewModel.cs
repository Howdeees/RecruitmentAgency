namespace RecruitmentAgency.Models
{
    public class StatsViewModel
    {
        // Общие цифры
        public int TotalVacancies { get; set; }
        public int ActiveVacancies { get; set; }
        public int TotalApplications { get; set; }
        public int HiredCount { get; set; } // Статус "Accepted"

        // Группировки для графиков или списков
        public Dictionary<string, int> ApplicationsByStatus { get; set; }
        public List<VacancyStats> TopVacancies { get; set; }
    }

    public class VacancyStats
    {
        public string Title { get; set; }
        public int Count { get; set; }
    }
}