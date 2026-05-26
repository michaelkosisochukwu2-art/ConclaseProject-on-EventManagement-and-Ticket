using ConclaseProject.Models;

namespace ConclaseProject.Interfaces
{
    public interface IEventPassRepository
    {
        Task<EventPass?> GetByTokenWithDetailsAsync(string token);
        Task<bool> HasAttendeeRegisteredAsync(Guid eventId, string email);
        Task<Attendees?> GetAttendeeByEmailAsync(string email);
        Task AddAttendeeAsync(Attendees attendee);
        Task AddPassAsync(EventPass pass);
        Task AddVerificationLogAsync(VerificationLog log);
        Task<bool> SaveChangesAsync();
    }
}
