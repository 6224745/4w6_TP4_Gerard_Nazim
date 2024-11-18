namespace PostHubServer.Models.DTOs
{
    /// <summary>
    /// Objet de transfert de données qui permet d'envoyer un commentaire prêt à l'affichage à un client.
    /// Les listes d'upvoters et de downvoters sont convertis en simples nombres de vote. La référence
    /// vers l'utilisateur est transformée en simple pseudo. Utilisez le constructeur, ne remplissez pas
    /// les champs vous-mêmes !
    /// </summary>
    public class CommentDisplayDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime? Date { get; set; }
        public string? Username { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public bool Upvoted { get; set; }
        public bool Downvoted { get; set; }
        public int SubCommentTotal { get; set; }
        public List<CommentDisplayDTO>? SubComments { get; set; }

        public CommentDisplayDTO() { }
        public CommentDisplayDTO(Comment comment, bool withSubComments, User? user)
        {
            List<CommentDisplayDTO>? subComments = null;
            if (withSubComments) subComments = comment.SubComments?.Select(c => new CommentDisplayDTO(c, true, user)).ToList();

            Id = comment.Id;
            Text = comment.Text;
            Date = comment.Date;
            Username = comment.User?.UserName;
            Upvotes = comment.Upvoters?.Count ?? 0;
            Downvotes = comment.Downvoters?.Count ?? 0;
            Upvoted = user != null && (comment.Upvoters?.Contains(user) ?? false);
            Downvoted = user != null && (comment.Downvoters?.Contains(user) ?? false);
            SubCommentTotal = comment.GetSubCommentTotal();
            SubComments = subComments;
        }
    }
}
