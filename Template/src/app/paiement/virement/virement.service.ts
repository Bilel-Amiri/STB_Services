import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';
import { Transaction } from '../virement/transaction.model';
import { HttpResponse } from '@angular/common/http';

interface JwtPayload {
  accountId?: number;
  sub?: string;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class VirementService {
  private apiUrl = 'http://localhost:5010/api/transactions';

  constructor(private http: HttpClient) { }

  initiateVirement(virementData: any): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('Authentication required');
  
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const accountId = this.extractAccountId(decoded);
  
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });
  
      const payload = {
        source_account_id: accountId,
        destination_rib: Number(virementData.destinationRIB),
        destination_email: virementData.destinationEmail.trim(),
        amount: Number(virementData.amount),
        Motif: virementData.motif || null
      };
  
      console.log('Payload sent to backend:', JSON.stringify(payload, null, 2));
  
      return this.http.post(`${this.apiUrl}/virements/initiate`, payload, { headers }).pipe(
        map((response: any) => ({
          tempTransactionId: response.tempTransactionId,
          message: response.message,
          motif: response.Motif
        })),
        catchError(error => {
          console.error('Full error:', error); // Optional: keep for dev only
  
          const friendlyMessage = 'Initiation de virement échouée. Veuillez vérifier les informations saisies.';
          return throwError(() => new Error(friendlyMessage));
        })
      );
    } catch (error) {
      console.error('Initiation failed:', error);
      return throwError(() => new Error('Initiation de virement échouée.'));
    }
  }
  
  
  

  getVirementHistory(accountId: number): Observable<Transaction[]> {
    const token = localStorage.getItem('token');
    if (!token) {
      return throwError(() => new Error('Authentication required'));
    }
  
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  
    return this.http.get<any[]>(`${this.apiUrl}/history`, { headers }).pipe(
      map((transactions: any[]) => transactions.map((t: any) => {
        return {
          transactionId: t.transactionId || t.TransactionId,
          accountId: t.accountId || t.AccountId,
          amount: t.amount || t.Amount,
          transactionType: t.transactionType || t.TransactionType,
          transactionDate: t.transactionDate ? new Date(t.transactionDate) : 
                           t.TransactionDate ? new Date(t.TransactionDate) : new Date(),
          targetAccountId: t.targetAccountId || t.TargetAccountId,
          targetRib: t.targetRib || t.TargetRib || t.Target_Rib || t.target_Rib
        } as Transaction;
      })),
      map((transactions: Transaction[]) => 
        transactions.sort((a, b) => b.transactionDate.getTime() - a.transactionDate.getTime())
      ),
      catchError(this.handleServerError)
    );
  }

  private extractAccountId(decoded: JwtPayload): number {
    // Try all possible claim names
    const accountId =
      decoded['account_id'] ||
      decoded['AccountId'] ||
      decoded['accountId'] ||
      decoded['sub'] || // Standard JWT subject claim
      decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || // Common in .NET
      decoded['userId'] || // Another common alternative
      decoded['id']; // Generic fallback
  
    console.log('Token claims:', decoded); // Debug all claims
    console.log('Extracted account ID:', accountId); // Debug extracted value
  
    if (!accountId) {
      console.error('Token does not contain account identification:', decoded);
      throw new Error('Token does not contain account identification');
    }
  
    const numericId = Number(accountId);
    if (isNaN(numericId)) {
      console.error('Invalid account ID format in token:', accountId);
      throw new Error('Invalid account ID format in token');
    }
  
    return numericId;
  }
  
  

  private handleServerError(error: any): Observable<never> {
    console.error('Server error:', error);
    if (error.status === 0) {
      return throwError(() => new Error('Connexion au serveur perdue'));
    }
    const serverMessage = error.error?.message || error.message;
    return throwError(() => new Error(serverMessage || 'Erreur inconnue du serveur'));
  }


  getPendingVirements(): Observable<PendingVirement[]> {
    const token = localStorage.getItem('token');
    if (!token) return throwError(() => new Error('Authentication required'));
  
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const accountId = this.extractAccountId(decoded);
  
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });
  
      return this.http.get<any[]>(`${this.apiUrl}/virements/pending/${accountId}`, { headers }).pipe(
        map((virements: any[]) => virements.map(v => ({
          tempTransactionId: v.tempTransactionId || v.TempTransactionId,
          accountId: v.accountId || v.AccountId,
          targetRib: v.targetRib || v.TargetRib,
          targetEmail: v.targetEmail || v.TargetEmail,
          amount: v.amount || v.Amount,
          motif: v.motif || v.Motif,
          initiationDate: v.initiationDate || v.InitiationDate
        } as PendingVirement))),
        catchError(this.handleServerError)
      );
    } catch (error) {
      console.error('Error decoding token:', error);
      return throwError(() => new Error('Invalid authentication token'));
    }
  }
  
  validateVirement(tempTransactionId: string): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) return throwError(() => new Error('Authentication required'));
  
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  
    return this.http.post(
      `${this.apiUrl}/virements/validate/${tempTransactionId}`,
      {},
      { headers }
    ).pipe(
      catchError(this.handleServerError)
    );
  }


}

export interface PendingVirement {
  tempTransactionId: string;
  accountId: number;
  targetRib: string;
  targetEmail: string;
  amount: number;
  motif: string;
  initiationDate: string;
}