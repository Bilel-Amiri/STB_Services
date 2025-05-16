namespace Transaction_Service.Services
{
    public interface IUserServiceClient
    {
        Task<AccountDto?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<AccountDto?> GetAccountByRibAsync(long rib, CancellationToken cancellationToken = default);
        Task<bool> UpdateBalanceAsync(UpdateBalanceRequest request, CancellationToken cancellationToken = default);
        Task<UserInfoDto?> GetUserInfoAsync(int clientId, CancellationToken cancellationToken = default);
        Task<UserInfoDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    }
}
