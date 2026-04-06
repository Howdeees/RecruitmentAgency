namespace RecruitmentAgency.Models
{
    public class Interview
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public DateTime Date { get; set; }

        public string Result { get; set; }

        public string Comment { get; set; }
    }
}