import { Component } from '@angular/core';
import { CreditService, CreditCreationRequest } from '../credit.service';

@Component({
  selector: 'app-add-credit',
  templateUrl: './add-credit.component.html',
  styleUrls: ['./add-credit.component.scss']
})
export class AddCreditComponent {
  creditData: CreditCreationRequest = {
    clientId: 0, // Keep if needed by backend
    amount: 0,
    durationMonths: 0,
    interestRate: 0
  };
  accountId: number = 0; // Keep if needed by backend
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(private creditService: CreditService) {}

  createCredit() {
    if (!this.isValidInput()) {
      this.errorMessage = 'Please fill all fields with valid values';
      return;
    }

    this.isLoading = true;
    this.clearMessages();

    this.creditService.createCredit(this.accountId, this.creditData).subscribe({
      next: (res) => {
        this.successMessage = 'Credit created successfully!';
        this.isLoading = false;
        this.resetForm();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to create credit';
        this.isLoading = false;
      }
    });
  }

  private isValidInput(): boolean {
    return this.accountId > 0 &&
           this.creditData.amount > 0 &&
           this.creditData.durationMonths > 0 &&
           this.creditData.interestRate >= 0;
  }

  private resetForm(): void {
    this.accountId = 0;
    this.creditData = {
      clientId: 0,
      amount: 0,
      durationMonths: 0,
      interestRate: 0
    };
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}