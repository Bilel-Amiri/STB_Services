import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing.module';
import { AccountsComponent } from './accounts/accounts.component';
import { ReclamationsComponent } from './reclamations/reclamations.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AdminComponent } from './admin.component';
import { SidebarComponent } from './sidebar/sidebar.component';



@NgModule({
  declarations: [
    AccountsComponent,
    ReclamationsComponent,
    DashboardComponent,
    AdminComponent,
    SidebarComponent,
    
    
    
  ],
  imports: [
    CommonModule,
    AdminRoutingModule
  ]
})
export class AdminModule { }
