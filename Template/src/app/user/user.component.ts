import { Component, OnInit } from '@angular/core';

import {UserService} from './user.service';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  clientInfo: any;

 clientID!: number;

  userInfo = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',

    
   
  };


  constructor(private userService: UserService) {}

  ngOnInit(): void {

this.loadUserData();  


    try {
      const accountId = this.userService.getAccountIdFromToken();
      this.userService.getClientInfo(accountId).subscribe({
        next: (data) => {
          this.clientInfo = data;
        },
        error: (err) => {
          console.error('Error fetching client info:', err);
        }
      });
    } catch (e) {
      console.error(e);
    }
  }


 loadUserData() {
    try {
      const clientId = this.userService.getAccountIdFromToken();
      this.clientID = clientId; // Save it in case you need it later
      this.userService.getClientInfo(clientId).subscribe({
        next: (data) => {
          this.userInfo = data; // Fill form fields
          console.log('User data loaded:', this.userInfo);
        },
        error: (err) => {
          console.error('Failed to load user info', err);
        }
      });
    } catch (error) {
      console.error('Error getting client ID from token:', error);
    }
  }



 updateUserInfo() {
  const clientId = this.userService.getClientIdFromToken();
 
  this.userService.updateClientInfo(clientId, this.userInfo).subscribe({
    next: res => console.log('Update successful', res),
    error: err => console.error('Update failed:', err)
  });
}


}
