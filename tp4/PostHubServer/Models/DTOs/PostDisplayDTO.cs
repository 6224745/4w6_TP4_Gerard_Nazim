namespace PostHubServer.Models.DTOs
{
    /// <summary>
    /// Objet de transfert de données qui permet d'envoyer au client un Post prêt à l'affichage.
    /// Utilisez le constructeur, ne remplissez pas les champs vous-mêmes !
    /// </summary>
    public class PostDisplayDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int HubId { get; set; }
        public string HubName { get; set; } = null!;
        public CommentDisplayDTO MainComment { get; set; } = null!;


        public PostDisplayDTO() { }
        public PostDisplayDTO(Post post, bool withSubComments, User? user)
        {
            Id = post.Id;
            Title = post.Title;
            MainComment = new CommentDisplayDTO(post.MainComment!, withSubComments, user);
            HubName = post.Hub!.Name;
            HubId = post.Hub!.Id;
        }
    }
}
