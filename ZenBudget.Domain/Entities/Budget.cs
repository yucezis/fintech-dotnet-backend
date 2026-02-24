using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenBudget.Domain.Entities
{
    public class Budget
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; } 
        public decimal Amount { get; set; } 
        public int Month { get; set; } 
        public int Year { get; set; }

        public User? User { get; set; }
        public Category? Category { get; set; }
    }
}
