import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { UserDetailsComponent } from './user-details/user-details.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { UserComponent } from './user.component';

const routes: Routes = [
  { path: '', component: UserComponent },
  { path: ':id', component: UserDetailsComponent, data: { breadcrumb: { alias: 'productDetails' } } },
  { path: 'changePassword', component: ChangePasswordComponent, data: { breadcrumb: { alias: 'productDetails' } } },
  { path: 'UserSettingsComponent', component: UserSettingsComponent, data: { breadcrumb: { alias: 'productDetails' } } },
]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
