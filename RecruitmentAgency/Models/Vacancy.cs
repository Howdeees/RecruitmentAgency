using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace RecruitmentAgency.Models
{
    public class Vacancy
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }

        public string Schedule { get; set; }

        public decimal Salary { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string EmployerId { get; set; }
        public IdentityUser Employer { get; set; }
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}