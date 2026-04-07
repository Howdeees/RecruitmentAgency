using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentAgency.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }
        [ValidateNever]
        public Vacancy Vacancy { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ValidateNever]
        public IdentityUser User { get; set; }

        [Display(Name = "Выберите резюме")]
        [Required(ErrorMessage = "Необходимо выбрать резюме для отклика")]
        public int ResumeId { get; set; }
        [ValidateNever]
        public Resume Resume { get; set; }

        [Required(ErrorMessage = "Напишите сопроводительное письмо")]
        [Display(Name = "Сопроводительное письмо")]
        public string CoverLetter { get; set; }
        public string? RecruiterNotes { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
    }

    public enum ApplicationStatus
    {
        [Display(Name = "Новый")] New,
        [Display(Name = "Рассматривается")] UnderReview,
        [Display(Name = "Отказ")] Rejected,
        [Display(Name = "Отказ работодателем")] RejectedEmployer,
        [Display(Name = "Отказ рекрутером")] RejectedRecruiter,
        [Display(Name = "Собеседование")] Interview,
        [Display(Name = "Принят")] Accepted,
        [Display(Name = "Рекомендован")] Recommended

    }
}