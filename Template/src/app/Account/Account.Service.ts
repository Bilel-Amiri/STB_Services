// src/app/services/account.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Account {
  id: string;
  accountNumber: string;
  balance: number;
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = 'http://localhost:5010/api/accounts';

  constructor(private http: HttpClient) { }

  getUserAccounts(): Observable<Account[]> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.get<Account[]>(`${this.apiUrl}/me`, { headers });
  }
}