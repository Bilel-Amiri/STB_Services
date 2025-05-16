using System.Linq.Expressions;
using RestSharp;
using static Reclamation_Service.Services.SendMail;

namespace Reclamation_Service.Services
{
    public class SendMail
    {
        public class bodyMail{
            public string to { get; set; }
            public string subject { get; set; }
            public string content { get; set; }
        }
        public async Task<string> SendMailAsync(bodyMail bm)
        {
            try {
                var options = new RestClientOptions("https://openbank.stb.com.tn")
                {
                    MaxTimeout = -1,
                };

                var client = new RestClient(options);
                var request = new RestRequest("/api/students/subscription/sendmail", Method.Post);

                request.AddHeader("Ocp-Apim-Subscription-Key", "55c87b41825244d7b0299f66e3bda7f6");
                request.AddHeader("Content-Type", "application/json");

                var payload = new
                {
                    from = "mokhtar.hammami@stb.com.tn",
                    to = bm.to,
                    subject = bm.subject,
                    content = bm.content
                };

                request.AddJsonBody(payload);

                var response = await client.ExecuteAsync(request);
                Console.WriteLine(response.Content);
                return response.Content;

            }
        catch(Exception ex)
        {
                return ex.Message.ToString();
            }
        }
    }
}
