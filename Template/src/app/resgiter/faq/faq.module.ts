// src/app/resgiter/faq/faq.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FaqComponent } from './faq.component';

@NgModule({
  declarations: [FaqComponent],
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: '', component: FaqComponent }
    ])
  ]
})
export class FaqModule { }