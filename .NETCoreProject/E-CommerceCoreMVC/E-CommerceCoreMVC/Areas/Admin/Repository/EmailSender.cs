using System.Net;
using System.Net.Mail;

namespace E_CommerceCoreMVC.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("dnguen82@gmail.com", "injgeqsrnwhlhnyh")//  qtuixzjxgpnetvrq
            };

            return client.SendMailAsync(
                new MailMessage(from: "dnguen82@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}

