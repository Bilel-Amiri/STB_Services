import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree ,Router } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

 constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    const token = localStorage.getItem('token');
    if (token) {
      return true; // Allow access
    } else {
      return this.router.parseUrl('/'); // Redirect to login
    }
  }
  
}
