using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Credit_Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Credit_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly ICreditStatusService _creditStatusService;
        private readonly ICreditSimulationService _creditSimulationService;
        private readonly ICreditService _creditService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreditsController> _logger;

        public CreditsController(
            ICreditStatusService creditStatusService,
            ICreditSimulationService creditSimulationService,
            ICreditService creditService,
            INotificationService notificationService,
            ILogger<CreditsController> logger)
        {
            _creditStatusService = creditStatusService;
            _creditSimulationService = creditSimulationService;
            _creditService = creditService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetCreditStatus(
    [FromQuery] int clientId,
    [FromQuery] int accountID)
        {
            try
            {
                if (clientId <= 0 || accountID <= 0)
                    return BadRequest("Both clientId and accountID must be positive numbers");

                var result = await _creditStatusService.GetCreditStatusAsync(clientId, accountID);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("simulate")]
        public IActionResult Simulate([FromBody] CreditRequest request)
        {
            var result = simulateur.SimulateurCredit(
                request.MontantCredit,
                request.Duree,
                request.TauxAnnuel,
                request.TypeAmortissement.ToLower()
            );

            return Ok(result);
        }



        [HttpPost]
        public async Task<IActionResult> CreateCredit(
              [FromQuery] int accountId,  
              [FromBody] CreditCreationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _creditService.CreateCreditAsync(accountId, request);
                return CreatedAtAction(
                    actionName: nameof(GetCreditStatus),
                    routeValues: new { clientId = accountId },
                    value: response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credit for account {AccountId}", accountId);
                return StatusCode(500, "An error occurred while creating the credit");
            }
        }

        [HttpPost("notifications/check")]
        public async Task<IActionResult> CheckAndSendNotifications()
        {
            try
            {
                _logger.LogInformation("Manual notification check triggered");

                // Changed from void to return a result
                var result = await _notificationService.CheckDueDatesAndNotifyAsync();

                return Accepted(new NotificationCheckResponse
                {
                    Status = "Processing completed",
                    NotificationsCreated = result.NotificationsCreated,
                    UpcomingCount = result.UpcomingCount,
                    OverdueCount = result.OverdueCount,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual notification check");
                return StatusCode(500, new ErrorResponse
                {
                    Error = "Internal server error",
                    Details = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }




        [HttpGet("notifications/upcoming")]
        public async Task<IActionResult> GetUpcomingNotifications(
    [FromQuery] int clientId,
    [FromQuery] int daysAhead = 7)
        {
            try
            {
                if (clientId <= 0) return BadRequest("Client ID must be positive");
                if (daysAhead <= 0) return BadRequest("Days ahead must be positive");

                var notifications = await _notificationService.GetUpcomingNotificationsAsync(clientId, daysAhead);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming notifications for client {ClientId}", clientId);
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost("notifications/resend/{creditId}")]
        public async Task<IActionResult> ResendNotification(int creditId)
        {
            try
            {
                var success = await _notificationService.ResendNotificationAsync(creditId);
                return success ? Ok("Notification resent") : NotFound("Credit not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending notification for credit {CreditId}", creditId);
                return StatusCode(500, "Internal server error");
            }
        }




    }
}