import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { HomeComponent } from './home.component';

const routes: Routes = [
  {path:'', component: HomeComponent ,
        children: [
            { path: '', component: DashboardComponent, data: { breadcrumb: 'Dashboard' }},
            { path: 'dashboard', component: DashboardComponent, data: { breadcrumb: 'Dashboard' }},
            { path: 'virement', loadChildren: () => import('./../paiement/virement/virement.module').then(mod => mod.VirementModule), data: { breadcrumb: 'Virement' } },
            { path: 'carte', loadChildren: () => import('./../paiement/carte/carte.module').then(mod => mod.CarteModule), data: { breadcrumb: 'Carte' } },
            { path: 'credit', loadChildren: () => import('./../paiement/beneficiaires/credit.Module').then(mod => mod.CreditModule), data: { breadcrumb: 'Credit' } },
            { path: 'user', loadChildren: () => import('./../user/user.module').then(mod => mod.UserModule), data: { breadcrumb: 'User' } },
            {path: 'reclamation', loadChildren: () => import('./../reclamation/reclamation.module').then(m => m.ReclamationModule) },
            { path: '**', redirectTo: '', pathMatch: 'full' }
        ],
    },

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }
