import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-loginadm',
  templateUrl: './loginadm.component.html',
  styleUrls: ['./loginadm.component.scss']
})
export class LoginadmComponent {
  username: string = '';
  password: string = '';

  constructor(private router: Router) {}

  onSubmit() {
    // Simulation d'une connexion admin réussie (à remplacer par un appel API)
    if (this.username === 'admin' && this.password === 'admin123') {
      localStorage.setItem('role', 'admin'); // Stocke le rôle
      this.router.navigate(['/admin/dashboard']); // Redirige vers le dashboard admin
    } else {
      alert('Identifiants incorrects');
    }
  }
}