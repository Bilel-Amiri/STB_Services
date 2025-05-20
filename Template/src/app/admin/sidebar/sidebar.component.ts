import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  isComptesOpen = false;
  isReclamationsOpen = false;

  toggleComptes() {
    this.isComptesOpen = !this.isComptesOpen;
    // Fermer les autres sous-menus si nécessaire
    if (this.isComptesOpen) {
      this.isReclamationsOpen = false;
    }
  }

  toggleReclamations() {
    this.isReclamationsOpen = !this.isReclamationsOpen;
    // Fermer les autres sous-menus si nécessaire
    if (this.isReclamationsOpen) {
      this.isComptesOpen = false;
    }
  }
}