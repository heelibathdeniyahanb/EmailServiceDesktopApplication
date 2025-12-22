namespace EmailServiceApplication.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        public int EmailId { get; set; }
        public Email Email { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }  // stored in wwwroot/uploads
        public long FileSize { get; set; }
    }
}
