import { Component, ElementRef, ViewChild } from '@angular/core';
import { UserService } from '../services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  userIsConnected: boolean = false;
  isAdmin: boolean = false; // Détermine si l'utilisateur a le rôle d'administrateur ou pas

  // Vous êtes obligés d'utiliser ces trois propriétés
  oldPassword: string = "";
  newPassword: string = "";
  newPasswordConfirm: string = "";

  modUsername: string = ""

  username: string | null = null;
  avatarUrl: string = '';
  defaultAvatarUrl: string = 'assets/default-avatar.png';


  constructor(public userService: UserService) { }
  @ViewChild("myPictureViewChild", { static: false }) pictureInput?: ElementRef;

  async ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");


    if (this.username) {
      this.avatarUrl = `https://localhost:7216/api/Users/GetAvatar/${this.username}`;
    }

    this.isAdmin = localStorage.getItem("role") === "admin"; // Check if user has admin role
    console.log('isAdmin:', this.isAdmin); // Debugging line
  }
  async changeAvatar(): Promise<void> {
    const file = this.pictureInput?.nativeElement.files[0];
    if (file) {
      const formData = new FormData();
      if (!this.username) {
        console.error('Username is null or empty');
        return;
      }

      formData.append('username', this.username);
      formData.append('Avatar', file);

      try {
        const response = await this.userService.changeAvatar(this.username, formData);
        console.log('Avatar updated successfully:', response);
        this.avatarUrl = response?.AvatarUrl + `?t=${new Date().getTime()}`;
        window.location.reload();
      } catch (error) {
        console.error('Error updating avatar:', error);
        alert('Failed to upload avatar. Please try again.');
      }
    } else {
      console.error('No file selected');
      alert('Please select a file to upload.');
    }
  }
  async changePassword(): Promise<void> {
    if (this.oldPassword == null || this.newPassword == null || this.newPasswordConfirm == null) {
      alert("Les champs sont vides")
    }
    // Vérification si l'ancien mot de passe est le même que le nouveau
    if (this.oldPassword === this.newPassword) {
      alert("Le nouveau mot de passe doit être différent de l'ancien.");
      return;
    }
    // Vérification de la confirmation du nouveau mot de passe
    if (this.newPassword !== this.newPasswordConfirm) {
      alert("Les nouveaux mots de passe ne correspondent pas.");
      return;
    }
    //Créer un objet FormData
    const formData = new FormData();
    formData.append('oldPassword', this.oldPassword);
    formData.append('newPassword', this.newPassword);
    formData.append('newPasswordConfirm', this.newPasswordConfirm);

    try {
      let x = await this.userService.changePassword(formData)
      console.log(x)
      alert("Mot de passe modifié avec succès !");
    } catch (error) {
      alert("Erreur lors du changement du mot de passe. Essayez à nouveau.");
    }

  }
  async makeModerator(): Promise<void> {
    if (!this.isAdmin) {
      alert("Vous devez être administrateur pour effectuer cette action.");
      return;
    }

    if (!this.modUsername) {
      console.error('Username is null or empty');
      return;
    }

    try {
      let x = await this.userService.makeModerator(this.modUsername)
      console.log(x)
      alert(`${this.modUsername} est maintenant modérateur !`);
    } catch (error) {
      console.error("Erreur lors du changement de rôle:", error);
      alert("Erreur lors du changement du rôle. Essayez à nouveau.");
    }
  }
}