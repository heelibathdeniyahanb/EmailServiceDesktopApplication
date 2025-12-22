namespace EmailServiceApplication.Models
{
    public class EmailRecipient
    {
        public int Id { get; set; }

        public int EmailId { get; set; }
        public Email Email { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
