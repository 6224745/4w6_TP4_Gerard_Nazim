namespace PostHubServer.Models
{
    public class Picture
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string GetFullPath(string size)
        {
            return Directory.GetCurrentDirectory() + "/images/" + size + "/" + FileName;
        }
    }
}
