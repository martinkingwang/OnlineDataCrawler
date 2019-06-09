using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OnlineDataCrawler.Engine;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;

namespace OnlineDataCrawler.Logging
{
    public static class Log
    {
        private static ILog log;

        static Log()
        {
            string filePath = Config.Get("LogConfig");
            var assembly = Assembly.GetEntryAssembly();
            var logRepository = LogManager.CreateRepository(assembly, typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(logRepository, new FileInfo(filePath));
            log = LogManager.GetLogger(assembly, typeof(Program));
            var appenders = log4net.LogManager.GetRepository(assembly).GetAppenders();
            foreach (var appender in appenders)
            {
                var rollingFileAppender = appender as log4net.Appender.RollingFileAppender;
                if (rollingFileAppender != null)
                {
                    rollingFileAppender.ImmediateFlush = true;
                    rollingFileAppender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
                    rollingFileAppender.ActivateOptions();
                }
            }
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="error">String Error</param>
        /// <param name="overrideMessageFloodProtection">Force sending a message, overriding the "do not flood" directive</param>
        public static void Error(string error, bool overrideMessageFloodProtection = false)
        {
            log.Error(error);
        }

        /// <summary>
        /// Log error. This overload is usefull when exceptions are being thrown from within an anonymous function.
        /// </summary>
        /// <param name="method">The method identifier to be used</param>
        /// <param name="exception">The exception to be logged</param>
        /// <param name="message">An optional message to be logged, if null/whitespace the messge text will be extracted</param>
        /// <param name="overrideMessageFloodProtection">Force sending a message, overriding the "do not flood" directive</param>
        private static void Error(string method, Exception exception, string message = null, bool overrideMessageFloodProtection = false)
        {
            message = method + "(): " + (message ?? string.Empty) + " " + exception;
            log.Error(message);
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="exception">The exception to be logged</param>
        /// <param name="message">An optional message to be logged, if null/whitespace the messge text will be extracted</param>
        /// <param name="overrideMessageFloodProtection">Force sending a message, overriding the "do not flood" directive</param>
        public static void Error(Exception exception, string message = null, bool overrideMessageFloodProtection = false)
        {
            Error(WhoCalledMe.GetMethodName(1), exception, message, overrideMessageFloodProtection);
        }

        /// <summary>
        /// Log trace
        /// </summary>
        public static void Trace(string traceText, bool overrideMessageFloodProtection = false)
        {
            log.Info(traceText);
        }

        /// <summary>
        /// Writes the message in normal text
        /// </summary>
        public static void Trace(string format, params object[] args)
        {
            Trace(string.Format(format, args));
        }

        /// <summary>
        /// Writes the message in red
        /// </summary>
        public static void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public static void Clear()
        {
            var assembly = Assembly.GetEntryAssembly();
            var appenders = log4net.LogManager.GetRepository(assembly).GetAppenders();
            foreach (var appender in appenders)
            {
                var rollingFileAppender = appender as log4net.Appender.RollingFileAppender;
                if (rollingFileAppender != null)
                {
                    var path = rollingFileAppender.File;
                    File.Delete(path);
                }
            }
        }

        public static string GetLogFilePath()
        {
            var assembly = Assembly.GetEntryAssembly();
            var appenders = log4net.LogManager.GetRepository(assembly).GetAppenders();
            foreach (var appender in appenders)
            {
                var rollingFileAppender = appender as log4net.Appender.RollingFileAppender;
                if (rollingFileAppender != null)
                {
                    var path = rollingFileAppender.File;
                    return path;
                }
            }
            return "";
        }

        /// <summary>
        /// Output to the console, and sleep the thread for a little period to monitor the results.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="level">debug level</param>
        /// <param name="delay"></param>
        public static void Debug(string text, int level = 1, int delay = 0)
        {
            log.Debug(text);
        }

        /// <summary>
        /// C# Equivalent of Print_r in PHP:
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recursion"></param>
        /// <returns></returns>
        public static string VarDump(object obj, int recursion = 0)
        {
            var result = new StringBuilder();

            // Protect the method against endless recursion
            if (recursion < 5)
            {
                // Determine object type
                var t = obj.GetType();

                // Get array with properties for this object
                var properties = t.GetProperties();

                foreach (var property in properties)
                {
                    try
                    {
                        // Get the property value
                        var value = property.GetValue(obj, null);

                        // Create indenting string to put in front of properties of a deeper level
                        // We'll need this when we display the property name and value
                        var indent = String.Empty;
                        var spaces = "|   ";
                        var trail = "|...";

                        if (recursion > 0)
                        {
                            indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();
                        }

                        if (value != null)
                        {
                            // If the value is a string, add quotation marks
                            var displayValue = value.ToString();
                            if (value is string) displayValue = String.Concat('"', displayValue, '"');

                            // Add property name and value to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);

                            try
                            {
                                if (!(value is ICollection))
                                {
                                    // Call var_dump() again to list child properties
                                    // This throws an exception if the current property value
                                    // is of an unsupported type (eg. it has not properties)
                                    result.Append(VarDump(value, recursion + 1));
                                }
                                else
                                {
                                    // 2009-07-29: added support for collections
                                    // The value is a collection (eg. it's an arraylist or generic list)
                                    // so loop through its elements and dump their properties
                                    var elementCount = 0;
                                    foreach (var element in ((ICollection)value))
                                    {
                                        var elementName = String.Format("{0}[{1}]", property.Name, elementCount);
                                        indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();

                                        // Display the collection element name and type
                                        result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());

                                        // Display the child properties
                                        result.Append(VarDump(element, recursion + 2));
                                        elementCount++;
                                    }

                                    result.Append(VarDump(value, recursion + 1));
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            // Add empty (null) property to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
                        }
                    }
                    catch
                    {
                        // Some properties will throw an exception on property.GetValue()
                        // I don't know exactly why this happens, so for now i will ignore them...
                    }
                }
            }

            return result.ToString();
        }
    }
}
