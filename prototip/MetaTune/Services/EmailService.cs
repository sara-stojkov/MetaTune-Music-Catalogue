using Core.Services.EmailService;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;
using System.Windows;

namespace MetaTune.Services
{
    class EmailService(string server, int port, string email, string password, string displayName) : IEmailService
    {
        private readonly string SERVER = server;
        private readonly int PORT = port;
        private readonly string EMAIL = email;
        private readonly string PASSWORD = password;
        private readonly string DISPLAY_NAME = displayName;
        public void Send(Email email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(DISPLAY_NAME, EMAIL));
            message.To.Add(new MailboxAddress(email.Name, email.Address));
            message.Subject = email.Subject;

            // Always create the text part first
            var textPart = new TextPart("plain")
            {
                Text = email.Content
            };

            if (email.Attachments != null && email.Attachments.Count > 0)
            {
                // If there are attachments, use multipart/mixed
                var multipart = new Multipart("mixed")
                {
                    textPart
                };

                foreach (var attachment in email.Attachments)
                {
                    if (attachment.Content == null || attachment.Content.Length == 0)
                        continue; // skip empty attachments

                    multipart.Add(ToMime(attachment));
                }

                message.Body = multipart;
            }
            else
            {
                // No attachments → just use the text part
                message.Body = textPart;
            }

            try
            {
                using var client = new SmtpClient();
                client.Connect(SERVER, PORT, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(EMAIL, PASSWORD);

                client.Send(message);
                client.Disconnect(true);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                throw new Exception($"Failed to send email. Error details: {ex.Message}", ex);
                

            }
        }
        public static EmailService FromEnv()
        {
            string? emailServer = Environment.GetEnvironmentVariable("EMAIL_SERVER");
            string? emailPortString = Environment.GetEnvironmentVariable("EMAIL_PORT");
            string? emailAddress = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");
            string? emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
            string? emailDisplayName = Environment.GetEnvironmentVariable("EMAIL_DISPLAY_NAME");
            if (String.IsNullOrEmpty(emailServer)) throw new Exception("Unable to load email server");
            if (String.IsNullOrEmpty(emailPortString)) throw new Exception("Unable to load email port");
            if (String.IsNullOrEmpty(emailAddress)) throw new Exception("Unable to load email address");
            if (String.IsNullOrEmpty(emailPassword)) throw new Exception("Unable to load email password");
            if (String.IsNullOrEmpty(emailDisplayName)) throw new Exception("Unable to load email display name");
            if (!int.TryParse(emailPortString, out int emailPort)) throw new Exception("Email port must be a number");
            return new(emailServer, emailPort, emailAddress, emailPassword, emailDisplayName);
        }
        private static MimePart ToMime(Attachment attachment)
        {
            var mimeString = attachment.MimeType.GetString();
            var mimeParts = mimeString.Split('/');

            return new(mimeParts[0], mimeParts[1])
            {
                Content = new MimeContent(new MemoryStream(attachment.Content!)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = attachment.Name
            };

        }
    }
}
