using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Interfaces;
using ConclaseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ConclaseProject.Services
{
    public class VerificationService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventPassRepository _passRepository;

        public VerificationService(IEventRepository eventRepository, IEventPassRepository passRepository)
        {
            _eventRepository = eventRepository;
            _passRepository = passRepository;
        }
        public async Task<bool> UpdateGuestAsync(Guid passId, UpdatedGuestDto dto)
        {
            var pass = await _passRepository.GetByIdWithAttendeeAsync(passId);
            if (pass == null) return false;

            // Update attendee personal details
            pass.Attendee.FullName = dto.FullName;
            pass.Attendee.PhoneNumber = dto.PhoneNumber;

            // Update ticket status (e.g., Organizer revoking a ticket manually)
            pass.Status = dto.Status;
            pass.LastStatusUpdatedAt = DateTime.UtcNow;

            return await _passRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteGuestFromEventAsync(Guid passId)
        {
            var pass = await _passRepository.GetByIdWithAttendeeAsync(passId);
            if (pass == null) return false;

            // Log a system note or handle logic before deleting if needed
            _passRepository.DeletePass(pass);

            return await _passRepository.SaveChangesAsync();
        }

        public async Task<RegistrationResponseDto> ProcessRsvpAsync(Guid eventId, RsvpRequestDto request)
        {
            var targetEvent = await _eventRepository.GetByIdAsync(eventId);
            if (targetEvent == null) throw new ArgumentException("Target event profile not found.");

            // 1. Enforce strict capacity checking before processing registration records
            // NOTE: Ensure your Repository implementation uses .Include(e => e.EventPasses) when fetching by ID!
            if (targetEvent.EventPasses.Count >= targetEvent.Capacity)
            {
                throw new InvalidOperationException("Registration closed. Event is at full capacity.");
            }

            // 2. Prevent duplicate entries early
            bool alreadyRegistered = await _passRepository.HasAttendeeRegisteredAsync(eventId, request.Email);
            if (alreadyRegistered)
            {
                throw new InvalidOperationException("An entry pass has already been issued to this email address.");
            }

            // 3. Resolve or create unique individual identity profile records
            // Note: Updated 'Attendees' to singular 'Attendee' to match entity standard conventions. 
            // If you chose to keep the class pluralized, change this back to 'Attendees'.
            var attendee = await _passRepository.GetAttendeeByEmailAsync(request.Email);
            if (attendee == null)
            {
                attendee = new Attendee
                {
                    FullName = request.FullName,
                    Email = request.Email.ToLower().Trim(),
                    PhoneNumber = request.PhoneNumber
                };
                await _passRepository.AddAttendeeAsync(attendee);
                await _passRepository.SaveChangesAsync();
            }

            // 4. Issue dynamically protected validation token
            string token = $"GTHR-{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";

            var pass = new EventPass
            {
                EventId = eventId,
                AttendeeId = attendee.Id,
                UniqueVerificationToken = token,
                Status = PassStatus.Active
            };

            await _passRepository.AddPassAsync(pass);
            await _passRepository.SaveChangesAsync();

            return new RegistrationResponseDto
            {
                AttendeeName = attendee.FullName,
                EventTitle = targetEvent.Title,
                UniqueVerificationToken = pass.UniqueVerificationToken,
                PassStatus = pass.Status.ToString()
            };
        }

        public async Task<VerificationResultDto> ExecuteGateValidationAsync(ScanRequestDto scan)
        {
            var executionTime = DateTime.UtcNow;

            var pass = await _passRepository.GetByTokenWithDetailsAsync(scan.UniqueVerificationToken);
            if (pass == null)
            {
                return new VerificationResultDto
                {
                    IsSuccess = false,
                    StatusMessage = "Invalid Ticket: Verification token is not recognized.",
                    Timestamp = executionTime
                };
            }

            var result = new VerificationResultDto
            {
                AttendeeName = pass.Attendee.FullName,
                EventTitle = pass.Event.Title,
                Timestamp = executionTime
            };

            try
            {
                // Action Logic Parsing: CHECK-IN
                if (scan.ActionType.Equals("CHECKIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (pass.Status == PassStatus.CheckedIn)
                    {
                        await SaveAuditLogAsync(pass.Id, "CHECKIN", scan.ScannedByDevice, false, "DUPLICATE_SCAN_ALERT: Immediate entry denial.");
                        result.IsSuccess = false;
                        result.StatusMessage = "Access Denied: Ticket is already checked in inside the venue.";
                        result.CurrentPassStatus = pass.Status.ToString();
                        return result;
                    }

                    if (pass.Status == PassStatus.CheckedOut && !pass.Event.IsReEntryAllowed)
                    {
                        await SaveAuditLogAsync(pass.Id, "CHECKIN", scan.ScannedByDevice, false, "FRAUD_ALERT: Checked-out ticket attempting re-entry on blocked rule.");
                        result.IsSuccess = false;
                        result.StatusMessage = "Access Denied: Event configuration does not authorize re-entry.";
                        result.CurrentPassStatus = pass.Status.ToString();
                        return result;
                    }

                    if (pass.Status == PassStatus.Revoked)
                    {
                        await SaveAuditLogAsync(pass.Id, "CHECKIN", scan.ScannedByDevice, false, "SECURITY_WARN: Access attempted using blacklisted ticket profile.");
                        result.IsSuccess = false;
                        result.StatusMessage = "Access Denied: Ticket profile has been revoked and flagged.";
                        result.CurrentPassStatus = pass.Status.ToString();
                        return result;
                    }

                    // Successful Check-In Progression path
                    pass.Status = PassStatus.CheckedIn;
                    pass.LastStatusUpdatedAt = executionTime;
                    await SaveAuditLogAsync(pass.Id, "CHECKIN", scan.ScannedByDevice, true, "Check-in completed successfully.");
                }
                // Action Logic Parsing: CHECK-OUT
                else if (scan.ActionType.Equals("CHECKOUT", StringComparison.OrdinalIgnoreCase))
                {
                    if (pass.Status != PassStatus.CheckedIn)
                    {
                        await SaveAuditLogAsync(pass.Id, "CHECKOUT", scan.ScannedByDevice, false, "INVALID_STATE: Checkout attempted from outside the venue.");
                        result.IsSuccess = false;
                        result.StatusMessage = "Operation Failed: Ticket holder must be verified inside before requesting checkout.";
                        result.CurrentPassStatus = pass.Status.ToString();
                        return result;
                    }

                    pass.Status = PassStatus.CheckedOut;
                    pass.LastStatusUpdatedAt = executionTime;
                    await SaveAuditLogAsync(pass.Id, "CHECKOUT", scan.ScannedByDevice, true, "Checkout recorded successfully. Eligible for controlled re-entry.");
                }

                // Update Context Transactionally 
                await _passRepository.SaveChangesAsync();

                result.IsSuccess = true;
                result.StatusMessage = "Transaction Authorized successfully.";
                result.CurrentPassStatus = pass.Status.ToString();
                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                // High-traffic safety catch: Triggers if identical ticket string hits two gate terminals at once
                return new VerificationResultDto
                {
                    IsSuccess = false,
                    StatusMessage = "Security Breach: Simultaneous processing conflict detected on this token.",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        private async Task SaveAuditLogAsync(Guid passId, string action, string device, bool success, string notes)
        {
            var log = new VerificationLog
            {
                EventPassId = passId,
                ActionType = action,
                ScannedByDevice = string.IsNullOrWhiteSpace(device) ? "Default_Gate" : device,
                IsSuccess = success,
                Notes = notes
            };
            await _passRepository.AddVerificationLogAsync(log);
            await _passRepository.SaveChangesAsync();
        }
    }
}