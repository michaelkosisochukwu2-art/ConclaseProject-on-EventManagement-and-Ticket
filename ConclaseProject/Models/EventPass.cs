using System;
using System.Collections.Generic;

using ConclaseProject.Models;

public class EventPass
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }

    public Guid AttendeeId { get; set; }

    public string UniqueVerificationToken { get; set; } = string.Empty;

    public PassStatus Status { get; set; } = PassStatus.Active;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastStatusUpdatedAt { get; set; }

    // Optimistic Concurrency Token to safely process sub-second gate check-ins
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Event Event { get; set; } = null!;

    public Attendee Attendee { get; set; } = null!;

    public ICollection<VerificationLog> VerificationLogs { get; set; } = new List<VerificationLog>();
}