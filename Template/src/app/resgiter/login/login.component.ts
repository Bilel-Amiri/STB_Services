import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup; // Reactive form group
  errorMessage: string = ''; // To display error messages

  constructor(
    private fb: FormBuilder, // FormBuilder for reactive forms
    private http: HttpClient, // HttpClient for making API calls
    private router: Router // Router for navigation
  ) {
    // Initialize the login form in the constructor
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]], // Email field with validation
      password: ['', Validators.required] // Password field with validation
    });
  }

  ngOnInit(): void {
   localStorage.removeItem('token');
  }

  // Helper method to easily access form controls
  get f() {
    return this.loginForm.controls;
  }

  // Method to handle form submission
  onSubmit() {
    console.log('Form submitted');
    // Check if the form is invalid
    if (this.loginForm.invalid) {
      return;
    }

    // Prepare the payload for the API request
    const payload = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    // Make a POST request to the backend API
    this.http.post('http://localhost:5142/api/auth/login', payload)
      .subscribe({
        next: (response: any) => {
          console.log('Login successful:', response);

          // Store the token in localStorage (if using JWT)
          localStorage.setItem('token', response.token);

          // Redirect to another page (e.g., dashboard)
          this.router.navigate(['/home']);
        },
        error: (error) => {
          console.error('Login failed:', error);

          // Display an error message to the user
          this.errorMessage = error.error?.message || 'Invalid email or password';
        }
      });
  }
}