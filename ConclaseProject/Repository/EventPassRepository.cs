// Infrastructure/Repositories/EventPassRepository.cs
using System;
using System.Threading.Tasks;
using ConclaseProject.Interfaces;
using ConclaseProject.Models;
using ConclaseProject.Data;
using Microsoft.EntityFrameworkCore;

namespace ConclaseProject.Repositories
{
    public class EventPassRepository : IEventPassRepository
    {
        private readonly EventDbContext _context;

        public EventPassRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<EventPass?> GetByTokenWithDetailsAsync(string token)
        {
            return await _context.EventPasses
                .Include(ep => ep.Event)
                .Include(ep => ep.Attendee)
                .FirstOrDefaultAsync(ep => ep.UniqueVerificationToken == token);
        }

        public async Task<bool> HasAttendeeRegisteredAsync(Guid eventId, string email)
        {
            return await _context.EventPasses
                .AnyAsync(ep => ep.EventId == eventId && ep.Attendee.Email.ToLower() == email.ToLower());
        }

        public async Task<Attendees?> GetAttendeeByEmailAsync(string email)
        {
            return await _context.Attendees
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
        }

        public async Task AddAttendeeAsync(Attendees attendee)
        {
            await _context.Attendees.AddAsync(attendee);
        }

        public async Task AddPassAsync(EventPass pass)
        {
            await _context.EventPasses.AddAsync(pass);
        }

        public async Task AddVerificationLogAsync(VerificationLog log)
        {
            await _context.VerificationLogs.AddAsync(log);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}