using System;
using OnlineDataCrawler.Util;
using OnlineDataCrawler.Engine;
using OnlineDataCrawler.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;

namespace OnlineDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 20;
            int[] max = new int[count];
            StockBasic[] sbs = new StockBasic[count];
            var ds = DataStorage.GetInstance();
            var stocks = ds.GetAllStocks();
            List<StockHistoryPrice> prices = new List<StockHistoryPrice>();
            foreach(var stock in stocks)
            {
                var ps = ds.GetHistoryPrices(stock, new DateTime(2008, 1, 1), new DateTime(2009, 1, 1));
                if(ps != null || ps.Count > 0)
                {
                    bool isMore = false;
                    StockBasic oldStock = new StockBasic();
                    for(int i = 0; i < max.Length; i ++)
                    {
                        if(ps.Count > max[i])
                        {
                            isMore = true;
                            max[i] = ps.Count;
                            oldStock = sbs[i];
                            sbs[i] = stock;
                            break;
                        }
                    }
                    if (isMore)
                    {
                        if (oldStock != null)
                        {
                            var toDelete = from p in prices
                                           where p.Stock.Equals(oldStock)
                                           select p;
                            var listTemp = new List<StockHistoryPrice>(toDelete);
                            foreach (var delete in listTemp)
                            {
                                prices.Remove(delete);
                            }
                        }
                        prices.AddRange(ps);
                    }
                        
                }
            }
            CsvHelper.ExportToCsv(prices, "a.csv");
        }

        static void main1()
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
