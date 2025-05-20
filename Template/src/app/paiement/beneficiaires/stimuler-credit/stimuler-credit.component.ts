import { Component } from '@angular/core';

@Component({
  selector: 'app-stimuler-credit',
  templateUrl: './stimuler-credit.component.html',
  styleUrls: ['./stimuler-credit.component.scss']
})
export class SimulateurCreditComponent {
  montant: number = 50000;  
  duree: number = 36;      
  taux: number = 7;        
  typeAmortissement: string = 'constant'; 

  result: any = null;  
  details: any[] = []; 

  
  calculerCredit() {
    let tauxMensuel = (this.taux / 12) / 100;
    let nombreMensualites = this.duree;

    if (this.typeAmortissement === 'constant') {
      
      let mensualite = this.montant * tauxMensuel / (1 - Math.pow(1 + tauxMensuel, -nombreMensualites));
      this.result = { type: 'Mensualité constante', mensualite: mensualite.toFixed(2) };
    } else if (this.typeAmortissement === 'variable') {
     
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
