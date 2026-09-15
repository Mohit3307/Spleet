using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spleet.Models
{
    public class RecurringExpenseTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }

        [Required, MaxLength(150)]
        public string Description { get; set; } = "";

        [Required, Range(0.01, 100000000)]
        [Column(TypeName = "decimal(12,2)")]
        public decimal DefaultAmount { get; set; }

        [MaxLength(30)]
        public string Category { get; set; } = "general";

        public SplitType DefaultSplitType { get; set; } = SplitType.Equal;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedAt { get; set; }
    }
}