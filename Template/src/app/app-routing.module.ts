import { HomeComponent } from './home/home.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './resgiter/login/login.component';
import { AddReclamationComponent } from './reclamation/add-reclamation/add-reclamation.component';
import { ConsulterReclamationsComponent } from './reclamation/consulter-reclamations/consulter-reclamations.component';
import { AuthGuard } from './auth.guard';


const routes: Routes = [
  {path:'', component: LoginComponent, data: {breadcrumb: 'Login'}},
  {path:'home', loadChildren:()=> import('./home/home.module').then(mod => mod.HomeModule), canActivate: [AuthGuard], data: {breadcrumb: 'Home'} },



  { 
    path: 'about', 
    loadChildren: () => import('./resgiter/about/about.module').then(m => m.AboutModule)
  },


  { 
    path: 'contact', 
    loadChildren: () => import('./resgiter/contact/contact.module').then(m => m.ContactModule),
    data: { breadcrumb: 'Contact' }
  },

  { 
    path: 'faq', 
    loadChildren: () => import('./resgiter/faq/faq.module').then(m => m.FaqModule),
    data: { breadcrumb: 'FAQ' }
  },




  // {path:'test-error', component: TestErrorComponent, data: {breadcrumb: 'Test  Errors'}},
  // {path:'server-error', component: ServerErrorComponent, data: {breadcrumb: 'Server  Errors'}},
  // {path:'not-found', component: NotFoundComponent, data: {breadcrumb: 'Not Found'}},
   //{path:'virement', loadChildren:()=> import('./paiement/virement/virement.module').then(mod => mod.VirementModule), data: {breadcrumb: 'Virement'} },
  // {path:'carte', loadChildren:()=> import('./paiement/carte/carte.module').then(mod => mod.CarteModule), data: {breadcrumb: 'Carte'} },
  // { path: 'beneficiaire', loadChildren: () => import('./paiement/beneficiaires/beneficiaire.module').then(mod => mod.BeneficiaireModule), data: { breadcrumb: 'Beneficiaire'} },
  // { path: 'user', loadChildren: () => import('./user/user.module').then(mod => mod.UserModule), data: { breadcrumb: 'User'} },
  {path:'**', redirectTo:'not-found', pathMatch: 'full'},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
