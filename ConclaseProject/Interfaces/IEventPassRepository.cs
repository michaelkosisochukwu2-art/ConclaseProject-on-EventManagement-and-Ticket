using ConclaseProject.Models;

namespace ConclaseProject.Interfaces
{
    public interface IEventPassRepository
    {
        Task<EventPass?> GetByTokenWithDetailsAsync(string token);
        Task<bool> HasAttendeeRegisteredAsync(Guid eventId, string email);
        Task<Attendee?> GetAttendeeByEmailAsync(string email);
        Task AddAttendeeAsync(Attendee attendee);
        Task AddPassAsync(EventPass pass);
        Task AddVerificationLogAsync(VerificationLog log);
        Task<bool> SaveChangesAsync();
        Task<EventPass?> GetByIdWithAttendeeAsync(Guid passId);
        void DeletePass(EventPass pass);
    }
}
