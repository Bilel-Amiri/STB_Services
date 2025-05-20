import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CreditItemComponent } from './credit-item/credit-item.component';
import { CreditDetailsComponent } from './credit-details/credit-details.component';
import { CreditComponent } from './credit.component';
import { CreditRoutingModule } from './credit-routing.module';
import { AddCreditComponent } from './add-credit/add-credit.component';
import { SimulateurCreditComponent } from './stimuler-credit/stimuler-credit.component';
import { FormsModule,ReactiveFormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    CreditComponent,
    CreditItemComponent,
    CreditDetailsComponent,
    AddCreditComponent,
    SimulateurCreditComponent
  ],
  imports: [
    CommonModule,
    CreditRoutingModule,
    CommonModule,
    FormsModule,
     ReactiveFormsModule
  ]
})
export class CreditModule { }
