
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReclamationService {
  private apiUrl = 'http://localhost:5041/api/reclamations';  

  constructor(private http: HttpClient) {}

  



 addReclamation(dto: any): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    return this.http.post(`${this.apiUrl}`, dto, { headers });
  }

  




   getReclamationsByAccount(accountId: number): Observable<any> {
   const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    
    return this.http.get(`${this.apiUrl}/account/${accountId}`, { headers });
  }
}
