// src/app/services/card-image.service.ts
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CardImageService {
  private cardImages: { [key: string]: string } = {
    'VISA CLASSIC INTERNATIONALE': 'assets/img/CARTE VISA CLASSIC INTERNATIONALE.png',
    'CIB': 'assets/img/CIB.png',
    'VISA CLASSIC NATIONALE': 'assets/img/VISA CLASSIC NATIONALE.png',
    'STB VISA PLATINUM BUSINESS INTERNATIONALE':'assets/img/STB VISA PLATINUM BUSINESS INTERNATIONALE.png',
    'VISA PLATINUM BUSINESS NATIONALE': 'assets/img/VISA PLATINUM BUSINESS NATIONALE.png',
    'STB TRAVEL': 'assets/img/STB TRAVEL.png',
    'C-Cash': 'assets/img/C-Cash.png',
    'VISA ELECTRON NATIONALE': 'assets/img/VISA ELECTRON NATIONALE.png',
    'TECHNOLOGIQUE INTERNATIONALE': 'assets/img/TECHNOLOGIQUE INTERNATIONALE.png',
    'MASTERCARD GOLD INTERNATIONALE': 'assets/img/CARTE MASTERCARD GOLD INTERNATIONALE.png',
    'MASTERCARD GOLD NATIONALE': 'assets/img/CARTE MASTERCARD GOLD NATIONALE.png',
    'C-Pay': 'assets/img/C-Pay.png',
    'Epargne': 'assets/img/Epargne.png',
    'default': 'assets/img/default-card.png'


    
  };

  getCardImage(cardType: string): string {
    // Remove .toLowerCase() since your keys are case-sensitive
    return this.cardImages[cardType] || this.cardImages['default'];
  }
}