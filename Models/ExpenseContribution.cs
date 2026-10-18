
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spleet.Models
{
    public class ExpenseContribution
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseId { get; set; }
        public Expense? Expense { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [Range(0.01, 100000000)]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
