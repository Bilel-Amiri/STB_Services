import { CarteRoutingModule } from './carte-routing.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // Add this
import { MatDialogModule } from '@angular/material/dialog'; // Add this
import { MatButtonModule } from '@angular/material/button'; // Add this
import { MatRadioModule } from '@angular/material/radio'; // Add this for radio buttons
import { MatIconModule } from '@angular/material/icon'; // Add this for icons

import { CarteItemComponent } from './carte-item/carte-item.component';
import { CarteDetailsComponent } from './carte-details/carte-details.component';
import { CarteComponent } from './carte.component';
import { CarteService } from '../carte/carte.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BlockCardComponent } from './block-card/block-card.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    CarteComponent,
    CarteItemComponent,
    CarteDetailsComponent,
    BlockCardComponent,
  ],
  providers: [CarteService],
  imports: [
    CommonModule,
    CarteRoutingModule,
    MatProgressSpinnerModule,
    FormsModule, // Required for ngModel
    MatDialogModule, // For dialog functionality
    MatButtonModule, // For buttons
    MatRadioModule, // For radio buttons
    MatIconModule ,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    ReactiveFormsModule,
    
  ],
  entryComponents: [BlockCardComponent]
})
export class CarteModule { }