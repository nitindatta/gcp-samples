using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Gcp.Entities;

namespace Demo.Gcp.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactions();
        Task<Transaction> AddTransaction(Transaction transaction);
        public Task<string> SendMessage(string messageBody);
 

    }
}