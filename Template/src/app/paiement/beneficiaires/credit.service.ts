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

  // Simulate Credit
  simulateCredit(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/simulate`, data, {
      headers: this.getHeaders()
    });
  }

  // Create Credit
  createCredit(accountId: number, data: any): Observable<any> {
    const params = new HttpParams().set('accountId', accountId.toString());
    return this.http.post(this.baseUrl, data, { 
      params,
      headers: this.getHeaders()
    });
  }

  // Check & Send Notifications
  checkAndSendNotifications(): Observable<any> {
    return this.http.post(`${this.baseUrl}/notifications/check`, {}, {
      headers: this.getHeaders()
    });
  }

  // Get Upcoming Notifications
  getUpcomingNotifications(clientId: number, daysAhead: number = 7): Observable<any> {
    const params = new HttpParams()
      .set('clientId', clientId.toString())
      .set('daysAhead', daysAhead.toString());
    
    return this.http.get(`${this.baseUrl}/notifications/upcoming`, { 
      params,
      headers: this.getHeaders()
    });
  }

  // Resend Notification
  resendNotification(creditId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/notifications/resend/${creditId}`, {}, {
      headers: this.getHeaders()
    });
  }
}

export interface CreditCreationRequest {
  clientId: number;
  amount: number;
  durationMonths: number;
  interestRate: number;
}