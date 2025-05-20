import { Component, OnInit } from '@angular/core';
import {UserService}  from '../../user/user.service'

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss']
})
export class NavBarComponent implements OnInit {


 userName: string = '';
  

  constructor(private userService: UserService) {   }

  ngOnInit(): void {


 try {
      const clientId = this.userService.getAccountIdFromToken();
      this.userService.getClientInfo(clientId).subscribe({
        next: (data) => {
          this.userName = data.firstName + (data.lastName ? ' ' + data.lastName : '');
        },
        error: (err) => {
          console.error('Failed to load user info for navbar:', err);
        }
      });
    } catch (error) {
      console.error('Error getting client ID from token:', error);
    }
  }
    
  

  ShowHideMenu(){
    document.getElementsByTagName("body")[0].classList.toggle('toggle-sidebar');
  }

}
