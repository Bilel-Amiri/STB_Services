namespace Credit_Service.DTOs
{
    public class RateConfiguration
    {
        public decimal BaseRate { get; set; } = 4.5m;
        public decimal MinRate { get; set; } = 3.0m;
        public decimal MaxRate { get; set; } = 15.0m;
    }

    
    public interface IRiskAssessmentService
    {
        Task<int> GetCreditScoreAsync(int clientId);
    }

    // RiskAssessmentService.cs (temporary implementation)
    public class RiskAssessmentService : IRiskAssessmentService
    {
        public Task<int> GetCreditScoreAsync(int clientId)
        {
            // TODO: Implement actual risk assessment
            return Task.FromResult(700); // Default score
        }
    }
}
