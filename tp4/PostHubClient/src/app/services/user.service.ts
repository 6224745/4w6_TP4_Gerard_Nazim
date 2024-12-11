import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Observable } from 'rxjs';

const domain = "https://localhost:7216/";

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(public http: HttpClient) { }

  private apiUrl = 'http://localhost:7216/api/Users';


  async changeAvatar(username: string, formData: FormData): Promise<any> {
    const url = domain + "api/Users/ChangeAvatar";
    return await lastValueFrom(this.http.put<any>(url, formData));
  }
  async changePassword(formData: FormData): Promise<any> {
    const url = domain + "api/Users/ChangePassword";
    return await lastValueFrom(this.http.put<any>(url, formData));
  }
  async makeModerator(username: string): Promise<any> {
    const url = `${domain}api/Users/MakeModerator/${username}`;
    return await lastValueFrom(this.http.put<any>(url, {}));
  }




  // S'inscrire
  async register(username: string, email: string, password: string, passwordConfirm: string): Promise<void> {

    let registerDTO = {
      username: username,
      email: email,
      password: password,
      passwordConfirm: passwordConfirm
    };

    let x = await lastValueFrom(this.http.post<any>(domain + "api/Users/Register", registerDTO));
    console.log(x);
  }

  // Se connecter
  async login(username: string, password: string): Promise<void> {

    let loginDTO = {
      username: username,
      password: password
    };

    let x = await lastValueFrom(this.http.post<any>(domain + "api/Users/Login", loginDTO));
    console.log(x);

    // N'hésitez pas à ajouter d'autres infos dans le stockage local... 
    // Cela pourrait vous aider pour la partie admin / modérateur
    localStorage.setItem("token", x.token);
    localStorage.setItem("username", x.username);
    localStorage.setItem("role", x.role);

  }

}
