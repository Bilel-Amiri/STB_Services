import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PaiementRoutingModule } from './paiement-routing.module';

// Child modules
import { CreditModule } from './beneficiaires/credit.Module';
import { VirementModule } from './virement/virement.module';
import { CarteModule } from './carte/carte.module';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';




@NgModule({
  declarations: [
    
    
    
  ], // Keep empty if no components are declared directly in this module
  imports: [
    CommonModule,
    PaiementRoutingModule, // Should come before feature modules
    CreditModule,
    VirementModule,
    CarteModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatInputModule,
    ReactiveFormsModule,
    FormsModule,
    CommonModule,
  ],
  exports: [] // Add if you need to export any components/modules
})
export class PaiementModule { }