import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // { path: '', loadChildren: () => import('./../paiement/virement/virement.module').then(mod => mod.VirementModule), data: { breadcrumb: 'Virement' } },
  { path: 'virement', loadChildren: () => import('./../paiement/virement/virement.module').then(mod => mod.VirementModule), data: { breadcrumb: 'Virement' } },
  { path: 'carte', loadChildren: () => import('./../paiement/carte/carte.module').then(mod => mod.CarteModule), data: { breadcrumb: 'Carte' } },
  { path: 'credit', loadChildren: () => import('./../paiement/beneficiaires/credit.Module').then(mod => mod.CreditModule), data: { breadcrumb: 'Credit' } },
  { path: 'user', loadChildren: () => import('./../user/user.module').then(mod => mod.UserModule), data: { breadcrumb: 'User' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PaiementRoutingModule { }
