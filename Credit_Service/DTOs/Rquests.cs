using System.ComponentModel.DataAnnotations;

namespace Credit_Service.DTOs
{
    // Database table columns: credit_id, account_id, credit_amount, interest_rate, 
    // duration_months, start_date, end_date, remaining_balance, status

    public record CreditStatusResponse(
        int accountID,                  // Maps to account_id
        string ClientName,             // From UserService
        decimal TotalAmount,           // Sum of credit_amount
        decimal TotalRemaining,        // Sum of remaining_balance
        DateOnly? UpcomingDueDate,     // Calculated from repayments
        List<CreditDetailDto> Credits
       );

    public record CreditDetailDto(
     [Required] int CreditId,
     string ContractNumber,
     [Required] decimal OriginalAmount,
     [Required] decimal RemainingBalance,
     [Required][Range(0, 100)] decimal InterestRate,
     [Required] DateOnly StartDate,
     [Required] DateOnly EndDate,
     DateOnly NextDueDate,
     [Required] string Status);

    public record CreditSimulationRequest(
        [Required][Range(100, 1000000)] decimal Amount,
        [Required][Range(1, 84)] int DurationMonths,
        [Required][Range(0.1, 25.0)] decimal InterestRate,
        [Required] int accountID,
        [Required] int ClientId);

    public record CreditSimulationResponse(
     bool approved,
     decimal amount,
     decimal monthlyPayment,
     decimal totalInterest,
     decimal totalRepayment,
     decimal apr,
     DateOnly firstPaymentDate,
     DateOnly finalPaymentDate,
     decimal interestRate);

    public record CreditCreationRequest(
      [Required][Range(100, 1000000)] decimal Amount,
      [Required][Range(1, 84)] int DurationMonths,
      [Required][Range(0.1, 25.0)] decimal AnnualInterestRate,
      [Required] DateOnly StartDate);


    public record CreditCreationResponse(
        int CreditId,                      
        string ContractNumber,            
        int ClientId,                     
        decimal Amount,                   
        decimal MonthlyPayment,           
        DateOnly StartDate,              
        DateOnly EndDate,                 
        string Status,                    
        List<DueDateAlert> PaymentSchedule); 

    public record CreditNotification(
        int ClientId,                    
        string ClientName,                
        string ClientEmail,               
        int CreditId,                     
        string ContractNumber,            
        DateOnly DueDate,                
        decimal DueAmount,                
        string NotificationType);        

    public record DueDateAlert(
        int InstallmentNumber,
        DateOnly DueDate,                 
        decimal Amount);                 
}

public record UpcomingNotificationDto(
    int CreditId,
    string ContractNumber,
    DateOnly DueDate,
    decimal AmountDue,
    bool IsOverdue,
    string ClientName);


public record NotificationCheckResponse
{
    public string Status { get; init; }
    public int NotificationsCreated { get; init; }
    public int UpcomingCount { get; init; }
    public int OverdueCount { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ErrorResponse
{
    public string Error { get; init; }
    public string Details { get; init; }
    public DateTime Timestamp { get; init; }
}
public record NotificationCheckResult
{
    public int NotificationsCreated { get; init; }
    public int UpcomingCount { get; init; }
    public int OverdueCount { get; init; }
}



