import { Component, OnInit } from '@angular/core';
import { ReclamationService } from '../reclamation.service';
import { UserService } from '../../user/user.service';

@Component({
  selector: 'app-consulter-reclamations',
  templateUrl: './consulter-reclamations.component.html',
  styleUrls: ['./consulter-reclamations.component.scss']
})
export class ConsulterReclamationsComponent implements OnInit {

  reclamations: any[] = [];
  errorMessage = '';

  constructor(
    private reclamationService: ReclamationService,
    private userService: UserService
  ) {}

  ngOnInit() {
    const accountId = this.userService.getAccountIdFromToken();
    if (accountId) {
      this.reclamationService.getReclamationsByAccount(accountId).subscribe({
        next: (data) => {
          this.reclamations = data;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load reclamations';
          console.error(error);
        }
      });
    } else {
      this.errorMessage = 'Invalid account ID';
    }
  }
}