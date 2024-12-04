export class Comment{
    constructor(
        public id : number,
        public text : string,
        public date : Date,
        public username : string | null,
        public upvotes : number,
        public downvotes : number,
        public upvoted : boolean,
        public downvoted : boolean,
        public subCommentTotal : number,
<<<<<<< HEAD
        public subComments : Comment[] | null,
        public pictureIds : number[] | null
=======
<<<<<<< HEAD
        public subComments : Comment[] | null,
        public picturesId : number[]
=======
        public subComments : Comment[] | null
>>>>>>> origin/dev
>>>>>>> origin/dev
    ){}
}