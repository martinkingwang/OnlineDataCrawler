using System;
using OnlineDataCrawler.Util;
using OnlineDataCrawler.Engine;
using System.Text;
using System.Runtime.Loader;

namespace OnlineDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += SigTermEventHandler; //register sigterm event handler. Don't forget to import System.Runtime.Loader!
            Console.CancelKeyPress += CancelHandler; //register sigint event handler
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

        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            System.Console.WriteLine("Unloading...");
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            System.Console.WriteLine("Exiting...");
        }
    }
}
