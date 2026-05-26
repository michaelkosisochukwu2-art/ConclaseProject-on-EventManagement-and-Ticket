
using System;
using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConclaseProject.Api.Controllers
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
        // PUT: api/events/guests/{passId}
        // Allows organizers to update a guest's details or change their ticket state manually
        [Authorize(Roles = "Organizer")]//Secured:Only Logged-in user with 'organizer' can update guest details or ticket status
        [HttpPut("guests/{passId}")]
        public async Task<IActionResult> UpdateGuest(Guid passId, [FromBody] UpdatedGuestDto dto)
        {
            var success = await _verificationService.UpdateGuestAsync(passId, dto);
            if (!success)
            {
                return NotFound(new { error = "Guest pass record not found or no changes made." });
            }

            return Ok(new { message = "Guest profile updated successfully by organizer." });
        }

        // DELETE: api/events/guests/{passId}
        // Removes the attendee entirely from this specific event's guest list
        [Authorize(Roles ="Organizer")]//Only logged-in organizers can delete guests
        [HttpDelete("guests/{passId}")]
        public async Task<IActionResult> DeleteGuest(Guid passId)
        {
            var success = await _verificationService.DeleteGuestFromEventAsync(passId);
            if (!success)
            {
                return NotFound(new { error = "Guest pass record not found." });
            }

            return Ok(new { message = "Guest has been successfully removed from the event guest list." });
        }
            [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var result = await _eventService.CreateNewEventAsync(dto);
            return CreatedAtAction(nameof(GetLandingFeed), new { id = result.Id }, result);
        }

        [HttpGet("landing")]//anyone can browse the landing page feed
        public async Task<IActionResult> GetLandingFeed()
        {
            var feed = await _eventService.GetLandingPageFeedAsync();
            return Ok(feed);
        }
        [Authorize]//Secured:Only logged in users can rsvp to an event
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