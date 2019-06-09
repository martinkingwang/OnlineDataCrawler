using System;
using OnlineDataCrawler.Util;
using OnlineDataCrawler.Engine;
using System.Text;

namespace OnlineDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Logging.Log.Trace("Program start.");
            Console.WriteLine("Please input your Email address.");
            MailHelper.EmailAddress = Console.ReadLine();
            Console.WriteLine("Please input your Email password.");
            MailHelper.EmailPassword = Console.ReadLine();
            MainLoop loop = new MainLoop();
            loop.Init();
            loop.Loop();
        }
    }
}
