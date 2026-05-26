
using System;
using System.Threading.Tasks;
using ConclaseProject.DTOs;
using Gatherly.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly VerificationService _verificationService;

        public EventsController(EventService eventService, VerificationService verificationService)
        {
            _eventService = eventService;
            _verificationService = verificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var result = await _eventService.CreateNewEventAsync(dto);
            return CreatedAtAction(nameof(GetLandingFeed), new { id = result.Id }, result);
        }

        [HttpGet("landing")]
        public async Task<IActionResult> GetLandingFeed()
        {
            var feed = await _eventService.GetLandingPageFeedAsync();
            return Ok(feed);
        }

        [HttpPost("{id}/rsvp")]
        public async Task<IActionResult> SubmitRsvp(Guid id, [FromBody] RsvpRequestDto request)
        {
            try
            {
                var response = await _verificationService.ProcessRsvpAsync(id, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}