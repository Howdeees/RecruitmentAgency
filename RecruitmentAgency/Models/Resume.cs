using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAgency.Models
{
    public class Resume
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Skills { get; set; }

        public string Experience { get; set; }

        public string Education { get; set; }

        public decimal DesiredSalary { get; set; }

        public string UserId { get; set; }
    }
}