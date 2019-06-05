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
        private static string from;
        private static string smtpServer;
        private static string emailPassword;

        static MailHelper()
        {
            from = Config.Get("emailAddress");
            smtpServer = Config.Get("emailServer");
            emailPassword = Config.Get("emailPassword");
        }
         
        public static void SendMail(string destination, string subject, string content)
        {
            MailMessage mail = new MailMessage(new MailAddress(from), new MailAddress(destination));
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Host = smtpServer;
            client.Credentials = new NetworkCredential(from, emailPassword);
            mail.Subject = subject;
            mail.Body = content;
            client.Send(mail);
        }
    }
}
