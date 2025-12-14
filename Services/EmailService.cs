using System.Net;
using System.Net.Mail;
using QRCoder;

namespace Kino.Services
{
    public class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "pavlo.zhuk@nure.ua";
        private const string SenderPassword = "qiwq syom buby ssbb";

        public async Task SendTicketAsync(string toEmail, string subject, string body, byte[] pdfBytes)
        {
            using (var mail = new MailMessage())
            using (var client = new SmtpClient(SmtpServer, SmtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);

                mail.From = new MailAddress(SenderEmail, "Kino Theatr Multiplex");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    var stream = new MemoryStream(pdfBytes);
                    var attachment = new Attachment(stream, "Ticket_Kino.pdf", "application/pdf");
                    mail.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mail);
            }
        }
    }
}