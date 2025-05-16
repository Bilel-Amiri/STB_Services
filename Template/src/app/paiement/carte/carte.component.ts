import { Component, OnInit } from '@angular/core';
import { CarteService } from './carte.service';
import { Card } from './carte.model';
import { MatDialog } from '@angular/material/dialog';
import { BlockCardComponent } from './block-card/block-card.component';

@Component({
  selector: 'app-carte',
  templateUrl: './carte.component.html',
  styleUrls: ['./carte.component.scss']
})
export class CarteComponent implements OnInit {
  cards: Card[] = [];
  selectedCardId: number | null = null;
  error: string | null = null;
  isLoading = false;

  constructor(
    private carteService: CarteService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadCards();
  }

  loadCards(): void {
    this.isLoading = true;
    this.carteService.getCards().subscribe({
      next: (cards: Card[]) => {
        this.cards = cards.map(card => ({
          ...card,
          status: card.status?.toUpperCase() ?? 'INACTIVE' 
        }));
        this.isLoading = false;
        this.error = null;
      },
      
      error: () => {
        this.error = 'Failed to load cards';
        this.isLoading = false;
      }
    });
  }

  toggleCardActions(cardId: number): void {
    this.selectedCardId = this.selectedCardId === cardId ? null : cardId;
  }

  openBlockDialog(card: Card): void {
    const dialogRef = this.dialog.open(BlockCardComponent, {
      width: '400px',
      data: { cardId: card.cardId, cardNumber: card.maskedNumber }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.confirmed && result.reason) {
        this.blockCard(result.cardId, result.reason);
      }
    });
  }

  blockCard(cardId: number, reason: string): void {
    this.carteService.blockCard(cardId, reason).subscribe({
      next: () => {
        this.loadCards();
        this.selectedCardId = null;
      },
      error: () => {
        this.error = 'Failed to block card';
      }
    });
  }

  deblockCard(cardId: number): void {
    this.carteService.deblockCard(cardId).subscribe({
      next: () => {
        this.loadCards();
        this.selectedCardId = null;
      },
      error: () => {
        this.error = 'Failed to unblock card';
      }
    });
  }
}
