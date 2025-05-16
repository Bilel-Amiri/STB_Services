import { PaiementModule } from './../paiement/paiement.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SideBarComponent } from './side-bar/side-bar.component';
import { CoreModule } from '../core/core.module';
import { HomeRoutingModule } from './home-routing.module';
import { PaiementRoutingModule } from '../paiement/paiement-routing.module';
import { ReclamationModule } from './../reclamation/reclamation.module';



@NgModule({
  declarations: [
    HomeComponent,
    DashboardComponent,
    SideBarComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    HomeRoutingModule,
    PaiementRoutingModule,
     ReclamationModule

  ],
  exports: [
    HomeComponent
  ]
})
export class HomeModule { }
