using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Demo.Gcp.Entities;
using Demo.Gcp.Infrastructure.Data;
using Demo.Gcp.Services.Interfaces;
using Demo.Gcp.Services;

//using Amazon;
//using Amazon.SecretsManager;
//using Amazon.SecretsManager.Model;
using Google.Cloud.SecretManager.V1;

using System;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Demo.Gcp
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration,IWebHostEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddControllers();

            services
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped(typeof(IAsyncRepository<>), typeof(Repository<>));
                //.AddSingleton<IAmazonSQS, AmazonSQSClient>();

            if (HostingEnvironment.IsDevelopment())
            {
                services.AddDbContext<TransactionContext>(c =>
                    c.UseSqlServer(Configuration.GetConnectionString("DemoDb")),
                    ServiceLifetime.Scoped);
                Console.WriteLine("inside development");
            }
            else {
                // Example of how to set credentials security from Secrets Manager
                SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder(Configuration.GetConnectionString("DemoDb"));
                //TO DO : done getting credentials from secret manager
                var dbParam = JsonSerializer.Deserialize<DBSecret>(GetSecret());
                builder.UserID=dbParam.username;
                builder.Password=dbParam.password;
                //builder.Encrypt=true;
                //Oh What are you doing after securiting writing it in logs
                //very bad never do it :)
                Console.WriteLine("connection string (very bad code ugh.....):" + builder.ConnectionString);
                services.AddDbContext<TransactionContext>(c =>
                    c.UseSqlServer(builder.ConnectionString),
                    ServiceLifetime.Scoped);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
        IWebHostEnvironment env,
        TransactionContext transactionContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            //Pick from config :)
            //AWSConfigs.AWSRegion = Configuration.GetValue<string>("AWSRegion");

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapGet("/tasks/summary", async context =>
                {
                    Console.WriteLine($"Scheduled task");
                    await context.Response.WriteAsync("Scheduled Job Process");
                    
                });
                endpoints.MapPost("/sendtransaction", context =>
                {
                    // Log the request payload
                    
                    var reader = new StreamReader(context.Request.Body);
                    var task = reader.ReadToEndAsync();
                
                    Console.WriteLine($"Received task with payload: {task.Result}");
                    return context.Response.WriteAsync($"Printed task payload: {task.Result}");
                });

                endpoints.MapControllers();
            });
            transactionContext.Database.Migrate();
        }
        public string GetSecret()
        {
            string secretName = "SqlUserSecret";
            //string region = Configuration.GetValue<string>("AWSRegion");
            var projectId=Configuration.GetValue<string>("projectid");

            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            // Build the resource name.
            SecretVersionName secretVersionName = new SecretVersionName(projectId, secretName, "latest");

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();
            return payload;
            // Your code goes here.
        }
    }
    class DBSecret
    {
        public string username {get; set;}
        public string password {get; set;}
    }
}
