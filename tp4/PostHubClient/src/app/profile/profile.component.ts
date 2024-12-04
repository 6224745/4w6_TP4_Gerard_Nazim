import { Component, ElementRef, ViewChild } from '@angular/core';
import { UserService } from '../services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  userIsConnected : boolean = false;
  @ViewChild("myPictureViewchild", {static:false}) pictureInput ?: ElementRef;
  // Vous êtes obligés d'utiliser ces trois propriétés
  oldPassword : string = "";
  newPassword : string = "";
  newPasswordConfirm : string = "";
  username : string | null = null;

  avatarSrc: string | null = null;
  constructor(public userService : UserService) { }

  async ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");
    if (this.username) {
      await this.loadAvatar();
    }
  }
  
  
  async loadAvatar(): Promise<void> {
    if (this.username) {
      try {
        const avatarBlob = await this.userService.getAvatar(this.username);
        this.avatarSrc = URL.createObjectURL(avatarBlob);
      } catch (error) {
        console.log("Aucun avatar disponible pour cet utilisateur.");
      }
    }
  }

  async onAvatarChange(event: any): Promise<void> {
    const file = event.target.files[0];
    if (file && this.username) {
      try {
        await this.userService.changeAvatar(this.username, file);
        console.log("Avatar mis à jour !");
        await this.loadAvatar(); // Recharger l'avatar mis à jour
      } catch (error) {
        console.error("Erreur lors de la modification de l'avatar :", error);
      }
    }
  }
}
  
  