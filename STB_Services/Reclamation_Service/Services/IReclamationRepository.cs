using Reclamation_Service.DTOS;
using Reclamation_Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Reclamation_Service.Services.ReclamationRepository;

namespace Reclamation_Service.Services
{
    public interface IReclamationRepository
    {
        Task<ReclamationResponseDto> CreateReclamationAsync(CreateReclamationDto dto, CancellationToken cancellationToken);

   
        Task<Reclamation?> GetByIdAsync(int id, CancellationToken ct);
        Task<List<Reclamation>> GetByAccountAsync(int accountId, CancellationToken ct);
        Task UpdateAsync(Reclamation reclamation, CancellationToken ct);






    }

    public class AssignmentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ReclamationResponseDto Reclamation { get; set; }
    }
}
