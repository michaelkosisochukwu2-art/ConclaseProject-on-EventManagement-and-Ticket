using System.ComponentModel.DataAnnotations;

namespace ConclaseProject.DTOs
{
    public class ScanRequestDto
    {
        [Required] public string UniqueVerificationToken { get; set; } = string.Empty;
        [Required] public string ScannedByDevice { get; set; } = string.Empty;
        [Required, RegularExpression("^(CHECKIN|CHECKOUT)$", ErrorMessage = "Action must be 'CHECKIN' or 'CHECKOUT'.")]
        public string ActionType { get; set; } = string.Empty;
    }
}
