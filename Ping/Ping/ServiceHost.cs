using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpearSoft.PingHost.Ping;
using Quartz;
using Topshelf.Logging;

namespace SpearSoft.PingHost.Ping
{
    class ServiceHost : IDisposable
    {
        public string LogSource { get; } = ConfigurationManager.AppSettings["LogSource"] ?? "SpearSoft.PingHost.Ping";
        public LogWriter Log { get; } = HostLogger.Get<ServiceHost>();

        private static HttpClient _httpClient;
        private static HttpClientHandler _httpClientHandler;

        private const string MediaTypeJson = "application/json";
        private const string ClientUserAgent = "spearsoft-pinghost-ping-service";

        public string WriteToFile { get; } = ConfigurationManager.AppSettings["WriteToFile"] ?? "false";

        public void OnStart()
        {
            try
            {
                EnsureHttpClientCreated();
                WriteLog(Utilities.ActivityLog.Category.Trace, "Service Starting");
            }
            catch (Exception ex)
            {
                WriteLog(Utilities.ActivityLog.Category.Error, ex.ToString());
                throw;
            }
        }

        public void OnStop()
        {
            try
            {
                WriteLog(Utilities.ActivityLog.Category.Trace, "Service Stopping");
            }
            catch (Exception ex)
            {
                WriteLog(Utilities.ActivityLog.Category.Error, ex.ToString());
                throw;
            }
        }

        private void WriteLog(Utilities.ActivityLog.Category errorLevel, string message)
        {
            if (WriteToFile == "true")
            {
                Utilities.ActivityLog.Write(LogSource, errorLevel, message);
            }

            switch (errorLevel)
            {
                case Utilities.ActivityLog.Category.Error:
                    Log.Info(message);
                    break;

                case Utilities.ActivityLog.Category.Information:
                    Log.Info(message);
                    break;

                case Utilities.ActivityLog.Category.Trace:
                    Log.Info(message);
                    break;
            }
        }

        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
        }

        public void DoTickEvent(JobKey jobDetailKey, string jobDetailDescription)
        {
            //WriteLog(Utilities.ActivityLog.Category.Trace, DateTime.Now.ToString());

            //using (var client = new HttpClient())
            //{
                var endpoint1 = ConfigurationManager.AppSettings["Uri1"] ?? string.Empty;
                var endpoint2 = ConfigurationManager.AppSettings["Uri2"] ?? string.Empty;
                var endpoint3 = ConfigurationManager.AppSettings["Uri3"] ?? string.Empty;
                var endpoint4 = ConfigurationManager.AppSettings["Uri4"] ?? string.Empty;
                var endpoint5 = ConfigurationManager.AppSettings["Uri5"] ?? string.Empty;

                PingEndpoint(endpoint1);
                PingEndpoint(endpoint2);
                PingEndpoint(endpoint3);
                PingEndpoint(endpoint4);
                PingEndpoint(endpoint5);
            //}
        }

        private void PingEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return;
            }

            try
            {
                EnsureHttpClientCreated();
                var response = _httpClient.GetAsync(endpoint).Result;

                WriteLog(Utilities.ActivityLog.Category.Trace, response.IsSuccessStatusCode
                    ? $"Successfully contacted {endpoint}"
                    : $"Error trying to reach {endpoint}:  http response returned an error code ");
            }
            catch (Exception e)
            {
                WriteLog(Utilities.ActivityLog.Category.Error,
                    $"Error trying to reach {endpoint}:  unable to reach endpoint ");
            }
        }
        private void CreateHttpClient()
        {
            _httpClientHandler = new HttpClientHandler();

            _httpClient = new HttpClient(_httpClientHandler, false);

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);
        
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJson));
        }

        private void EnsureHttpClientCreated()
        {
            if (_httpClient == null)
            {
                CreateHttpClient();
            }
        }
    }

    public class JobRunner : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var sh = new ServiceHost();
            sh.DoTickEvent(context.JobDetail.Key, context.JobDetail.Description);
        }
    }
}