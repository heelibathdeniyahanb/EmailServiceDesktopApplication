using EmailServiceApplication.Data;
using EmailServiceApplication.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace EmailServiceApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {

        private readonly DatabaseContext _context;

        public EmailController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromForm] string senderEmail, [FromForm] string subject, [FromForm] string body, [FromForm] string[] recipients, [FromForm] List<IFormFile> attachments)
        {
            if (string.IsNullOrWhiteSpace(senderEmail)) return BadRequest("senderEmail is required");
            if (recipients == null || recipients.Length == 0) return BadRequest("At least one recipient is required");

            // find sender user
            var senderUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == senderEmail);
            if (senderUser == null) return BadRequest("Sender not found in users");

            // resolve recipient users
            var recipientUsers = new List<User>();
            foreach (var r in recipients)
            {
                var u = await _context.Users.SingleOrDefaultAsync(x => x.Email == r);
                if (u != null) recipientUsers.Add(u);
            }

            if (!recipientUsers.Any()) return BadRequest("No valid recipients found in users");

            // build Mime message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(senderEmail));
            foreach (var r in recipients)
            {
                emailMessage.To.Add(MailboxAddress.Parse(r));
            }
            emailMessage.Subject = subject;
            var builder = new BodyBuilder { HtmlBody = body };

            // prepare domain Email entity
            var emailModel = new Email
            {
                SenderId = senderUser.Id,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
                IsDeleted = false,
                Attachments = new List<Models.Attachment>(),
                Recipients = new List<EmailRecipient>()
            };

            // handle attachments: save to wwwroot/uploads and attach to mail and model
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsRoot);

            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file.Length <= 0) continue;
                    var safeFileName = Path.GetRandomFileName() + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsRoot, safeFileName);
                    await using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // attach to mime message
                    builder.Attachments.Add(file.FileName, System.IO.File.ReadAllBytes(filePath));

                    // add to DB model
                    emailModel.Attachments.Add(new Models.Attachment
                    {
                        FileName = file.FileName,
                        FilePath = Path.Combine("uploads", safeFileName).Replace('\\', '/'),
                        FileSize = file.Length
                    });
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            // create EmailRecipient entries
            foreach (var ru in recipientUsers)
            {
                emailModel.Recipients.Add(new EmailRecipient
                {
                    UserId = ru.Id
                });
            }

            // persist email record before sending
            _context.Emails.Add(emailModel);
            await _context.SaveChangesAsync();

            // send email using MailKit asynchronously
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            // TODO: don't hardcode credentials; store them securely (User Secrets / environment variables)
            // If you want to use the sender's credentials, fetch them securely.
            var smtpUser = "crmitfac@gmail.com";
            var smtpPass = "vswp acpu deyy vrhx"; // replace with secure retrieval
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);

            return Ok(new { message = "Email sent and recorded", emailId = emailModel.Id });
        }

        // GET: api/email
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Attachments)
                .Include(e => e.Recipients).ThenInclude(r => r.User)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.SentAt)
                .Select(e => new
                {
                    e.Id,
                    Sender = new { e.Sender.Id, e.Sender.Username, e.Sender.Email, e.Sender.FullName },
                    e.Subject,
                    e.Body,
                    e.SentAt,
                    Attachments = e.Attachments.Select(a => new { a.Id, a.FileName, a.FilePath, a.FileSize }),
                    Recipients = e.Recipients.Select(r => new { r.Id, r.UserId, User = r.User != null ? new { r.User.Id, r.User.Username, r.User.Email } : null, r.IsRead, r.ReadAt })
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/email/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _context.Emails
                .Include(x => x.Sender)
                .Include(x => x.Attachments)
                .Include(x => x.Recipients).ThenInclude(r => r.User)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (e == null) return NotFound();

            var result = new
            {
                e.Id,
                Sender = new { e.Sender.Id, e.Sender.Username, e.Sender.Email, e.Sender.FullName },
                e.Subject,
                e.Body,
                e.SentAt,
                Attachments = e.Attachments.Select(a => new { a.Id, a.FileName, a.FilePath, a.FileSize }),
                Recipients = e.Recipients.Select(r => new { r.Id, r.UserId, User = r.User != null ? new { r.User.Id, r.User.Username, r.User.Email } : null, r.IsRead, r.ReadAt })
            };

            return Ok(result);
        }

        // GET: api/email/user/{userId}/inbox
        [HttpGet("user/{userId:int}/inbox")]
        public async Task<IActionResult> GetInbox(int userId)
        {
            var list = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Attachments)
                .Include(e => e.Recipients).ThenInclude(r => r.User)
                .Where(e => !e.IsDeleted && e.Recipients.Any(r => r.UserId == userId))
                .OrderByDescending(e => e.SentAt)
                .Select(e => new
                {
                    e.Id,
                    Sender = new { e.Sender.Id, e.Sender.Username, e.Sender.Email, e.Sender.FullName },
                    e.Subject,
                    e.Body,
                    e.SentAt,
                    Attachments = e.Attachments.Select(a => new { a.Id, a.FileName, a.FilePath, a.FileSize }),
                    RecipientInfo = e.Recipients.Where(r => r.UserId == userId).Select(r => new { r.Id, r.IsRead, r.ReadAt })
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/email/user/{userId}/sent
        [HttpGet("user/{userId:int}/sent")]
        public async Task<IActionResult> GetSent(int userId)
        {
            var list = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Attachments)
                .Include(e => e.Recipients).ThenInclude(r => r.User)
                .Where(e => !e.IsDeleted && e.SenderId == userId)
                .OrderByDescending(e => e.SentAt)
                .Select(e => new
                {
                    e.Id,
                    e.Subject,
                    e.Body,
                    e.SentAt,
                    Attachments = e.Attachments.Select(a => new { a.Id, a.FileName, a.FilePath, a.FileSize }),
                    Recipients = e.Recipients.Select(r => new { r.Id, r.UserId, User = r.User != null ? new { r.User.Id, r.User.Username, r.User.Email } : null, r.IsRead })
                })
                .ToListAsync();

            return Ok(list);
        }

        // DELETE: api/email/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _context.Emails.FindAsync(id);
            if (e == null) return NotFound();
            e.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
