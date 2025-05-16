using Microsoft.AspNetCore.Mvc;
using Reclamation_Service.Models;
using Reclamation_Service.Services;
using Reclamation_Service.DTOS;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;
using System.Linq;
using static Reclamation_Service.Services.SendMail;
using Microsoft.AspNetCore.Authorization;

namespace Reclamation_Service.Controllers
{


   
    [ApiController]
    [Route("api/[controller]")]
    public class ReclamationsController : ControllerBase
    {
        private readonly IReclamationRepository _repository;
        private readonly IUserServiceHttpClient _userService;
        private readonly ILogger<ReclamationsController> _logger;

        public ReclamationsController(
            IReclamationRepository repository,
            IUserServiceHttpClient userService,
            ILogger<ReclamationsController> logger)
        {
            _repository = repository;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReclamationDto dto)
        {
            try
            {
                // 1. Create the reclamation
                var reclamation = await _repository.CreateReclamationAsync(dto, HttpContext.RequestAborted);
                if (reclamation == null)
                {
                    _logger.LogError("Failed to create reclamation for account {AccountId}", dto.AccountId);
                    return BadRequest("Failed to create reclamation");
                }






                return CreatedAtAction(
                    nameof(GetById),
                    new { id = reclamation.ReclamationId },
                    new ReclamationResponseDto
                    {
                        ReclamationId = reclamation.ReclamationId,
                        AccountId = reclamation.AccountId,
                        Subject = reclamation.Subject,
                        Description = reclamation.Description,
                        Status = reclamation.Status,
                        CreatedAt = reclamation.CreatedAt == default(DateTime) ? DateTime.UtcNow : reclamation.CreatedAt,
                        AssignedAdminId = reclamation.AssignedAdminId 

                    }) ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reclamation");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reclamation = await _repository.GetByIdAsync(id, HttpContext.RequestAborted);
            return reclamation != null
                ? Ok(MapToDto(reclamation))
                : NotFound();
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            var reclamations = await _repository.GetByAccountAsync(accountId, HttpContext.RequestAborted);
            return Ok(reclamations.Select(MapToDto));
        }

        private ReclamationResponseDto MapToDto(Reclamation reclamation)
        {   
            return new ReclamationResponseDto
            {
                ReclamationId = reclamation.ReclamationId,
                AccountId = reclamation.AccountId,
                Subject = reclamation.Subject,
                Description = reclamation.Description,
                Status = reclamation.Status,
                CreatedAt = reclamation.ReclamationDate ?? DateTime.UtcNow,
                AssignedAdminId = reclamation.AssignedAdminId
            };
        }

        [HttpPut("{reclamationId}/assign")]
        public async Task<IActionResult> UpdateAssignment(
    int reclamationId,
    [FromBody] AssignReclamationDto request)
        {
            var reclamation = await _repository.GetByIdAsync(reclamationId, CancellationToken.None);
            if (reclamation == null)
                return NotFound();

            reclamation.Status = ReclamationStatus.InProgress;
            reclamation.AssignedAdminId = request.AdminId;
            await _repository.UpdateAsync(reclamation, CancellationToken.None);

            return Ok();
        }



        [HttpPost("respond/{reclamationId}")]
        public async Task<IActionResult> RespondToReclamation(
    int reclamationId,
    [FromBody] RespondToReclamationRequest request)
        {
           
            var reclamation = await _repository.GetByIdAsync(reclamationId,CancellationToken.None);
           

           
            reclamation.Status = request.IsResolved ? "Resolved" : "Closed";
            
            reclamation.AssignmentDate = DateTime.UtcNow;

            await _repository.UpdateAsync(reclamation, CancellationToken.None);

           


            SendMail sm = new SendMail();
            bodyMail bm = new bodyMail();
            var userInfo = await _userService.GetUserInfoByAccountIdAsync(reclamation.AccountId);
            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Client email not found. Cannot send email.");
                return BadRequest("Email introuvable pour ce client.");
            }

            bm.to = userInfo.Email;
            bm.subject = "Mise à jour concernant votre réclamation";
            bm.content = "Bonjour , \n\nNous tenons à vous informer que le problème que vous avez signalé dans votre réclamation a été résolu avec succès.\n\n ";


            string response = await sm.SendMailAsync(bm);
            if (response.Contains("success"))
            {

            }

            return Ok(new
            {
                reclamation.ReclamationId,
                reclamation.Status
            });
        }


        [HttpGet("history/{AccountId}")]
        public async Task<IActionResult> GetReclamationHistory(int AccountId)
        {
            var reclamations = await _repository.GetByAccountAsync( AccountId,CancellationToken.None  );

            return Ok(reclamations.Select(r => new ReclamationHistoryDto(
                r.ReclamationId,
                r.Subject,
                r.Status,
                r.ReclamationDate ?? DateTime.MinValue,

                r.AssignmentDate
                
            )));
        }


        [HttpOptions("api/reclamations")]
        public IActionResult Options()
        {
            return Ok();
        }





    }
    public record RespondToReclamationRequest(
            [Required] bool IsResolved,  
            [Required] string Message
            
        );
    public static class ReclamationStatus
    {
        public const string Open = "Open";
        public const string InProgress = "In Progress";
        public const string Resolved = "Resolved";
        public const string Closed = "Closed";
    }
    public record ReclamationHistoryDto(
    int Id,
    string Subject,
    string Status,  // "Open", "Assigned", "Resolved", "Closed"
    DateTime CreatedAt,
    DateTime? ResolvedOrClosedAt
    );

}