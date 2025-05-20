import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { VirementService } from '../virement.service';
import { Transaction } from '../transaction.model';
import { jwtDecode } from 'jwt-decode';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-virement-details',
  templateUrl: './virement-details.component.html',
  styleUrls: ['./virement-details.component.scss'],
  providers: [DatePipe]
})
export class VirementDetailsComponent implements OnInit {
  transactions: Transaction[] = [];
  filteredTransactions: Transaction[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  filterForm: FormGroup;
  today = new Date();
  accountId: number | null = null;

  constructor(
    private virementService: VirementService,
    private fb: FormBuilder,
    private datePipe: DatePipe
  ) {
    this.filterForm = this.fb.group({
      filterValue: [null],
      filterType: ['none']
    });
  }

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.errorMessage = null;
  
    try {
      this.accountId = this.getAccountIdFromToken();
      console.log('Loading history for account:', this.accountId);
  
      this.virementService.getVirementHistory(this.accountId).subscribe({
        next: (transactions) => {
          console.log('Raw transactions:', transactions);
          
          this.transactions = transactions.map(t => {
            // Convert transactionDate to Date if it's a string
            const transactionDate = typeof t.transactionDate === 'string' 
              ? new Date(t.transactionDate) 
              : t.transactionDate;
  
            return {
              ...t,
              amount: t.accountId === this.accountId ? -Math.abs(t.amount) : Math.abs(t.amount),
              transactionDate: transactionDate,
              targetRib: t.targetRib?.toString() // Convert to string if it exists
            } as Transaction;
          });
          
          this.filteredTransactions = [...this.transactions];
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error loading history:', err);
          this.errorMessage = err.message || 'Failed to load transaction history';
          this.isLoading = false;
        }
      });
    } catch (error: any) {
      console.error('Initialization error:', error);
      this.errorMessage = error.message;
      this.isLoading = false;
    }
  }


  onAmountInputChange(): void {
    const filterType = this.filterForm.get('filterType')?.value;
    if (filterType === 'amount') {
      this.applyFilter();
    }
  }

  applyFilter(): void {
    const { filterType, filterValue } = this.filterForm.value;
  
    if (!filterValue || filterType === 'none') {
      this.filteredTransactions = [...this.transactions];
      return;
    }
  
    if (filterType === 'amount') {
      this.filteredTransactions = this.transactions.filter(
        t => Math.abs(t.amount) === Math.abs(Number(filterValue))
      );
    } else if (filterType === 'date') {
      const selectedDate = new Date(filterValue);
      const nextDay = new Date(selectedDate);
      nextDay.setDate(nextDay.getDate() + 1);
      
      this.filteredTransactions = this.transactions.filter(t => {
        const transactionDate = new Date(t.transactionDate);
        return transactionDate >= selectedDate && transactionDate < nextDay;
      });
    }
  }

  resetFilters(): void {
    this.filterForm.reset({
      filterType: 'none',
      filterValue: null
    });
    this.filteredTransactions = [...this.transactions];
  }

  formatDate(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return this.datePipe.transform(dateObj, 'yyyy-MM-dd') || '';
  }

  private getAccountIdFromToken(): number {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('Session expirée. Veuillez vous reconnecter.');

    try {
      const decoded: any = jwtDecode(token);
      const accountId = decoded['AccountId'] || decoded['accountId'];
      if (!accountId) throw new Error('Aucun ID de compte trouvé dans le token');
      return Number(accountId);
    } catch (e) {
      throw new Error('Token invalide');
    }
  }

  getAbsoluteAmount(amount: number): number {
    return Math.abs(amount);
  }

}