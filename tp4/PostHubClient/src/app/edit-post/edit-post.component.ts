import { Component, ElementRef, ViewChild } from '@angular/core';
import { Post } from '../models/post';
import { HubService } from '../services/hub.service';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { Hub } from '../models/hub';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-edit-post',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './edit-post.component.html',
  styleUrl: './edit-post.component.css'
})
export class EditPostComponent {
  hub : Hub | null = null;
  postTitle : string = "";
  postText : string = "";
  @ViewChild("MyPicture", {static: false}) pictureInput ?: ElementRef;
  constructor(public hubService : HubService, public route : ActivatedRoute, public postService : PostService, public router : Router) { }

  async ngOnInit() {
    let hubId : string | null = this.route.snapshot.paramMap.get("hubId");

    if(hubId != null){
      this.hub = await this.hubService.getHub(+hubId);
    }
  }

  // Créer un nouveau post (et son commentaire principal)
  async createPost(){
    if(this.postTitle == "" || this.postText == ""){
      alert("Remplis mieux le titre et le texte niochon");
      return;
    }
    if(this.hub == null) return;


    if (this.pictureInput === undefined) {
      console.log("Input HTML non chargé.");
      return;
    }
  
    // Get the selected file from the input element
    let index = 0
    let file = this.pictureInput.nativeElement.files[0];

  
    // Check if no file was selected
    if (file == null) {
      console.log("Input HTML ne contient aucune image.");
    }
  
    // Prepare the form data with the file to upload
    let formData = new FormData();
    formData.append("title",this.postTitle)
    formData.append("text",this.postText)

    while( file != null){
      formData.append("monImage"+index, file, file.name);
      index++
      file = this.pictureInput.nativeElement.files[index];
    }

    let newPost : Post = await this.postService.postPost(this.hub.id, formData);

    // On se déplace vers le nouveau post une fois qu'il est créé
    this.router.navigate(["/post", newPost.id]);
  }
}
