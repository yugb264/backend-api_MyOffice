using System;

namespace backend_api.Models
{
    public class Users
    {
        public int Id { get; set; }

        public string AzureId { get; set; }  
        public string Email { get; set; }
        public string DisplayName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}