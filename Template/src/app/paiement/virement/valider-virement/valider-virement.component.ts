import { Component, OnInit } from '@angular/core';
import { VirementService } from '../../virement/virement.service';
import { ToastrService } from 'ngx-toastr';

interface PendingVirement {
  tempTransactionId: string;
  accountId: number;
  targetRib: string;
  targetEmail: string;
  amount: number;
  motif?: string;
  initiationDate: Date;
  status: string;
}

@Component({
  selector: 'app-valider-virement',
  templateUrl: './valider-virement.component.html',
  styleUrls: ['./valider-virement.component.scss']
})
export class ValiderVirementComponent implements OnInit {
  pendingVirements: PendingVirement[] = [];
  isLoading = false;
  selectedVirement: PendingVirement | null = null;
  debugMode: boolean = true; // Set to false in production
  lastError: string | undefined = undefined; // Changed from null to undefined

  constructor(
    private virementService: VirementService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadPendingVirements();
  }

  retryLoading(): void {
    this.lastError = undefined;
    this.loadPendingVirements();
  }

  loadPendingVirements(): void {
    this.isLoading = true;
    this.pendingVirements = [];
    this.lastError = undefined;
    
    this.virementService.getPendingVirements().subscribe({
      next: (response: any) => {
        console.log('Pending virements received:', response);
        this.pendingVirements = this.mapPendingVirements(response);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading pending virements:', err);
        this.handleLoadError(err);
        this.isLoading = false;
      },
      complete: () => {
        console.log('Pending virements loading complete');
        this.isLoading = false;
      }
    });
  }

  private mapPendingVirements(response: any[]): PendingVirement[] {
    return response
      .map(v => ({
        tempTransactionId: v.tempTransactionId || v.id || '',
        accountId: v.accountId || 0,
        targetRib: v.targetRib || v.rib || '',
        targetEmail: v.targetEmail || v.email || '',
        amount: Number(v.amount) || 0,
        motif: v.motif || undefined,
        initiationDate: this.parseDate(v.initiationDate || v.createdAt),
        status: v.status || 'pending'
      }))
      .filter(v => v.tempTransactionId)
      .sort((a, b) => b.initiationDate.getTime() - a.initiationDate.getTime()); // Sort newest first
  }

  private parseDate(dateString: string | Date): Date {
    try {
      return new Date(dateString);
    } catch (e) {
      console.warn('Invalid date format, using current date instead');
      return new Date();
    }
  }

  private handleLoadError(err: any): void {
    this.lastError = err.error?.message || err.message || 'Erreur inconnue';
    console.error('Loading error:', err);
    this.isLoading = false;
    this.toastr.error(this.lastError || 'Erreur inconnue', 'Erreur de chargement', { timeOut: 3000 });
  }

  selectVirement(virement: PendingVirement): void {
    this.selectedVirement = virement;
  }

  validateVirement(): void {
    if (!this.selectedVirement) return;

    this.isLoading = true;
    this.virementService.validateVirement(this.selectedVirement.tempTransactionId)
      .subscribe({
        next: () => this.handleValidationSuccess(),
        error: (err) => this.handleValidationError(err),
        complete: () => this.isLoading = false
      });
  }

  private handleValidationSuccess(): void {
    this.toastr.success('Virement validé avec succès', '', { timeOut: 2000 });
    this.loadPendingVirements();
    this.selectedVirement = null;
  }

  private handleValidationError(err: any): void {
    const errorMessage = err.error?.message || 'Échec de validation du virement';
    this.toastr.error(errorMessage, 'Erreur', { timeOut: 3000 });
    console.error('Validation error:', err);
  }

  cancelSelection(): void {
    this.selectedVirement = null;
  }
}