using System;
using System.Collections.Generic;

public class Attendees
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventPass> EventPasses { get; set; } = new List<EventPass>();
}