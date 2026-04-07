using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentAgency.Models
{
    public class Resume
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите заголовок резюме (напр. .NET Разработчик)")]
        [Display(Name = "Желаемая должность")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите ваше ФИО")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Укажите ваши навыки")]
        [Display(Name = "Навыки и технологии")]
        public string Skills { get; set; }

        [Required(ErrorMessage = "Опишите ваш опыт работы")]
        [Display(Name = "Опыт работы")]
        public string Experience { get; set; }

        [Display(Name = "Образование")]
        public string Education { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        [ValidateNever]
        public IdentityUser User { get; set; }

        [ValidateNever]
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}