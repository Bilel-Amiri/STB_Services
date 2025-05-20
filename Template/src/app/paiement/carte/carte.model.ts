// src/app/models/card.model.ts
export interface Card {
  cardId: number;
  accountId: number;
  maskedNumber: string;
  cardType: string;
  expirationDate: string;
  status: string;
  imageUrl?: string;
}