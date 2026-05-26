using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConclaseProject.Controllers
{
    [Authorize]//locked every end point inside this entire controller
    [Route("api/[controller]")]
    [ApiController]
    public class GateController : ControllerBase
    {
        private readonly VerificationService _verificationService;

        public GateController(VerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> ProcessGateScan([FromBody] ScanRequestDto request)
        {
            var outcome = await _verificationService.ExecuteGateValidationAsync(request);

            // To ensure smooth user flows on mobile clients, we return 200 OK containing 
            // the validation state so the client application can instantly display a green or red UI card.
            return Ok(outcome);
        }
    }
}