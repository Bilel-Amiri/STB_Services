using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Credit_Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Credit_Service.Services.SendMail;
using Credit_Service.Models;

namespace Credit_Service.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CreditsController : ControllerBase
  {
    private readonly IUserServiceClient _userServiceClient;
    private readonly Demande_Credit _demandeCredit;

    private readonly ILogger<CreditsController> _logger;

    public CreditsController(

        ILogger<CreditsController> logger, Demande_Credit demande_Credit, IUserServiceClient userServiceClient)
    {
      _demandeCredit = demande_Credit;

      _logger = logger;
      _userServiceClient = userServiceClient;

    }



    [HttpPost("simulate")]
    public IActionResult Simulate([FromBody] CreditRequest request)
    {
      var result = simulateur.SimulateurCredit(
          request.MontantCredit,
          request.Duree,
          request.TauxAnnuel,
          request.TypeAmortissement.ToLower()
      );

      return Ok(result);
    }




    [HttpPost("demande-credit/{accountId}")]
    public async Task<IActionResult> DemandeCredit(
       int accountId,
       [FromBody] DemandeCreditInputModel input,
       CancellationToken cancellationToken)
    {
      try
      {
        var creditRequest = await _demandeCredit.DemandeCreditAsync(
            accountId,
            input.CreditAmount,
            input.DurationMonths,
            input.CreditType,
            input.AmortizationType,
            input.Cin,
            input.MaritalStatus,
            cancellationToken
        );

        SendMail sm = new SendMail();
        bodyMail bm = new bodyMail();
        var userInfo = await _userServiceClient.GetUserByAccountIdAsync(accountId);
        if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
        {
          _logger.LogWarning("Client email not found. Cannot send email.");
          return BadRequest("Email introuvable pour ce client.");
        }

        bm.to = userInfo.Email;
        bm.subject = "Confirmation de réception de votre demande de crédit";
        bm.content = "Bonjour,\n\nNous vous confirmons la réception de votre demande de crédit. Notre équipe va l'étudier attentivement et vous tiendra informé(e) de la suite donnée dans les plus brefs délais.";


        string response = await sm.SendMailAsync(bm);
        if (response.Contains("success"))
        {

        }


        return Ok(creditRequest);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }








    }


  }

  public class DemandeCreditInputModel
  {
    public decimal CreditAmount { get; set; }
    public int DurationMonths { get; set; }
    public string CreditType { get; set; }
    public string AmortizationType { get; set; }
    public string? Cin { get; set; }
    public string? MaritalStatus { get; set; }
  }


}

