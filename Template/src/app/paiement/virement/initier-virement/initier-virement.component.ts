import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VirementService } from '../../virement/virement.service';
import { jwtDecode } from 'jwt-decode';

@Component({
  selector: 'app-initier-virement',
  templateUrl: './initier-virement.component.html',
  styleUrls: ['./initier-virement.component.scss']
})
export class InitierVirementComponent {
  virementForm: FormGroup;
  isLoading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;
  formSubmitted = false;
  transactionDetails: any = null;

  constructor(
    private fb: FormBuilder,
    private virementService: VirementService
  ) {
    this.virementForm = this.fb.group({
      destinationRIB: ['', [
        Validators.required,
        Validators.pattern(/^\d+$/),
       
      ]],
      destinationEmail: ['', [
        Validators.required,
        Validators.email
      ]],
      amount: ['', [
        Validators.required,
        Validators.min(0.01),
        Validators.max(100000),
        Validators.pattern(/^\d+(\.\d{1,2})?$/)
      ]],
      motif: ['', [
        Validators.maxLength(255)
      ]]
    });
  }

  onSubmit(): void {
    this.formSubmitted = true;
    this.markFormGroupTouched(this.virementForm);
  
    if (this.virementForm.invalid) {
      this.errorMessage = 'Veuillez corriger les erreurs dans le formulaire';
      return;
    }
  
    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;
    this.transactionDetails = null;
  
    // Extract form values
    const virementData = {
      destinationRIB: this.virementForm.value.destinationRIB,
      destinationEmail: this.virementForm.value.destinationEmail,
      amount: this.virementForm.value.amount,
      motif: this.virementForm.value.motif // Keep lowercase here
    };
  
    this.virementService.initiateVirement(virementData).subscribe({
      next: (response) => {
        this.handleSuccessResponse(response);
      },
      error: (err) => {
        this.handleErrorResponse(err);
      }
    });
  }
  

  private handleSuccessResponse(response: any): void {
    this.isLoading = false;
    this.transactionDetails = {
      reference: response.tempTransactionId || 'N/A',
      amount: this.virementForm.value.amount,
      destinationRIB: this.virementForm.value.destinationRIB,
      destinationEmail: this.virementForm.value.destinationEmail,
      motif: response.motif,
      date: new Date().toLocaleString()
    };
    
    this.successMessage = this.generateSuccessMessage();
    this.virementForm.reset();
    this.formSubmitted = false;
  }

  private handleErrorResponse(error: any): void {
    this.isLoading = false;
    
    if (error.message.includes('Token does not contain account identification')) {
      this.errorMessage = 'Problème d\'authentification. Veuillez vous reconnecter.';
      console.error('Token missing account ID. Full error:', error);
      localStorage.removeItem('token');
    } 
    else if (error.message.includes('Invalid account ID format')) {
      this.errorMessage = 'Erreur système: format de compte invalide';
      console.error('Malformed account ID:', error);
    }
    else if (error.message.includes('RIB must be numeric')) {
      this.errorMessage = 'RIB doit contenir uniquement des chiffres';
    }
    else if (error.message.includes('Destination email is required')) {
      this.errorMessage = 'Email du destinataire requis';
    }
    else {
      this.errorMessage = error.error?.message || error.message || 'Erreur inconnue';
    }
  }

  private generateSuccessMessage(): string {
    return `
      Virement initié avec succès!
      Référence: ${this.transactionDetails.reference}
      Montant: ${this.transactionDetails.amount.toFixed(2)}€
      Destinataire: ${this.transactionDetails.destinationRIB}
      Email: ${this.transactionDetails.destinationEmail}
      Motif: ${this.transactionDetails.Motif || 'Non spécifié'}
      Date: ${this.transactionDetails.date}
    `;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Field error helpers
  getRIBErrors(): string {
    const control = this.virementForm.get('destinationRIB');
    if (control?.hasError('required')) return 'RIB requis';
    if (control?.hasError('pattern')) return 'Uniquement des chiffres';
    if (control?.hasError('minlength')) return 'RIB trop court (min 10 chiffres)';
    if (control?.hasError('maxlength')) return 'RIB trop long (max 20 chiffres)';
    return '';
  }

  getEmailErrors(): string {
    const control = this.virementForm.get('destinationEmail');
    if (control?.hasError('required')) return 'Email requis';
    if (control?.hasError('email')) return 'Format email invalide';
    return '';
  }

  getAmountErrors(): string {
    const control = this.virementForm.get('amount');
    if (control?.hasError('required')) return 'Montant requis';
    if (control?.hasError('min')) return 'Minimum 0.01€';
    if (control?.hasError('max')) return 'Maximum 100 000€';
    if (control?.hasError('pattern')) return 'Format invalide (ex: 125.50)';
    return '';
  }

  getMotifErrors(): string {
    const control = this.virementForm.get('motif');
    if (control?.hasError('maxlength')) return 'Maximum 255 caractères';
    return '';
  }

  debugToken(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Aucun token trouvé');
      return;
    }
    
    try {
      const decoded = jwtDecode(token);
      console.log('Token décodé:', decoded);
      alert(`Contenu du token:\n${JSON.stringify(decoded, null, 2)}`);
    } catch (e) {
      console.error('Erreur de décodage:', e);
      alert('Token invalide');
    }
  }

  shouldShowError(controlName: string): boolean {
    const control = this.virementForm.get(controlName);
    if (!control) return false;
    
    return (
      (control.invalid && (control.dirty || control.touched)) ||
      (control.invalid && this.formSubmitted)
    );
  }
}