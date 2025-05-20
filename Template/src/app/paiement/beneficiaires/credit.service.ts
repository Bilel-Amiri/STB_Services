import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CreditService {
  private baseUrl = 'http://localhost:5165/api/credits'; 

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  // Get Credit Status
  getCreditStatus(clientId: number, accountID: number): Observable<any> {
    const params = new HttpParams()
      .set('clientId', clientId.toString())
      .set('accountID', accountID.toString());
    
    return this.http.get(`${this.baseUrl}/status`, { 
      params,
      headers: this.getHeaders() 
    });
  }

  
  simulateCredit(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/simulate`, data, {
      headers: this.getHeaders()
    });
  }

 

  demandeCredit(accountId: number, input: DemandeCreditInputModel): Observable<any> {


     const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    return this.http.post(`${this.baseUrl}/demande-credit/${accountId}`, input);
  }

  

 
}

export interface DemandeCreditInputModel {
  creditAmount: number;
  durationMonths: number;
  creditType: string;
  amortizationType: string;
  cin?: string;
  maritalStatus?: string;
}
