using System.ComponentModel.DataAnnotations;

namespace RecruitmentAgency.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        public int VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }

        public string Message { get; set; }

        public string Status { get; set; } = "New";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}