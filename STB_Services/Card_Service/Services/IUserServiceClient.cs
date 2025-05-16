namespace Card_Service.Services
{
    public interface IUserServiceClient
    {
        Task<AccountDto?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<UserInfoDto?> GetUserInfoAsync(int clientId, CancellationToken cancellationToken = default);
        Task<bool> ValidateCardOwnershipAsync(int clientId, int cardId, CancellationToken cancellationToken = default);
        Task<bool> HasAccountPermissionAsync(int clientId, int accountId, CancellationToken cancellationToken = default);
        Task<bool> UserExistsAsync(int ClientId);

    }
}
