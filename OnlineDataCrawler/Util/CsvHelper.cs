using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;


namespace OnlineDataCrawler.Util
{
    public class CsvHelper
    {
        public static void ExportToCsv<T>(List<T> items, string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var writer = File.Create(fileName);
            string result = "";
            var type = typeof(T);
            var assembly = Assembly.GetEntryAssembly();
            Encoding encoding = Encoding.GetEncoding("gb2312");
            foreach(var property in type.GetProperties())
            {
                if (property.PropertyType.Assembly.Equals(assembly))
                {
                    foreach (var subProperty in property.PropertyType.GetProperties())
                    {
                        result += "\"" + subProperty.Name + "\",";
                    }
                }
                else
                {
                    result += "\"" + property.Name + "\",";
                }

            }
            
            result = result.Remove(result.Length - 1, 1);
            result += "\r\n";
            byte[] buffer = encoding.GetBytes(result);
            writer.Write(buffer, 0, buffer.Length);
            
            foreach (var item in items)
            {
                result = "";
                type = item.GetType();
                foreach(var property in type.GetProperties())
                {
                    if (property.PropertyType.Assembly.Equals(assembly))
                    {
                        var subItem = property.GetValue(item);
                        foreach (var subProperty in property.PropertyType.GetProperties())
                        {
                            result += "\"" + subProperty.GetValue(subItem) + "\",";
                        }
                    }
                    else
                    {
                        result += "\"" + property.GetValue(item) + "\",";
                    }
                }
                result = result.Remove(result.Length - 1, 1);
                result += "\r\n";
                byte[] buffer1 = encoding.GetBytes(result);
                writer.Write(buffer1, 0, buffer1.Length);
            }
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
    }
}
