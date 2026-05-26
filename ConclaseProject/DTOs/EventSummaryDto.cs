namespace ConclaseProject.DTOs
{
    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public int AvailableSlots { get; set; }
    }
}
