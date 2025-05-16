// src/app/resgiter/about/about.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AboutComponent } from './about.component';

@NgModule({
  declarations: [
    AboutComponent // This should be the ONLY declaration
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: '', component: AboutComponent }
    ])
  ]
})
export class AboutModule { }