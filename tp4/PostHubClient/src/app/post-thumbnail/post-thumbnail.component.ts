import { Component, Input } from '@angular/core';
import { Post } from '../models/post';
import { faDownLong, faEllipsis, faMessage, faUpLong } from '@fortawesome/free-solid-svg-icons';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-post-thumbnail',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './post-thumbnail.component.html',
  styleUrl: './post-thumbnail.component.css'
})
export class PostThumbnailComponent {
  @Input() post : Post | null = null;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;

  constructor() { }

  ngOnInit(): void {
    
  }
}
