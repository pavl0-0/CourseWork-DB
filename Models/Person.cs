using System.ComponentModel.DataAnnotations;

namespace Kino.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}