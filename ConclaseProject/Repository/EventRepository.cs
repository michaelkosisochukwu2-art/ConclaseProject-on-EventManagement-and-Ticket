using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConclaseProject.Interfaces;
using ConclaseProject.Models;
using ConclaseProject.Interfaces;
using ConclaseProject. Data;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly EventDbContext _context;

        public EventRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.EventPasses)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetActivePublicEventsAsync()
        {
            return await _context.Events
                .Include(e => e.EventPasses)
                .Where(e => e.Visibility == EventVisibility.Public && e.StartDateTime >= DateTime.UtcNow)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        public async Task AddAsync(Event @event)
        {
            await _context.Events.AddAsync(@event);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}