import { Component } from '@angular/core';
import { ReclamationService } from '../reclamation.service';
import { UserService } from  '../../user/user.service'
import { jwtDecode } from 'jwt-decode';

@Component({
  selector: 'app-add-reclamation',
  templateUrl: './add-reclamation.component.html',
  styleUrls: ['./add-reclamation.component.scss']
})
export class AddReclamationComponent {
  subject = '';
  description = '';

  constructor(
    private reclamationService: ReclamationService,
    private userService: UserService
  ) {}

  message: string = '';
isError: boolean = false;


onSubmit() {
  const accountId = this.userService.getAccountIdFromToken();

  const dto = {
    accountId,
    subject: this.subject,
    description: this.description
  };

  this.reclamationService.addReclamation(dto).subscribe({
    next: () => {
      this.message = 'Réclamation créée avec succès !';
      this.isError = false;
      this.subject = '';
      this.description = '';
    },
    error: error => {
      console.error('Erreur lors de la création :', error);
      this.message = 'Une erreur est survenue. Veuillez réessayer.';
      this.isError = true;
    }
  });
}




  }


  




