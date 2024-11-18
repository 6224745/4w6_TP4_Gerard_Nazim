import { Component } from '@angular/core';
import { CommentService } from '../services/comment.service';
import { Comment } from '../models/comment';
import { CommentComponent } from '../comment/comment.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-moderate-comments',
  standalone: true,
  imports: [CommentComponent, CommonModule],
  templateUrl: './moderate-comments.component.html',
  styleUrl: './moderate-comments.component.css'
})
export class ModerateCommentsComponent {
  commentList : Comment[] = [];

  constructor(public commentService : CommentService) { }

  async ngOnInit() {
    // On doit remplir la liste commentList ici avec tous les commentaires signalés !
  }

  async deleteComment(comment : Comment){
    await this.commentService.deleteComment(comment.id);
    for(let i = this.commentList.length - 1; i >= 0; i--){
      if(this.commentList[i].id == comment.id){
        this.commentList.splice(i, 1);
      }
    }
  }
}
