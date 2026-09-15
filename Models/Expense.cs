using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spleet.Models
{
    public class Expense
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        [Required, MaxLength(150)]
        public string Description { get; set; } = "";

        [Required, Range(0.01, 100000000)]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [MaxLength(30)]
        public string Category { get; set; } = "general";

        [Required]
        public Guid PaidByUserId { get; set; }
        public User? PaidBy { get; set; }

        public SplitType SplitType { get; set; } = SplitType.Equal;

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid CreatedByUserId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
    }

    public class ExpenseSplit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseId { get; set; }
        public Expense? Expense { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required, Range(0.01, 100000000)]
        [Column(TypeName = "decimal(12,2)")]
        public decimal ShareAmount { get; set; }
    }
}