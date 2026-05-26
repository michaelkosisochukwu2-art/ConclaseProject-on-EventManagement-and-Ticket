namespace ConclaseProject.DTOs
{
    public class RegistrationResponseDto
    {
        public string AttendeeName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string UniqueVerificationToken { get; set; } = string.Empty;
        public string PassStatus { get; set; } = string.Empty;
    }
}
