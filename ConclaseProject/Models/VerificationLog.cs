using System;

namespace ConclaseProject.Models
{
    public class VerificationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EventPassId { get; set; }

        public string ActionType { get; set; } = string.Empty; // "CHECKIN" or "CHECKOUT"

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string ScannedByDevice { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }

        public string Notes { get; set; } = string.Empty; // Captures validation details / fraud warnings

        public EventPass EventPass { get; set; } = null!;
    }
}