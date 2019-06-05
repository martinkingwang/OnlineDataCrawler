using System;
using OnlineDataCrawler.Util;

namespace OnlineDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            MailHelper.SendMail("18622608573@163.com", "this is a test", "this is a test");
        }
    }
}
