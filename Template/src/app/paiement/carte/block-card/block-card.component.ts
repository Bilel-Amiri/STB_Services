import { Component, Inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-block-card',
  templateUrl: './block-card.component.html',
  styleUrls: ['./block-card.component.scss']
})
export class BlockCardComponent {
  reasonControl = new FormControl('', [Validators.required]);
  cardLastFour: string;

  constructor(
    public dialogRef: MatDialogRef<BlockCardComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { cardId: number, cardNumber: string }
  ) {
    this.cardLastFour = data.cardNumber.slice(-4);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    if (this.reasonControl.valid) {
      this.dialogRef.close({
        confirmed: true,
        reason: this.reasonControl.value,
        cardId: this.data.cardId
      });
    }
  }
}