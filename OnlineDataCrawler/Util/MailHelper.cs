using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Net;
using OnlineDataCrawler.Engine;

namespace OnlineDataCrawler.Util
{
    public class MailHelper
    {
        private static string smtpServer;

        public static string EmailAddress
        {
            get;
            set;
        }

        public static string EmailPassword
        {
            get;
            set;
        }

        static MailHelper()
        {
            smtpServer = Config.Get("emailServer");
        }
         
        public static void SendMail(string destination, string subject, string content, string attachment)
        {
            MailMessage mail = new MailMessage(new MailAddress(EmailAddress), new MailAddress(destination));
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Host = smtpServer;
            client.Credentials = new NetworkCredential(EmailAddress, EmailPassword);
            mail.Subject = subject;
            mail.Body = content;
            mail.Attachments.Add(new Attachment(attachment));
            client.Send(mail);
        }
    }
}
