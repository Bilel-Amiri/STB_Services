using Credit_Service.DTOs;
using Credit_Service.Models;
using Credit_Service.Services;
namespace Credit_Service.Interfaces;

public class Demande_Credit
{

  private readonly IUserServiceClient _userServiceClient;
  private readonly CreditDBContext _dbContext;


  public Demande_Credit(IUserServiceClient userServiceClient, CreditDBContext dbContext)
  {
    _userServiceClient = userServiceClient;
    _dbContext = dbContext;
  }
  public async Task<CreditRequestDto> DemandeCreditAsync(
     int accountId,
     decimal creditAmount,
     int durationMonths,
     string creditType,
     string amortizationType,
     string? cin = null,
     string? maritalStatus = null,
     CancellationToken cancellationToken = default)
  {
    // 1. Get account info
    AccountDto account = await _userServiceClient.GetAccountAsync(accountId, cancellationToken);
    if (account == null)
    {
      throw new Exception("Account not found.");
    }

    // 2. Get user info using ClientId from account
    UserInfoDto user = await _userServiceClient.GetUserInfoAsync(account.ClientId, cancellationToken);
    if (user == null)
    {
      throw new Exception("User not found.");
    }

    // 3. Create a new Credit entity to persist in the database
    var credit = new Credit
    {
      AccountId = accountId,
      CreditAmount = creditAmount,
      DurationMonths = durationMonths,
      CreditType = creditType,
      AmortizationType = amortizationType,
      Cin = cin,
      MaritalStatus = maritalStatus,  
      StartDate = DateOnly.FromDateTime(DateTime.Now),
      EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(durationMonths)),
      Status = "open"
    };

    _dbContext.Credits.Add(credit);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // 4. Build and return the DTO
    return new CreditRequestDto
    {
      Rib = account.Rib,
      CreditAmount = creditAmount,
      DurationMonths = durationMonths,
      CreditType = creditType,
      AmortizationType = amortizationType,
      Cin = cin,
      MaritalStatus = maritalStatus,
      Nom = user.LastName,
      Prenom = user.FirstName,
      Email = user.Email
    };
  }





}
