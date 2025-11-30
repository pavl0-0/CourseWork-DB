using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required][MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string LastName { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string Position { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Salary { get; set; }

        [EmailAddress] public string? Email { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
    }
}