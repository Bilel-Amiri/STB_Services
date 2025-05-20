import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserService } from '../../user/user.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  balance: number | null = null;
  exchangeRates: any = null;
  loading: boolean = true;
  errorMessage: string | null = null; // Add error message property

  constructor(
    private userService: UserService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.userService.getBalance().subscribe(
      (response: any) => {
        this.balance = response.balance;
      },
      (error) => {
        console.error('Error fetching balance:', error);
      }
    );

    // Use the more reliable exchangerate-api.com instead
    const apiUrl = 'https://api.exchangerate-api.com/v4/latest/TND';
    
   const apiiUrl = 'https://api.exchangerate-api.com/v4/latest/USD';
    
    this.http.get(apiUrl).subscribe({
      next: (data: any) => {
        console.log('API Response:', data); 
        if (data && data.rates) {
          this.exchangeRates = {
            EUR: data.rates.EUR,
            USD: data.rates.USD
          };
        } else {
          this.errorMessage = 'Invalid response format';
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch exchange rates', err);
        this.errorMessage = 'Failed to load exchange rates. Please try again later.';
        this.loading = false;
      }
    });
  }
}