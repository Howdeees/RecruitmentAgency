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

        // Отключаем валидацию для этих полей, так как мы заполняем их сами в контроллере
        [ValidateNever]
        public string EmployerId { get; set; }
        [ValidateNever]
        public IdentityUser Employer { get; set; }
    }
}