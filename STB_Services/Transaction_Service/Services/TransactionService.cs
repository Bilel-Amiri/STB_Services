using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transaction_Service.Model;
using Transaction_Service.Services;

public class TransactionService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly TransactionDBContext _dbContext;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IUserServiceClient userServiceClient,
        TransactionDBContext dbContext,
        ILogger<TransactionService> logger)
    {
        _userServiceClient = userServiceClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> ProcessTransaction(TransactionRequest request)
    {
        try
        {
            if (request.amount <= 0)
                return new BadRequestObjectResult(new { Error = "Amount must be greater than zero." });

            if (string.IsNullOrEmpty(request.transaction_type))
                return new BadRequestObjectResult(new { Error = "Transaction type is required." });

            // Only process these two types
            switch (request.transaction_type.ToLower())
            {
                case "dépenses":
                    var depenseResult = await ProcessDepense(request);
                    if (!depenseResult.Success)
                        return new BadRequestObjectResult(new { Error = depenseResult.Message });

                    return new OkObjectResult(new
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = request.account_id,
                        Amount = request.amount,
                        TransactionType = request.transaction_type,
                        Status = depenseResult.Message
                    });

                case "dépôts":
                    var depotResult = await ProcessDepot(request);
                    if (!depotResult.Success)
                        return new BadRequestObjectResult(new { Error = depotResult.Message });

                    return new OkObjectResult(new
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = request.account_id,
                        Amount = request.amount,
                        TransactionType = request.transaction_type,
                        Status = depotResult.Message
                    });

                default:
                    return new BadRequestObjectResult(new
                    {
                        Error = "This endpoint only processes dépôts/dépenses. Use /virements for transfers."
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction processing failed");
            return new ObjectResult(new { Error = $"An error occurred: {ex.Message}" }) { StatusCode = 500 };
        }
    }


    private async Task<(bool Success, string Message)> ProcessDepense(TransactionRequest request)
    {
        try
        {
            // Validate account
            var account = await _userServiceClient.GetAccountByIdAsync(request.account_id);
            if (account == null)
                return (false, "Invalid account.");

            // Check balance
            if (account.Balance < request.amount)
                return (false, "Insufficient balance in the account.");

            // Process deduction
            if (!await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
            {
                AccountId = request.account_id,
                Amount = -request.amount
            }))
                return (false, "Failed to update account balance.");

            // Log transaction
            _dbContext.Transactions.Add(new Transaction
            {
                AccountId = request.account_id,
                Amount = request.amount,
                TransactionType = "dépenses",
                TransactionDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            return (true, "Dépense processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dépense processing failed");
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    private async Task<(bool Success, string Message)> ProcessDepot(TransactionRequest request)
    {
        try
        {
            // Validate account
            var account = await _userServiceClient.GetAccountByIdAsync(request.account_id);
            if (account == null)
                return (false, "Invalid account.");

            // Process addition
            if (!await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
            {
                AccountId = request.account_id,
                Amount = request.amount
            }))
                return (false, "Failed to update account balance.");

            // Log transaction
            _dbContext.Transactions.Add(new Transaction
            {
                AccountId = request.account_id,
                Amount = request.amount,
                TransactionType = "dépôts",
                TransactionDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            return (true, "Dépôt processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dépôt processing failed");
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<List<Transaction>> GetTransactionHistory()
    {
        return await _dbContext.Transactions.ToListAsync();
    }



    public async Task<List<Transaction>> GetVirementHistory(int accountId)
    {
        // Step 1: Get the account info from the Account microservice
        var account = await _userServiceClient.GetAccountByIdAsync(accountId);
        if (account == null || string.IsNullOrEmpty(account.Rib.ToString()))
        {
            return new List<Transaction>();
        }

        string rib = account.Rib.ToString(); ;

        // Step 2: Try parsing RIB to long
        if (!long.TryParse(rib, out var parsedRib))
        {
            return new List<Transaction>(); 
        }

       
        return await _dbContext.Transactions
            .Where(t => t.TransactionType == "virements" &&
                       (t.AccountId == accountId || t.TargetRib == parsedRib))
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }









    public async Task<IActionResult> InitiateVirement(TransactionRequest request)
    {
        try
        {


      



            // Input validation
            if (request.amount <= 0)
                return new BadRequestObjectResult(new { Error = "Amount must be greater than zero." });

           
            if (request.source_account_id <= 0)
                return new BadRequestObjectResult(new { Error = "Invalid source account ID format." });

            _logger.LogInformation($"Checking source account: {request.source_account_id}"); // Add logging
            var sourceAccount = await _userServiceClient.GetAccountByIdAsync(request.source_account_id);

            if (sourceAccount == null)
            {
                _logger.LogWarning($"Source account not found: {request.source_account_id}");
                return new BadRequestObjectResult(new
                {
                    Error = "Invalid source account.",
                    Details = $"Account {request.source_account_id} not found or inaccessible",
                    Solution = "Please verify the account number or contact support"
                });
            }

           

            // Validate destination account
            if (request.destination_rib == 0)
                return new BadRequestObjectResult(new { Error = "Destination RIB is required." });

            _logger.LogInformation($"Checking destination account: {request.destination_rib}");
            var destinationAccount = await _userServiceClient.GetAccountByRibAsync(request.destination_rib);

            if (destinationAccount == null)
                return new BadRequestObjectResult(new { Error = "Destination account with provided RIB not found." });

            // Validate destination email
            if (string.IsNullOrWhiteSpace(request.destination_email))
                return new BadRequestObjectResult(new { Error = "Destination email is required." });

            _logger.LogInformation($"Checking destination user: {request.destination_email}");
            var destinationUser = await _userServiceClient.GetUserByEmailAsync(request.destination_email);

            if (destinationUser == null)
                return new BadRequestObjectResult(new { Error = "Destination user with provided email not found." });

            if (destinationAccount.ClientId != destinationUser.ClientId)
                return new BadRequestObjectResult(new { Error = "Provided RIB and Email do not match the same account." });

            // Check balance
            if (sourceAccount.Balance < request.amount)
                return new BadRequestObjectResult(new
                {
                    Error = "Insufficient balance in the source account.",
                    CurrentBalance = sourceAccount.Balance,
                    RequiredAmount = request.amount
                });

            // Create temporary transaction
            var tempTransaction = new TransactionTemp
            {
                AccountId = request.source_account_id,
                TargetRib = request.destination_rib,
                TargetEmail = request.destination_email,
                Amount = request.amount,
                TransactionType = "virements",
                Motif = request.Motif, 
                Status = 0 
            };

            // Before saving
            _logger.LogInformation("Attempting to save temporary transaction to database");
            _logger.LogDebug($"Transaction details: {JsonSerializer.Serialize(tempTransaction)}");

            try
            {
                _dbContext.TransactionTemps.Add(tempTransaction);
                var result = await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"SaveChangesAsync affected {result} records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save transaction to database");
                throw; // Re-throw to maintain the 500 error response
            }




           

            return new OkObjectResult(new
            {
                TempTransactionId = tempTransaction.TempTransactionId,
                Message = "Virement initiated successfully. Waiting for validation.",
                Destination_Rib = request.destination_rib,
                Destination_Email = request.destination_email,
                Amount = request.amount,
                Motif = request.Motif
            }) ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Virement initiation failed for account {request?.source_account_id}");
            return new ObjectResult(new
            {
                Error = "An error occurred while processing your transfer",
                Reference = $"ErrorRef-{DateTime.UtcNow.Ticks}"
            })
            {
                StatusCode = 500
            };
        }
    }




    public async Task<IActionResult> ValidateVirement(Guid tempTransactionId)
    {
        try
        {
            // Get the pending transaction
            var tempTransaction = await _dbContext.TransactionTemps
                .FirstOrDefaultAsync(t => t.TempTransactionId == tempTransactionId && t.Status == 0);

            if (tempTransaction == null)
                return new BadRequestObjectResult(new { Error = "Pending transaction not found or already processed." });

            // Validate source account again (in case balance changed)
            var sourceAccount = await _userServiceClient.GetAccountByIdAsync(tempTransaction.AccountId);
            if (sourceAccount == null)
                return new BadRequestObjectResult(new { Error = "Source account no longer exists." });

            if (sourceAccount.Balance < tempTransaction.Amount)
                return new BadRequestObjectResult(new { Error = "Insufficient balance in the source account." });

            // Process deduction
            if (!await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
            {
                AccountId = tempTransaction.AccountId,
                Amount = -tempTransaction.Amount
            }))
                return new BadRequestObjectResult(new { Error = "Failed to deduct amount from source account." });

            // Process addition
            var destinationAccount = await _userServiceClient.GetAccountByRibAsync(tempTransaction.TargetRib);
            if (destinationAccount == null)
            {
                // Refund if destination account is no longer valid
                await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
                {
                    AccountId = tempTransaction.AccountId,
                    Amount = tempTransaction.Amount
                });
                return new BadRequestObjectResult(new { Error = "Destination account no longer exists." });
            }

            if (!await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
            {
                AccountId = destinationAccount.AccountId,
                Amount = tempTransaction.Amount
            }))
            {
                // Refund if addition fails
                await _userServiceClient.UpdateBalanceAsync(new UpdateBalanceRequest
                {
                    AccountId = tempTransaction.AccountId,
                    Amount = tempTransaction.Amount
                });
                return new BadRequestObjectResult(new { Error = "Failed to add amount to destination account." });
            }

            // Move to completed transactions
            _dbContext.Transactions.Add(new Transaction
            {
                AccountId = tempTransaction.AccountId,
                TargetRib = tempTransaction.TargetRib,
                TargetEmail = tempTransaction.TargetEmail,
                Amount = tempTransaction.Amount,
                TransactionType = tempTransaction.TransactionType,
                
                Motif = tempTransaction.Motif
            });

            // Update temp transaction status
            tempTransaction.Status = 1;
            tempTransaction.ValidationDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new OkObjectResult(new
            {
                TransactionId = Guid.NewGuid(),
                Message = "Virement validated and processed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virement validation failed");
            return new ObjectResult(new { Error = $"An error occurred: {ex.Message}" })
            {
                StatusCode = 500
            };
        }
    }




    public async Task<IActionResult> GetPendingVirements(int accountId)
    {
        try
        {
            var pendingTransactions = await _dbContext.TransactionTemps
                .Where(t => t.AccountId == accountId && t.Status == 0)
                .OrderBy(t => t.InitiationDate)
                .ToListAsync();

            return new OkObjectResult(pendingTransactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pending virements");
            return new ObjectResult(new { Error = $"An error occurred: {ex.Message}" })
            {
                StatusCode = 500
            };
        }
    }



}