import { CoreModule } from './core/core.module';
import { HomeModule } from './home/home.module';
import { NgModule, LOCALE_ID } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgxSpinnerModule } from 'ngx-spinner';
import { LoginComponent } from './resgiter/login/login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import localeFr from '@angular/common/locales/fr';
import localeFrExtra from '@angular/common/locales/extra/fr';
import { CommonModule, registerLocaleData } from '@angular/common';
import { ToastrModule } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AddCreditComponent } from './paiement/beneficiaires/add-credit/add-credit.component';










registerLocaleData(localeFr, 'fr', localeFrExtra);

@NgModule({

  declarations: [
    AppComponent,
    LoginComponent ,
    
  ],

  
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    NgxSpinnerModule,
    ReactiveFormsModule,
    MatIconModule,
    ToastrModule.forRoot(),
    FormsModule,
    RouterModule,
    
    
   

  ],
  providers: [{ provide: LOCALE_ID, useValue: 'fr' }],
  bootstrap: [AppComponent],
  exports: [ToastrModule]
})
export class AppModule {}