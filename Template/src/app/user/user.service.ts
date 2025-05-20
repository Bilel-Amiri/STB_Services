import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5142/api/auth';

  constructor(private http: HttpClient) { }

  // Add this method to decode the token
  getAccountIdFromToken(): number {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('No token found');
    
    try {
      const decoded: any = jwtDecode(token);

      
      const decodedToken = jwtDecode(token);
console.log('Decoded token:', decodedToken);

      
      if (!decoded.AccountId) {
        throw new Error('Token does not contain accountId');
      }
      return decoded.AccountId;
    } catch (e) {
      console.error('Token decoding failed:', e);
      throw new Error('Invalid token');
    }
  }


getClientIdFromToken(): number {
  const token = localStorage.getItem('token');
  if (!token) throw new Error('No token found');

  try {
    const decoded: any = jwtDecode(token);

    console.log('Decoded token:', decoded);

    if (!decoded.ClientId && !decoded.clientId) {
      throw new Error('Token does not contain clientId');
    }

    // Try to handle both camelCase and PascalCase
    return decoded.ClientId || decoded.clientId;
  } catch (e) {
    console.error('Token decoding failed:', e);
    throw new Error('Invalid token');
  }
}






  getBalance(): Observable<any> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${localStorage.getItem('token')}`
    });
    return this.http.get(`${this.apiUrl}/balance`, { headers });
  }


 getUserByAccountId(accountId: number) {
    return this.http.get(`${this.apiUrl}/account-info/${accountId}`);
  }

 getClientInfo(accountId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/Client-info/${accountId}`);
  }



 updateClientInfo(clientID: number, data: any): Observable<any> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${localStorage.getItem('token')}`,
      'Content-Type': 'application/json'
    });
    return this.http.put(`${this.apiUrl}/update-client-info/${clientID}`, data, { headers });
  }

  

      }