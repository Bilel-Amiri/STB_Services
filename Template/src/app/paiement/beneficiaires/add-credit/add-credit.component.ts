import { Component, OnInit } from '@angular/core';
import { CreditService, DemandeCreditInputModel } from '../credit.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../../user/user.service'; 

@Component({
  selector: 'app-add-credit',
  templateUrl: './add-credit.component.html',
  styleUrls: ['./add-credit.component.scss']
})
export class AddCreditComponent implements OnInit {
  creditForm: FormGroup;
  accountId: number | null = null;
  user: any; 

  constructor(
    private fb: FormBuilder,  
    private creditService: CreditService,
    private userService: UserService
  ) {
    this.creditForm = this.fb.group({
      creditAmount: [null, [Validators.required]],
      durationMonths: [null, [Validators.required]],
      creditType: ['', [Validators.required]],
      amortizationType: ['', [Validators.required]],
      cin: [''],
      maritalStatus: ['']
    });

    this.accountId = this.userService.getAccountIdFromToken(); 
  }

  onSubmit() {
    if (this.creditForm.valid && this.accountId != null) {
      const input: DemandeCreditInputModel = this.creditForm.value;

      this.creditService.demandeCredit(this.accountId, input).subscribe({
        next: (res) => {
          console.log('Crédit demandé avec succès', res);
          alert('Demande de crédit envoyée avec succès !');
        },
        error: (err) => {
          console.error('Erreur lors de la demande de crédit', err);
          alert('Erreur lors de l\'envoi de la demande');
        }
      });
    } else {
      alert('Formulaire invalide ou identifiant de compte manquant.');
    }
  }

  ngOnInit(): void {
    const accountId = this.userService.getAccountIdFromToken();

    this.userService.getUserByAccountId(accountId).subscribe({
      next: (res) => {
        this.user = res;
        console.log('User loaded:', this.user);
      },
      error: (err) => {
        console.error('Error loading user info:', err);
      }
    });
  }

  
}
