using System.Net.Mail;

namespace EmailServiceApplication.Models
{
    public class Email
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public List<EmailRecipient> Recipients { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }

        public List<Attachment> Attachments { get; set; }
    }
}
