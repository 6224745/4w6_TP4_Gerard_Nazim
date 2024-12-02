namespace PostHubServer.Models.DTOs
{
    /// <summary>
    /// Objet de transfert de données qui permet la création d'un Post et de son commentaire principal
    /// </summary>
    public class PostDTO
    {
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}
