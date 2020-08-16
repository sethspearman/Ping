using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Quartz;
using Topshelf;
using Topshelf.Quartz;

namespace SpearSoft.PingHost.Ping
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogger();

            var serviceDisplayName =
                ConfigurationManager.AppSettings["ServiceDisplayName"] ?? "SpearSoft.PingHost.Ping";
            var serviceDescription = ConfigurationManager.AppSettings["ServiceDescription"] ??
                                     "Ping service for keeping websites alive";
            var serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "SpearSoft_PingHost_Ping";
            var pollingInterval = ConfigurationManager.AppSettings["PollingInterval"] ?? "900";

            HostFactory.Run(x =>
            {
                x.Service<ServiceHost>(s =>
                {
                    s.WhenStarted(service => service.OnStart());
                    s.WhenStopped(service => service.OnStop());
                    s.ConstructUsing(() => new ServiceHost());

                    s.ScheduleQuartzJob(q =>
                        q.WithJob(() =>
                                JobBuilder.Create<JobRunner>()
                                    .WithDescription(serviceDescription)
                                    .WithIdentity(serviceName).Build())
                            .AddTrigger(() => TriggerBuilder.Create()
                                .WithSimpleSchedule(b => b
                                    .WithIntervalInSeconds(Convert.ToInt32(pollingInterval))
                                    .RepeatForever())
                                .Build())
                    );
                });

                x.RunAsLocalSystem()
                    .DependsOnEventLog()
                    .StartAutomatically()
                    .EnableServiceRecovery(rc => rc.RestartService(1));
                x.UseLog4Net();
                x.SetServiceName(serviceName);
                x.SetDisplayName(serviceDisplayName);
                x.SetDescription(serviceDescription);
            });


            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            //Console.WriteLine("Hello World!");
            //Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }

        private static void ConfigureLogger()
        {
            const string logConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                    <log4net>
                      <root>
                        <level value=""INFO"" />
                        <appender-ref ref=""console"" />
                      </root>
                      <logger name=""NHibernate"">
                        <level value=""ERROR"" />
                      </logger>
                      <appender name=""console"" type=""log4net.Appender.ColoredConsoleAppender"">
                        <layout type=""log4net.Layout.PatternLayout"">
                          <conversionPattern value=""%m%n"" />
                        </layout>
                      </appender>
                    </log4net>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
            {
                XmlConfigurator.Configure(stream);
            }
        }
    }
}