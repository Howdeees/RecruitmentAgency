using Microsoft.AspNetCore.Identity;
namespace RecruitmentAgency.Models
{

    public class Resume
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public decimal DesiredSalary { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}