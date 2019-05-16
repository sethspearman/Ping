using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace Netalytics.PingHost.Ping.Utilities
{
    public static class ActivityLog
    {

        public enum Category
        {
            Error,
            Information,
            Trace
        };

        public static void Write(string source, Category category, string details)
        {
            try
            {
                var verboseLogging =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["VerboseLogging"] ?? "false");

                // Only log TRACE messages if the EnableVerboseLogging setting is TRUE.
                if (category == Category.Trace && !verboseLogging)
                {
                    return;
                }
                else
                {
                    WriteToFile(details + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured in ActivityLog.Write().  The details are: " + ex.ToString() + "\n\n" +
                            "The intended log entry: " + source + " - " + details);
            }
        }

        private static void WriteToFile(string details)
        {
            try
            {
                string filename = (Assembly.GetExecutingAssembly().CodeBase).Replace("file:///", string.Empty) +
                                  ".errorLog";

                using (FileStream fs = File.Open(filename, FileMode.Append))
                {
                    var entry = Encoding.ASCII.GetBytes(DateTime.Now.ToString() + " - " + details);
                    fs.Write(entry, 0, entry.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}