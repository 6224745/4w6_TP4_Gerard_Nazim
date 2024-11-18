import { Component } from '@angular/core';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  registerUsername : string = "";
  registerEmail : string = "";
  registerPassword : string = "";
  registerPasswordConfirm : string = "";

  constructor(public userService : UserService, public router : Router) { }

  ngOnInit() {}

  async register() : Promise<void>{
    await this.userService.register(this.registerUsername, this.registerEmail, this.registerPassword, this.registerPasswordConfirm);
    this.router.navigate(["/login"]);
  }

}
