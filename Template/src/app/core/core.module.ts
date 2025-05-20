import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { SectionHeaderComponent } from './section-header/section-header.component';
import { BreadcrumbModule } from 'xng-breadcrumb';

@NgModule({
  declarations: [
    NavBarComponent,
    SectionHeaderComponent
  ],
  imports: [
    CommonModule,
    BreadcrumbModule

  ],
  exports:[ NavBarComponent,
    SectionHeaderComponent]
})
export class CoreModule { }
