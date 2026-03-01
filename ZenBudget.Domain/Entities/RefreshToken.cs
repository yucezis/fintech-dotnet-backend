using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenBudget.Domain.Entities
{

        public class RefreshToken
        {
            public Guid Id { get; set; }
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public bool IsRevoked { get; set; }
            public Guid UserId { get; set; }
            public User User { get; set; } = null!;
        }
    
}
