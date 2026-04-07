namespace RecruitmentAgency.Models
{
    public class Candidate
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        public string RecruiterId { get; set; }

        public string Comment { get; set; }
    }
}