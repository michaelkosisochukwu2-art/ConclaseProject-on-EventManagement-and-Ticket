namespace ConclaseProject.DTOs
{
    public class VerificationResultDto
    {
        public bool IsSuccess { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string AttendeeName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string CurrentPassStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
