using System.ComponentModel.DataAnnotations;

namespace Reclamation_Service.DTOS
{
    // AssignReclamationDto.cs
    public class AssignReclamationDto
    {
        [Required]
        public int AdminId { get; set; }
    }

    // UpdateReclamationStatusDto.cs
    public class UpdateReclamationStatusDto
    {
        [Required]
        public string Status { get; set; }
        public string AdminNotes { get; set; }
    }


    public class ReclamationResponseDto
    {
        public int ReclamationId { get; set; }
        public int AccountId { get; set; }
        public string Subject { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int? AssignedAdminId { get; set; }
    }

    public class CreateReclamationDto
    {
        public int? ClientId { get; set; }
        public int AccountId { get; set; }
        public string Subject { get; set; } = null!;
        public string? Description { get; set; }
        public string? Email { get; set; } = null!;
    }




    public class AdminInfoDto
    {
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

   

}



