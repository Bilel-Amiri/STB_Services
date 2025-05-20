using System.ComponentModel.DataAnnotations;

namespace Credit_Service.DTOs
{
  public class CreditRequestDto
  {
    public long Rib { get; set; }
    public decimal CreditAmount { get; set; }
    public int DurationMonths { get; set; }
    public string CreditType { get; set; }
    public string AmortizationType { get; set; } 
    public string? Cin { get; set; }
    public string? MaritalStatus { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
  }
}


