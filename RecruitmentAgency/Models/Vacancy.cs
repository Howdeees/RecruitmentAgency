using Microsoft.AspNetCore.Identity;

public class Vacancy
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Requirements { get; set; }
    public decimal Salary { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public string EmployerId { get; set; }
    public IdentityUser Employer { get; set; }
}