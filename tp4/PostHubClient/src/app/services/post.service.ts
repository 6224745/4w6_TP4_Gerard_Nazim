import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Post } from '../models/post';

const domain = "https://localhost:7216/";

@Injectable({
  providedIn: 'root'
})
export class PostService {

  constructor(public http : HttpClient) { }

  // Obtenir une liste de posts en triant par nouveauté / popularité
  async getPostList(tab : string, sorting : string) : Promise<Post[]>{
    let x = await lastValueFrom(this.http.get<Post[]>(domain + "api/Posts/GetPosts/" + tab + "/" + sorting));
    console.log(x);
    return x;
  }

  // Obtenir tous les posts d'un hub triés par nouveauté / popularité
  async getHubPosts(hubId : number, sorting : string) : Promise<Post[]>{
    let x = await lastValueFrom(this.http.get<Post[]>(domain + "api/Posts/GetHubPosts/" + hubId + "/" + sorting));
    console.log(x);
    return x;
  }

  // Recherche des posts avec la barre du header (la phrase utilisée est chercher dans les titres des posts et dans les commentaires principaux des posts)
  async searchPosts(searchText : string, sorting : string) : Promise<Post[]>{
    let x = await lastValueFrom(this.http.get<Post[]>(domain + "api/Posts/SearchPosts/" + searchText + "/" + sorting));
    console.log(x);
    return x;
  }

  // Créer un post
  async postPost(hubId : number, formData : any) : Promise<Post>{
   
    let x = await lastValueFrom(this.http.post<any>(domain + "api/Posts/PostPost/" + hubId, formData));
    console.log(x);
    return x;
  }

  // Obtenir un post précis et tous ses commentaires classés par nouveauté / popularité
  async getPost(postId : number, sorting : string) : Promise<Post>{
    let x = await lastValueFrom(this.http.get<Post>(domain + "api/Posts/GetFullPost/" + postId + "/" + sorting));
    console.log(x);
    return x;
  }

}
