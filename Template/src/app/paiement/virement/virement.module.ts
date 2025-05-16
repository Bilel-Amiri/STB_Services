import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // Added these

import { VirementRoutingModule } from './virement-routing.module';

// Components
import { VirementComponent } from './virement.component';
import { VirementItemComponent } from './virement-item/virement-item.component';
import { VirementDetailsComponent } from './virement-details/virement-details.component';
import { InitierVirementComponent } from './initier-virement/initier-virement.component';
import { ValiderVirementComponent } from './valider-virement/valider-virement.component';

@NgModule({
  declarations: [
    VirementComponent,
    VirementItemComponent,
    VirementDetailsComponent,
    InitierVirementComponent,
    ValiderVirementComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,              // Required for template-driven forms
    ReactiveFormsModule,      // Required for reactive forms
    VirementRoutingModule,
  
  ],
  exports: [
    // Add components here if they need to be used in other modules
    VirementComponent
  ]
})
export class VirementModule { }