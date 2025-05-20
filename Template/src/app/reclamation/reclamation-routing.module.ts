import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddReclamationComponent } from './add-reclamation/add-reclamation.component';
import { ConsulterReclamationsComponent } from './consulter-reclamations/consulter-reclamations.component';

const routes: Routes = [
  { path: 'add', component: AddReclamationComponent },
  { path: 'consulter', component: ConsulterReclamationsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReclamationRoutingModule {}
