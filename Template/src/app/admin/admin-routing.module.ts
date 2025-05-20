// admin-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AccountsComponent } from './accounts/accounts.component';
import { ReclamationsComponent } from './reclamations/reclamations.component';
import { AdminComponent } from './admin.component';


const routes: Routes = [
  {
    path: '',
    
    component: AdminComponent,
    
    children: [
     
      { path: 'dashboard', component: DashboardComponent },
      { path: 'accounts', component: AccountsComponent },
      { path: 'reclamations', component: ReclamationsComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
