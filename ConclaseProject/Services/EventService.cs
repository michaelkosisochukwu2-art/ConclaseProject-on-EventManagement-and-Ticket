using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Interfaces;
using ConclaseProject.Models;
using ConclaseProject.DTOs;
using ConclaseProject .Interfaces;

namespace Gatherly.Infrastructure.Services
{
    public class EventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventPassRepository _passRepository;

        public EventService(IEventRepository eventRepository, IEventPassRepository passRepository)
        {
            _eventRepository = eventRepository;
            _passRepository = passRepository;
        }

        public async Task<EventSummaryDto> CreateNewEventAsync(CreateEventDto dto)
        {
            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Venue = dto.Venue,
                StartDateTime = dto.StartDateTime,
                Capacity = dto.Capacity,
                Visibility = dto.Visibility,
                IsReEntryAllowed = dto.IsReEntryAllowed
            };

            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();

            return new EventSummaryDto
            {
                Id = newEvent.Id,
                Title = newEvent.Title,
                Venue = newEvent.Venue,
                StartDateTime = newEvent.StartDateTime,
                AvailableSlots = newEvent.Capacity
            };
        }

        public async Task<IEnumerable<EventSummaryDto>> GetLandingPageFeedAsync()
        {
            var events = await _eventRepository.GetActivePublicEventsAsync();
            return events.Select(e => new EventSummaryDto
            {
                Id = e.Id,
                Title = e.Title,
                Venue = e.Venue,
                StartDateTime = e.StartDateTime,
                AvailableSlots = e.Capacity - e.EventPasses.Count
            });
        }
    }
}