using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Card_Service.Services;

namespace Card_Service.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardsController> _logger;
        private readonly IUserServiceClient _userServiceClient;


        public CardsController(
            ICardService cardService,
             IUserServiceClient userServiceClient,
            ILogger<CardsController> logger)
        {
            _cardService = cardService;
            _logger = logger;
            _userServiceClient = userServiceClient;
        }




        private int GetAccountIdFromToken()
        {
            var accountIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
            {
                _logger.LogWarning("AccountId claim is missing in token.");
                throw new UnauthorizedAccessException("No AccountId found in token.");
            }

            if (accountIdClaim == "0")
            {
                _logger.LogWarning("AccountId is 0 (invalid).");
                throw new UnauthorizedAccessException("Invalid AccountId in token.");
            }

            if (!int.TryParse(accountIdClaim, out int accountId))
            {
                _logger.LogWarning($"Failed to parse AccountId: {accountIdClaim}");
                throw new InvalidOperationException("AccountId must be a number.");
            }

            return accountId;
        }





        [HttpGet]
        public async Task<IActionResult> GetUserCards()
        {
            try
            {
                // Get AccountId from token instead of ClientId
                var accountId = GetAccountIdFromToken(); // You'll need to implement this
                var cards = await _cardService.GetUserCardsAsync(accountId);
                return Ok(cards);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Account not found");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user cards");
                return StatusCode(500);
            }
        }

        [HttpGet("{cardId}")]
        public async Task<IActionResult> GetCardDetails(int cardId)
        {
            try
            {
                var accountId = GetAccountIdFromToken(); // From JWT claim
                var cardDetails = await _cardService.GetCardDetailsAsync(cardId, accountId);
                return Ok(cardDetails);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting card details");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{cardId}/block")]
        public async Task<IActionResult> BlockCard(
     int cardId,
     [FromBody] BlockCardReasonRequest request) // Combined DTO
        {
            try
            {
                var accountId = GetUserIdFromToken(); // Get from token, not from request
                var result = await _cardService.BlockCardAsync(
                    cardId,
                    accountId, // From token
                    request.Reason);

                return result ? Ok() : BadRequest("Unable to block card");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking card");
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpPost("{cardId}/deblock")]
        public async Task<IActionResult> DeblockCard(int cardId)
        {
            try
            {
                var clientId = GetUserIdFromToken();
                var result = await _cardService.DeblockCardAsync(cardId, clientId);

                return result ? Ok() : BadRequest("Unable to deblock card");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deblocking card");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestNewCard([FromBody] NewCardRequest request)
        {
            try
            {
                // Get CLIENT ID from token, not account ID
                var clientId = GetUserIdFromToken(); // Renamed for clarity

                // Verify the requested account belongs to this client
                var account = await _userServiceClient.GetAccountByIdAsync(request.AccountId);
                if (account == null || account.ClientId != clientId)
                {
                    _logger.LogWarning($"Client {clientId} doesn't own account {request.AccountId}");
                    return BadRequest("Invalid account specified");
                }

                var result = await _cardService.RequestNewCardAsync(request.AccountId, request);
                return result ? Ok() : BadRequest("Unable to request card");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Card request failed");
                return StatusCode(500);
            }
        }




        [HttpGet("types")]
        public async Task<IActionResult> GetSupportedCardTypes()
        {
            try
            {
                var types = await _cardService.GetSupportedCardTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported card types");
                return StatusCode(500, "Internal server error");
            }
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }





      




    }
}