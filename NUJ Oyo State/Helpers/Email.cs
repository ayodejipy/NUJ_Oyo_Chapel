using NUJ_Oyo_State.Models.ViewModels.Helpers;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NUJ_Oyo_State.Models.Helpers
{
    public class Email
    {
        public static bool SendMail(EmailMessageVM model)
        {
            try
            {
                // Get gmail username and password
                string senderEmail = System.Configuration.ConfigurationManager.AppSettings["smtpUserEmail"].ToString();
                string senderPassword = System.Configuration.ConfigurationManager.AppSettings["smtpPassword"].ToString();

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                MailMessage mailMessage = new MailMessage(senderEmail, model.ToAddress);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;

                mailMessage.Subject = model.Subject;
                mailMessage.Body = model.Message;
                client.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                return false;

            }
        }
    }
}