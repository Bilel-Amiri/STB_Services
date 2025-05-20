using System;
using System.Threading;
using System.Threading.Tasks;
using Credit_Service.Services;

namespace Credit_Service.Interfaces
{
    public interface IUserServiceClient
    {
        Task<AccountDto> GetAccountAsync(int accountId, CancellationToken cancellationToken = default);
        Task<AccountDto> GetAccountByRibAsync(long rib, CancellationToken cancellationToken = default);
        Task<UserInfoDto> GetUserInfoAsync(int clientId, CancellationToken cancellationToken = default);
        Task<UserInfoDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> UpdateBalanceAsync(UpdateBalanceRequest request, CancellationToken cancellationToken = default);
    Task<UserInfoDto> GetUserByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);

  }


}
