import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { HubService } from './services/hub.service';
import { Hub } from './models/hub';
import { faChevronDown, faChevronUp, faMagnifyingGlass, faRightFromBracket, faRightToBracket, faStar } from '@fortawesome/free-solid-svg-icons';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, CommonModule, FontAwesomeModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {

  faMagnifyingGlass = faMagnifyingGlass;
  faStar = faStar;
  faRightToBracket = faRightToBracket;
  faRightFromBracket = faRightFromBracket;
  faChevronDown = faChevronDown;

  searchText : string = "";

  hubsToggled : boolean = false;
  hubList : Hub[] = [];

  constructor(public hubService : HubService){}

  async toggleHubs(){
    this.faChevronDown = this.faChevronDown == faChevronDown ? faChevronUp : faChevronDown;
    this.hubsToggled = !this.hubsToggled;

    if(this.hubsToggled && localStorage.getItem("token") != null){
      let jsonHubs : string | null = localStorage.getItem("myHubs");
      if(jsonHubs != null) this.hubList = JSON.parse(jsonHubs);
    }
  }

  logout(){
    localStorage.clear();
    location.reload();
  }
  
}
