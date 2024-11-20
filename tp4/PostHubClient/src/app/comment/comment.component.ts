import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faL, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { CommentService } from '../services/comment.service';
import { Comment } from '../models/comment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
 
@Component({
  selector: 'app-comment',
  standalone: true,
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.css'] // Corrected to styleUrls
})
export class CommentComponent {
  @ViewChild('fileInput') fileInput: ElementRef | undefined;
  selectedImages: String[] = [];
 
  @Input() comment: Comment | null = null;
 
  // Font Awesome icons
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;
  faImage = faImage;
  faXmark = faXmark;
 
  // Variables to show/hide HTML elements
  replyToggle: boolean = false;
  editToggle: boolean = false;
  repliesToggle: boolean = false;
  isAuthor: boolean = false;
  editMenu: boolean = false;
  displayInputFile: boolean = false;
 
  // Variables associated with inputs
  newComment: string = "";
  editedText?: string;
 
  constructor(public commentService: CommentService) { }
 
  ngOnInit() {
    this.isAuthor = localStorage.getItem("username") == this.comment?.username;
    this.editedText = this.comment?.text;
  }
 
  triggerFileInput(): void {
    this.fileInput?.nativeElement.click();
  }
 
  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.selectedImages.push(e.target.result);
      };
      reader.readAsDataURL(file);
    }
  }
 
  submitComment(): void {
    const formData = new FormData();
    formData.append('text', this.editedText || '');
    if (this.fileInput) {
      for (let i = 0; i < this.fileInput.nativeElement.files.length; i++) {
        formData.append(`image${i}`, this.fileInput.nativeElement.files[i]);
      }
    }
 
    // Send formData to the server
    // this.commentService.createComment(formData).subscribe(response => {
    //   // Handle response
    // });
  }
 
  // Create a new sub-comment
  async createComment() {
    if (this.newComment == "") {
      alert("Écris un commentaire niochon !");
      return;
    }
 
    if (this.comment == null) return;
    if (this.comment.subComments == null) this.comment.subComments = [];
 
    let commentDTO = {
      text: this.newComment
    }
 
    this.comment.subComments.push(await this.commentService.postComment(commentDTO, this.comment.id));
 
    this.replyToggle = false;
    this.repliesToggle = true;
    this.newComment = "";
  }
 
  // Edit a comment
  async editComment() {
    if (this.comment == null || this.editedText == undefined) return;
 
    let commentDTO = {
      text: this.editedText
    }
 
    let newMainComment = await this.commentService.editComment(commentDTO, this.comment.id);
    this.comment = newMainComment;
    this.editedText = this.comment.text;
    this.editMenu = false;
    this.editToggle = false;
  }
 
  // Delete a comment
  async deleteComment() {
    if (this.comment == null || this.editedText == undefined) return;
    await this.commentService.deleteComment(this.comment.id);
 
    // Visual changes for soft-delete
    if (this.comment.subComments != null && this.comment.subComments.length > 0) {
      this.comment.username = null;
      this.comment.upvoted = false;
      this.comment.downvoted = false;
      this.comment.upvotes = 0;
      this.comment.downvotes = 0;
      this.comment.text = "Commentaire supprimé.";
      this.isAuthor = false;
    }
    // Visual changes for hard-delete
    else {
      this.comment = null;
    }
  }
 
  // Upvote a comment
  async upvote() {
    if (this.comment == null) return;
    await this.commentService.upvote(this.comment.id);
 
    // Immediate visual changes
    if (this.comment.upvoted) {
      this.comment.upvotes -= 1;
    } else {
      this.comment.upvotes += 1;
    }
    this.comment.upvoted = !this.comment.upvoted;
    if (this.comment.downvoted) {
      this.comment.downvoted = false;
      this.comment.downvotes -= 1;
    }
  }
 
  // Downvote a comment
  async downvote() {
    if (this.comment == null) return;
    await this.commentService.downvote(this.comment.id);
 
    // Immediate visual changes
    if (this.comment.downvoted) {
      this.comment.downvotes -= 1;
    } else {
      this.comment.downvotes += 1;
    }
    this.comment.downvoted = !this.comment.downvoted;
    if (this.comment.upvoted) {
      this.comment.upvoted = false;
      this.comment.upvotes -= 1;
    }
  }
}