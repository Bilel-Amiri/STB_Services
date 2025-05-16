namespace Credit_Service.Services
{
    public class simulateur { 
    public static object SimulateurCredit(double montantCredit, int duree, double tauxAnnuel, string typeAmortissement)
    {
        double tauxMensuel = (tauxAnnuel / 12) / 100;
        int nombreMensualites = duree;

        if (typeAmortissement == "constant")
        {
            double mensualite = montantCredit * tauxMensuel / (1 - Math.Pow(1 + tauxMensuel, -nombreMensualites));
            return new { Type = "Mensualité constante", Mensualite = Math.Round(mensualite, 2) };
        }
        else if (typeAmortissement == "variable")
        {
            List<object> resultat = new List<object>();
            double capitalFixe = montantCredit / nombreMensualites;
            double capitalRestant = montantCredit;

            for (int i = 0; i < nombreMensualites; i++)
            {
                double interet = capitalRestant * tauxMensuel;
                double mensualite = capitalFixe + interet;
                resultat.Add(new
                {
                    Mois = i + 1,
                    Mensualite = Math.Round(mensualite, 2),
                    Interet = Math.Round(interet, 2),
                    CapitalRestant = Math.Round(capitalRestant, 2)
                });
                capitalRestant -= capitalFixe;
            }

            return new { Type = "Amortissement variable", Details = resultat };
        }
        else
        {
            return new { Error = "Type d'amortissement non reconnu." };
        }
    }
}
}

public class CreditRequest
{
    public double MontantCredit { get; set; }
    public int Duree { get; set; } // in months
    public double TauxAnnuel { get; set; }
    public string TypeAmortissement { get; set; } // "constant" or "variable"
}
