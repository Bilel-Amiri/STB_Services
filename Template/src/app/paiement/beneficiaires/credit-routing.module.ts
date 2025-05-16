import { CreditComponent } from './credit.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RouterModule, Routes } from '@angular/router';
import { CreditDetailsComponent } from './credit-details/credit-details.component';
import { AddCreditComponent } from './add-credit/add-credit.component';
import { SimulateurCreditComponent } from './stimuler-credit/stimuler-credit.component';
const routes: Routes = [
  {
    path: '',
    component: CreditComponent,
    children: [
      { path: 'ajouter-credit', component: AddCreditComponent, data: { breadcrumb: { alias: 'Ajouter' } } },
      { path: 'simulateur', component: SimulateurCreditComponent, data: { breadcrumb: { alias: 'Simulateur' } } },
      { path: ':id', component: CreditDetailsComponent, data: { breadcrumb: { alias: 'Details' } } },
    ]
  }
];
@NgModule({
  declarations: [],
  imports: [
    RouterModule.forChild(routes)
  ],
  exports :[
    RouterModule
  ]
})
export class CreditRoutingModule { }
