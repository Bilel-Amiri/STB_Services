using System.Text.Json;
using Card_Service.Models;
using Microsoft.EntityFrameworkCore;
using Card_Service.Services;

namespace Card_Service.Services
{
    public class CardService : ICardService
    {
        private readonly CardDBcontext _context;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ILogger<CardService> _logger;

        public CardService(
            CardDBcontext context,
            IUserServiceClient userServiceClient,
            ILogger<CardService> logger)
        {
            _context = context;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CardDto>> GetUserCardsAsync(int accountId)
        {
            _logger.LogInformation($"Fetching account {accountId} from UserService...");

            var account = await _userServiceClient.GetAccountByIdAsync(accountId);

            if (account == null)
            {
                _logger.LogError($"Account {accountId} not found in UserService.");
                throw new KeyNotFoundException($"Account {accountId} not found");
            }

            _logger.LogInformation($"Account found: {JsonSerializer.Serialize(account)}");


            // 1. Verify account exists
            account = await _userServiceClient.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account {accountId} not found");
            }

            // 2. Get cards for this account
            return await _context.Cards
         .Where(c => c.AccountId == accountId)
         .Select(c => new CardDto
         {
             CardId = c.CardId,
             CardType = c.CardType,
             MaskedNumber = CardService.MaskCardNumber(c.CardNumber), // Static call
             Status = c.Status,
             ExpirationDate = c.ExpirationDate
         })
         .ToListAsync();
        }

        public async Task<CardDetailsDto> GetCardDetailsAsync(int cardId, int accountId)
        {
            using var _ = _logger.BeginScope(new { CardId = cardId, AccountId = accountId });
            _logger.LogInformation("Fetching card details");

            try
            {
                // 1. Fetch card and verify ownership in single query
                var card = await _context.Cards
                    .Where(c => c.CardId == cardId && c.AccountId == accountId)
                    .Select(c => new {
                        c.CardId,
                        c.CardType,
                        c.CardNumber,
                        c.ExpirationDate,
                        c.Status,
                        c.AccountId
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Card not found or access denied");

                // 2. Get account owner details
                var account = await _userServiceClient.GetAccountByIdAsync(accountId)
                    ?? throw new KeyNotFoundException($"Account {accountId} not found");

                var userInfo = await _userServiceClient.GetUserInfoAsync(account.ClientId)
                    ?? throw new KeyNotFoundException($"User info for client {account.ClientId} not found");

                // 3. Map to DTO
                return new CardDetailsDto
                {
                    CardId = card.CardId,
                    CardType = card.CardType,
                    MaskedNumber = MaskCardNumber(card.CardNumber),
                    ExpirationDate = card.ExpirationDate,
                    Status = card.Status,
                    AccountId = card.AccountId,
                    AccountOwnerName = $"{userInfo.FirstName} {userInfo.LastName}".Trim()
                };
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch card details");
                throw;
            }
        }

        public async Task<bool> BlockCardAsync(int cardId, int clientId, string reason)
        {
            // 1. Get the card first to find its account
            var card = await _context.Cards
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CardId == cardId);

            if (card == null)
            {
                _logger.LogWarning($"Card {cardId} not found");
                return false;
            }

            // 2. Verify ownership through account
            var account = await _userServiceClient.GetAccountByIdAsync(card.AccountId);
            if (account == null || account.ClientId != clientId)
            {
                _logger.LogWarning($"User {clientId} lacks ownership of card {cardId}");
                return false;
            }

            // 3. Check if already blocked
            if (card.Status == "Blocked")
            {
                _logger.LogWarning($"Card {cardId} is already blocked");
                return false;
            }

            // 4. Update status
            card.Status = "Blocked";
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Card {cardId} blocked by {clientId}. Reason: {reason}");
            return true;
        }



        public async Task<bool> DeblockCardAsync(int cardId, int clientId)
        {
            // 1. Get the card with account verification
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.CardId == cardId);

            if (card == null)
            {
                _logger.LogWarning($"Card {cardId} not found");
                return false;
            }

            // 2. Verify ownership through account
            var account = await _userServiceClient.GetAccountByIdAsync(card.AccountId);
            if (account == null || account.ClientId != clientId)
            {
                _logger.LogWarning($"User {clientId} lacks ownership of card {cardId}");
                return false;
            }

            // 3. Check if already active
            if (card.Status != "Blocked")
            {
                _logger.LogWarning($"Card {cardId} is not blocked (current status: {card.Status})");
                return false;
            }

            // 4. Update status
            card.Status = "Active"; // Or your preferred active status
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Card {cardId} deblocked by {clientId}");
            return true;
        }





        public async Task<bool> RequestNewCardAsync(int accountId, NewCardRequest request)
        {
            // No need to re-verify account here (done in controller)

            var newCard = new Card
            {
                AccountId = accountId,  // Use the parameter, not request.AccountId
                CardNumber = GenerateValidCardNumber(),
                CardType = request.CardType,
                ExpirationDate = request.ExpirationDate,
                Status = "Active"
            };

            await _context.Cards.AddAsync(newCard);
            return await _context.SaveChangesAsync() > 0;
        }




        public async Task<IEnumerable<string>> GetSupportedCardTypesAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "VISA CLASSIC INTERNATIONALE",
                "CIB",
                "VISA CLASSIC NATIONALE",
                "STB VISA PLATINUM BUSINESS INTERNATIONALE",
                "VISA PLATINUM BUSINESS NATIONALE",
                "STB TRAVEL",
                "C-Cash",
                "VISA ELECTRON NATIONALE",
                "TECHNOLOGIQUE INTERNATIONALE",
                "MASTERCARD GOLD INTERNATIONALE",
                "MASTERCARD GOLD NATIONALE",
                "C-Pay",
                "Epargne"
            });
        }

        private CardDto MapToCardDto(Card card)
        {
            return new CardDto
            {
                CardId = card.CardId,
                CardType = card.CardType,
                MaskedNumber = MaskCardNumber(card.CardNumber),
                Status = card.Status,
                ExpirationDate = card.ExpirationDate
            };
        }

        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            return cardNumber[..6] + new string('*', cardNumber.Length - 10) + cardNumber[^4..];
        }

        private string GenerateValidCardNumber()
        {
            var random = new Random();
            while (true)
            {
                var candidate = string.Concat(
                    Enumerable.Range(0, 16)
                        .Select(_ => random.Next(0, 10).ToString()));

                // Verify uniqueness
                if (!_context.Cards.Any(c => c.CardNumber == candidate))
                {
                    return candidate;
                }
            }
        }

        private bool IsValidCardType(string cardType)
        {
            var validTypes = GetSupportedCardTypesAsync().Result;
            return validTypes.Contains(cardType);
        }


       


    }
}