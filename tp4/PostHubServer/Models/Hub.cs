using System.Text.Json.Serialization;

namespace PostHubServer.Models
{
    public class Hub
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public virtual List<Post> Posts { get; set; } = new List<Post>();

        [JsonIgnore]
        public virtual List<User> Users { get; set; } = new List<User>();
    }
}
