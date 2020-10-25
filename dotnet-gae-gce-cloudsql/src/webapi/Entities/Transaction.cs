#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Demo.Gcp.Entities
{
    public class Transaction : BaseEntity
    {
        [Key]
        public int TransactionId { get; set; }
        public string? Name { get; set; }
    
    }
}