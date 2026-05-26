using System.ComponentModel.DataAnnotations;
using ConclaseProject.Models;

namespace ConclaseProject.DTOs
{
    public class UpdatedGuestDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public PassStatus Status { get; set; } // Allows organizers to manually set to Revoked, Active, etc.
    }
}
