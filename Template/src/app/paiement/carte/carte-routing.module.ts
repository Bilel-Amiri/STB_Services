import { CarteDetailsComponent } from './carte-details/carte-details.component';
import { CarteComponent } from './carte.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RouterModule, Routes } from '@angular/router';



const routes: Routes = [
  { path: '', component: CarteComponent },
  { path: ':id', component: CarteDetailsComponent, data: { breadcrumb: { alias: 'Details' } } },
 
]

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule
  ]
})
export class CarteRoutingModule { }
