using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Transaction_Service.Model;
using Transaction_Service.Services;

[ApiController]
[Route("api/transactions")]
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;
    private readonly ILogger<TransactionController> _logger;
    private readonly IUserServiceClient _userServiceClient;

    public TransactionController(TransactionService transactionService,
                               ILogger<TransactionController> logger, IUserServiceClient userServiceClient)
    {
        _transactionService = transactionService;
        _logger = logger;
        _userServiceClient = userServiceClient;
    }

   

    // Generic transaction endpoint
    [HttpPost]
    public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequest request)
    {
        return await _transactionService.ProcessTransaction(request);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory()
    {
        try
        {

           

            // First await the task to get the List<Transaction>
            var allTransactions = await _transactionService.GetTransactionHistory();

            // Then apply the filter
            var virements = allTransactions
                .Where(t => t.TransactionType == "virements")
                .ToList();

            return Ok(virements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get transaction history");
            return StatusCode(500, new { Error = ex.Message });
        }
    }


    [HttpGet("user-by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        _logger.LogInformation($"Attempting to lookup email: {email}");
        try
        {
            var userInfo = await _userServiceClient.GetUserByEmailAsync(email);
            _logger.LogInformation($"Service returned: {userInfo != null}");
            return userInfo == null ? NotFound() : Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lookup failed");
            return StatusCode(500);
        }
    }





    [HttpPost("virements/initiate")]
    public async Task<IActionResult> InitiateVirement(TransactionRequest request)
    {

    


        return await _transactionService.InitiateVirement(request);
    }




    [HttpPost("virements/validate/{tempTransactionId}")]
    public async Task<IActionResult> ValidateVirement(Guid tempTransactionId)
    {
        return await _transactionService.ValidateVirement(tempTransactionId);
    }



    [HttpGet("virements/pending/{accountId}")]
    public async Task<IActionResult> GetPendingVirements(int accountId)
    {
        return await _transactionService.GetPendingVirements(accountId);
    }




}

