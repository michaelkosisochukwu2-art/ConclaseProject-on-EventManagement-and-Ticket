using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConclaseProject.Models;

namespace ConclaseProject.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id);

        Task<IEnumerable<Event>> GetActivePublicEventsAsync();

        Task AddAsync(Event @event);

        Task<bool> SaveChangesAsync();
       
    }
}