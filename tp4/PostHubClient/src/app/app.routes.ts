import { Routes } from '@angular/router';
import { HubComponent } from './hub/hub.component';
import { ProfileComponent } from './profile/profile.component';
import { RegisterComponent } from './register/register.component';
import { LoginComponent } from './login/login.component';
import { PostComponent } from './post/post.component';
import { EditPostComponent } from './edit-post/edit-post.component';
import { NewHubComponent } from './new-hub/new-hub.component';
import { ModerateCommentsComponent } from './moderate-comments/moderate-comments.component';

export const routes: Routes = [
    {path:"", redirectTo:"/postList/index", pathMatch:"full"},
    {path:"postList", redirectTo:"/postList/index", pathMatch:"full"},
    {path:"postList/:tab", component:HubComponent}, // Route pour accueil / vos hubs
    {path:"postList/hub/:hubId", component:HubComponent}, // Route pour un hub précis
    {path:"postList/search/:searchText", component:HubComponent}, // Router pour une recherche
    {path:"profile", component:ProfileComponent},
    {path:"register", component:RegisterComponent},
    {path:"login", component:LoginComponent},
    {path:"post/:postId", component:PostComponent},
    {path:"editPost/:hubId", component:EditPostComponent},
    {path:"newHub", component:NewHubComponent},
    {path:"reports", component:ModerateCommentsComponent}
];
