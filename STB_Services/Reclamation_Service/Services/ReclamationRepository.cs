using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reclamation_Service.DTOS;
using Reclamation_Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reclamation_Service.Services
{
    public class ReclamationRepository : IReclamationRepository
    {
        private readonly ReclamationDbContext _context;
        private readonly ILogger<ReclamationRepository> _logger;

        public ReclamationRepository(
            ReclamationDbContext context,
            ILogger<ReclamationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReclamationResponseDto> CreateReclamationAsync(CreateReclamationDto dto, CancellationToken cancellationToken)
        {
            var reclamation = new Reclamation
            {
                AccountId = dto.AccountId,
                Subject = dto.Subject,
                Description = dto.Description,
                Status = "Open",
                ReclamationDate = DateTime.UtcNow
            };

            _context.Reclamations.Add(reclamation);
            await _context.SaveChangesAsync(cancellationToken);

            return new ReclamationResponseDto
            {
                ReclamationId = reclamation.ReclamationId,
                AccountId = reclamation.AccountId,
                Subject = reclamation.Subject,
                Description = reclamation.Description,
                Status = reclamation.Status,
                CreatedAt = reclamation.ReclamationDate ?? DateTime.UtcNow,
               
            };
        }




        public async Task<Reclamation?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _context.Reclamations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReclamationId == id, ct);
        }

        public async Task<List<Reclamation>> GetByAccountAsync(int accountId, CancellationToken ct)
        {
            return await _context.Reclamations
                .AsNoTracking()
                .Where(r => r.AccountId == accountId)
                .OrderByDescending(r => r.ReclamationDate)
                .ToListAsync(ct);
        }


        public async Task UpdateAsync(Reclamation reclamation, CancellationToken ct)
        {
            _context.Reclamations.Update(reclamation);
            await _context.SaveChangesAsync(ct);
        }



    }
}

