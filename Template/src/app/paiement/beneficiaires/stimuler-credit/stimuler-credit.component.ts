import { Component } from '@angular/core';

@Component({
  selector: 'app-stimuler-credit',
  templateUrl: './stimuler-credit.component.html',
  styleUrls: ['./stimuler-credit.component.scss']
})
export class SimulateurCreditComponent {
  montant: number = 50000;  // Montant initial
  duree: number = 36;      // Durée du crédit en mois
  taux: number = 7;        // Taux d'intérêt annuel
  typeAmortissement: string = 'constant'; // Type d'amortissement

  result: any = null;  // Résultat du calcul
  details: any[] = []; // Détails de l'amortissement variable

  // Fonction pour calculer le crédit
  calculerCredit() {
    let tauxMensuel = (this.taux / 12) / 100;
    let nombreMensualites = this.duree;

    if (this.typeAmortissement === 'constant') {
      // Mensualité fixe (formule d’amortissement)
      let mensualite = this.montant * tauxMensuel / (1 - Math.pow(1 + tauxMensuel, -nombreMensualites));
      this.result = { type: 'Mensualité constante', mensualite: mensualite.toFixed(2) };
    } else if (this.typeAmortissement === 'variable') {
      // Mensualité variable (capital fixe + intérêts décroissants)
      let capitalFixe = this.montant / nombreMensualites;
      let capitalRestant = this.montant;
      this.details = [];

      for (let i = 1; i <= nombreMensualites; i++) {
        let interet = capitalRestant * tauxMensuel;
        let mensualite = capitalFixe + interet;
        this.details.push({
          mois: i,
          mensualite: mensualite.toFixed(2),
          interet: interet.toFixed(2),
          capitalRestant: capitalRestant.toFixed(2)
        });
        capitalRestant -= capitalFixe;
      }
      this.result = { type: 'Amortissement variable', details: this.details };
    }
  }
}
