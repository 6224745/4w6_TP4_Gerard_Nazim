using PostHubServer.Data;
using PostHubServer.Models;

namespace PostHubServer.Services
{
    public class CommentService
    {
        private readonly PostHubContext _context;

        public CommentService(PostHubContext context)
        {
            _context = context;
        }

        // Obtenir un commentaire spécifique par son id
        public async Task<Comment?> GetComment(int id)
        {
            if (IsContextNull()) return null;

            return await _context.Comments.FindAsync(id);
        }

        // Créer un commentaire (possiblement le commentaire principal d'un post, mais pas forcément)
        // Un commentaire parent peut être fourni si le commentaire créé est un sous-commentaire
        public async Task<Comment?> CreateComment(User user, string text, Comment? parentComment)
        {
            if (IsContextNull()) return null;

            Comment newComment = new Comment()
            {
                Id = 0,
                Text = text,
                Date = DateTime.UtcNow,
                User = user, // Auteur
                ParentComment = parentComment, // null si commentaire principal du post
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return newComment;
        }

        // Modifier le texte d'un commentaire
        public async Task<Comment?> EditComment(Comment comment, string text)
        {
            comment.Text = text;
            await _context.SaveChangesAsync();

            return comment;
        }

        // Supprimer un commentaire de manière « soft », c'est-à-dire que le
        // commentaire continue d'exister, mais sans texte, ni auteur, ni upvote, ni downvote.
        // Cela permet de préserver ses sous-commentaires dans la discussion.
        public async Task<Comment> SoftDeleteComment(Comment deletedComment)
        {
            deletedComment.Text = "Commentaire supprimé.";
            deletedComment.User = null;
            deletedComment.Upvoters ??= new List<User>();
            deletedComment.Downvoters ??= new List<User>();
            foreach (User u in deletedComment.Upvoters)
            {
                u.Upvotes?.Remove(deletedComment);
            }
            foreach (User u in deletedComment.Downvoters)
            {
                u.Downvotes?.Remove(deletedComment);
            }
            deletedComment.Upvoters = new List<User>();
            deletedComment.Downvoters = new List<User>();
            await _context.SaveChangesAsync();
            return deletedComment;
        }

        // Supprime un commentaire totalement. Possible lorsqu'un commentaire
        // n'a aucun sous-commentaire.
        public async Task<Comment?> HardDeleteComment(Comment deletedComment)
        {
            deletedComment.SubComments ??= new List<Comment>();

            for (int i = deletedComment.SubComments.Count - 1; i >= 0; i--)
            {
                Comment? deletedSubComment = await HardDeleteComment(deletedComment.SubComments[i]);
                if (deletedSubComment == null) return null;
            }

            _context.Comments.Remove(deletedComment);
            await _context.SaveChangesAsync();
            return deletedComment;
        }

        // Permet à un utilisateur d'upvote (ou d'annuler un upvote) un commentaire
        public async Task<bool> UpvoteComment(int id, User user)
        {
            if (IsContextNull()) return false;

            Comment? comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.User == null) return false;

            comment.Upvoters ??= new List<User>();
            comment.Downvoters ??= new List<User>();

            if (comment.Upvoters.Contains(user)) comment.Upvoters.Remove(user);
            else
            {
                comment.Upvoters.Add(user);
                comment.Downvoters.Remove(user);
            }
            await _context.SaveChangesAsync();

            return true; // Basculement du upvote réussi
        }

        // Permet à un utilisateur de downvote (ou d'annuler un downvote) un commentaire
        public async Task<bool> DownvoteComment(int id, User user)
        {
            if (IsContextNull()) return false;

            Comment? comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.User == null) return false;

            comment.Upvoters ??= new List<User>();
            comment.Downvoters ??= new List<User>();

            if (comment.Downvoters.Contains(user)) comment.Downvoters.Remove(user);
            else
            {
                comment.Downvoters.Add(user);
                comment.Upvoters.Remove(user);
            }

            await _context.SaveChangesAsync();

            return true; // Basculement du downvote réussi
        }

        private bool IsContextNull() => _context == null || _context.Comments == null;
    }
}
