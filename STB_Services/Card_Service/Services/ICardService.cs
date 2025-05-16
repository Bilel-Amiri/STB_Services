namespace Card_Service.Services
{
    public interface ICardService
    {
        Task<IEnumerable<CardDto>> GetUserCardsAsync(int accountId);
        Task<CardDetailsDto> GetCardDetailsAsync(int cardId, int userId);
        Task<bool> BlockCardAsync(int cardId, int clientId, string reason);

        Task<bool> RequestNewCardAsync(int userId, NewCardRequest request);
        Task<IEnumerable<string>> GetSupportedCardTypesAsync();
        Task<bool> DeblockCardAsync(int cardId, int clientId);
    }
}

public record NewCardRequest(
    int AccountId,
    string CardType,
    DateOnly ExpirationDate);

public class CardDto
{
    public int CardId { get; set; }
    public string CardType { get; set; }
    public string MaskedNumber { get; set; }
    public string Status { get; set; }
    public DateOnly ExpirationDate { get; set; }
}

public class CardDetailsDto : CardDto
{
    public int AccountId { get; set; }
    public string AccountOwnerName { get; set; }
}

public class BlockCardReasonRequest
{
    public string Reason { get; set; } = null!;
}
