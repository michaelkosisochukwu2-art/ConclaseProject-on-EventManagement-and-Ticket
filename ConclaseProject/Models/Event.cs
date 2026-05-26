using System;
using System.Collections.Generic;

namespace ConclaseProject.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Venue { get; set; } = string.Empty;

        public DateTime StartDateTime { get; set; }

        public int Capacity { get; set; }

        public EventVisibility Visibility { get; set; } = EventVisibility.Public;

        public bool IsReEntryAllowed { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EventPass> EventPasses { get; set; } = new List<EventPass>();
    }
}