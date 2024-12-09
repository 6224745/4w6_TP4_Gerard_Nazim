import { Comment } from '../models/comment';

export class Post{
    pictureIds: any;
    constructor(
        public id : number,
        public title : string,
        public hubId : number,
        public hubName : string,
        public mainComment : Comment | null,
        public isReported : boolean
    ){}
}