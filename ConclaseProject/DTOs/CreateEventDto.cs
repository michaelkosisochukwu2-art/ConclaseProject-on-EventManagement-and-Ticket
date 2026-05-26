using System.ComponentModel.DataAnnotations;
using ConclaseProject.Models;

namespace ConclaseProject.DTOs
{
    public class CreateEventDto
    {
        [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
        [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Venue { get; set; } = string.Empty;
        [Required] public DateTime StartDateTime { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")] public int Capacity { get; set; }
        public EventVisibility Visibility { get; set; } = EventVisibility.Public;
        public bool IsReEntryAllowed { get; set; } = true;
    }
}
