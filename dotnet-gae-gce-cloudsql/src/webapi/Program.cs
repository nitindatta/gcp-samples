using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore;

namespace Demo.Gcp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)  
                .ConfigureAppConfiguration((hostincontext,config)=>
                {        
                    Console.WriteLine("ASPNETCORE_ENVIRONMENT:" + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) ;  
                    Console.WriteLine("ConnectionStrings__DemoDb:" + Environment.GetEnvironmentVariable("ConnectionStrings__DemoDb")) ;   
        
                    config.AddJsonFile($"appsettings.json", true, true);
                    config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true);
                    config.AddEnvironmentVariables();
                })          
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseIIS();
                });

                private static string GetProjectId()
                {
                    GoogleCredential googleCredential = Google.Apis.Auth.OAuth2
                        .GoogleCredential.GetApplicationDefault();
                    if (googleCredential != null)
                    {
                        ICredential credential = googleCredential.UnderlyingCredential;
                        ServiceAccountCredential serviceAccountCredential =
                            credential as ServiceAccountCredential;
                        if (serviceAccountCredential != null)
                        {
                            return serviceAccountCredential.ProjectId;
                        }
                    }
                    return Google.Api.Gax.Platform.Instance().ProjectId;
                }
    }
    
}
