import { Component } from '@angular/core';
import { UserService } from '../services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent {
userIsConnected: boolean = false;
selectedFile: File | null = null;

// Vous êtes obligés d'utiliser ces trois propriétés
oldPassword: string = "";
newPassword: string = "";
newPasswordConfirm: string = "";

username: string | null = null;

constructor(public userService: UserService, public http: HttpClient) { }

ngOnInit() {
  this.userIsConnected = localStorage.getItem("token") != null;
  this.username = localStorage.getItem("username");
}

onFileSelected(event: any) {
  this.selectedFile = event.target.files[0];
}

onUpload() {
  if (this.selectedFile && this.username) {
    const formData = new FormData();
    formData.append('avatar', this.selectedFile, this.selectedFile.name);

    this.http.post<any>(`https://localhost:7216/api/Users/UpdateAvatar/${this.username}/avatar`, formData)
      .subscribe(response => {
        console.log('Avatar uploaded successfully');
      });
  }
}
}