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
        public async Task<EventPass?> GetByIdWithAttendeeAsync(Guid passId)
        {
            return await _context.EventPasses
                .Include(ep => ep.Attendee)
                .Include(ep => ep.Event)
                .FirstOrDefaultAsync(ep => ep.Id == passId);
        }

        public void DeletePass(EventPass pass)
        {
            // This safely removes the ticket/pass allocation.
            // If the attendee has NO other tickets, you could optionally delete the attendee too,
            // but usually, removing the EventPass is what "deletes them from the guest list".
            _context.EventPasses.Remove(pass);
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

        public async Task<Attendee?> GetAttendeeByEmailAsync(string email)
        {
            return await _context.Attendees
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
        }

        public async Task AddAttendeeAsync(Attendee attendee)
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