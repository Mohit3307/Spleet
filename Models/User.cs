using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Spleet.Models
{
    public class User : IdentityUser<Guid>
    {
        

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public ICollection<Expense> ExpensesPaid { get; set; } = new List<Expense>();
        public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();
    }
}