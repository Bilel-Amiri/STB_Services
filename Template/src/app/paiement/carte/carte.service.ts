// src/app/services/carte.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Card } from './carte.model';
import { CardImageService } from './Carte_image_Service';

@Injectable({
  providedIn: 'root'
})
export class CarteService {
  private apiUrl = 'http://localhost:5236/api/cards';  

  constructor(
    private http: HttpClient,
    private cardImageService: CardImageService
  ) { }

  getCards(): Observable<Card[]> {
    const token = localStorage.getItem('token'); 
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    
    return this.http.get<Card[]>(this.apiUrl, { headers }).pipe(
      map(cards => cards.map(card => ({
        ...card,
        // Add the imageUrl property here
        imageUrl: this.cardImageService.getCardImage(card.cardType)
      })))
    );
  }



  

  getCardDetails(cardId: number): Observable<Card> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    
    return this.http.get<Card>(`${this.apiUrl}/${cardId}`, { headers }).pipe(
      map(card => ({
        ...card,
        imageUrl: this.cardImageService.getCardImage(card.cardType)
      }))
    );
  }


  blockCard(cardId: number, reason: string): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.post(`${this.apiUrl}/${cardId}/block`, { reason }, { headers });
  }

  deblockCard(cardId: number): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.post(`${this.apiUrl}/${cardId}/deblock`, {}, { headers });

  }
}


