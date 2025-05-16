using Credit_Service.DTOs;

namespace Credit_Service.Interfaces
{




    public interface ICreditService
    {
        Task<CreditCreationResponse> CreateCreditAsync(int accountId, CreditCreationRequest request);
    }
    public interface ICreditStatusService
    {
        Task<CreditStatusResponse> GetCreditStatusAsync(int clientId , int accountID);
    }

    // Update ICreditSimulationService
    public interface ICreditSimulationService
    {
        
        Task<CreditCreationResponse> CreateCreditAsync(int accountId, CreditCreationRequest request);
    }

   

    
}
