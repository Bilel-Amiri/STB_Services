using System.ComponentModel.DataAnnotations;

namespace Transaction_Service.Model
{
    public class TransactionRequest
    {
        public int account_id { get; set; }
        public int source_account_id { get; set; }
        public long destination_rib { get; set; }
        public decimal amount { get; set; }
        public string?      transaction_type { get; set; }       // Not nullable if required
       
        public string destination_email { get; set; }
        public string Motif { get; set; }

    }

}

      
