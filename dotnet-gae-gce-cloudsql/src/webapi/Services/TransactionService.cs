using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Gcp.Entities;
using Demo.Gcp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Google.Cloud.Tasks.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
//Create One Instance per request , it is not thread safe
namespace Demo.Gcp.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAsyncRepository<Transaction> TransactionRepository;
        private readonly IConfiguration Configuration;


        public TransactionService(IAsyncRepository<Transaction> transactionRepository,
        IConfiguration configuration)
        {
            TransactionRepository = transactionRepository;
            Configuration = configuration;
        }

        public async Task<Transaction> AddTransaction(Transaction transaction)
        {
            
            return await TransactionRepository.AddAsync(transaction);
        }      

        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            return await TransactionRepository.ListAllAsync();
        }
        // Method to put a message on a queue
        // Could be expanded to include message attributes, etc., in a SendMessageRequest
        public async Task<string> SendMessage(string messageBody)
        {
            //Console.WriteLine($"Send message to queue\n  {Configuration.GetValue<string>("sqsqueue")}"); 
            Console.WriteLine($"Send message to queue\n  {Configuration.GetValue<string>("gqueue")}"); 
            var projectId=Configuration.GetValue<string>("projectid");
            var location=Configuration.GetValue<string>("location");
            var queue=Configuration.GetValue<string>("gqueue");
            
            Console.WriteLine(messageBody);
            
            CloudTasksClient client = CloudTasksClient.Create();
            QueueName parent = new QueueName(projectId, location, queue);

            var response = client.CreateTask(new CreateTaskRequest
            {
                Parent = parent.ToString(),
                Task = new Google.Cloud.Tasks.V2.Task
                {
                    AppEngineHttpRequest = new AppEngineHttpRequest
                    {                        
                        HttpMethod = HttpMethod.Post,
                        RelativeUri = "/sendtransaction",                        
                        Body = ByteString.CopyFromUtf8(messageBody),
                    
                    },
                    ScheduleTime = Timestamp.FromDateTime(
                        DateTime.UtcNow.AddSeconds(5))
                }
            });
            
            Console.WriteLine($"Created Task {response.Name}");
            return response.Name;

            
        }
    }
}