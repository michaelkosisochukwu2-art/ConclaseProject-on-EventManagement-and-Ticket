using System.ComponentModel.DataAnnotations;

namespace ConclaseProject.DTOs
{
    public class RsvpRequestDto
    {
        [Required, StringLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, Phone] public string PhoneNumber { get; set; } = string.Empty;
    }
}
