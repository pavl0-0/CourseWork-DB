using System.ComponentModel.DataAnnotations;

namespace Kino.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required][MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime? BirthDate { get; set; }
        public int BonusPoints { get; set; } = 0;
        public string PasswordHash { get; set; } = string.Empty;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}