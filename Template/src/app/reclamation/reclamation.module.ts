import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AddReclamationComponent } from './add-reclamation/add-reclamation.component';
import { ConsulterReclamationsComponent } from './consulter-reclamations/consulter-reclamations.component';
import { ReclamationRoutingModule } from './reclamation-routing.module';
import { FormsModule } from '@angular/forms';




@NgModule({
  declarations: [
    AddReclamationComponent,
    ConsulterReclamationsComponent
    
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReclamationRoutingModule
  ]
})
export class ReclamationModule { }
