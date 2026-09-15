using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spleet.Models
{
    public class Settlement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        [Required]
        public Guid PayerUserId { get; set; }
        public User? Payer { get; set; }

        [Required]
        public Guid PayeeUserId { get; set; }
        public User? Payee { get; set; }

        [Required, Range(0.01, 100000000)]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        [MaxLength(200)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? AutoConfirmDeadline { get; set; }

        public bool WasSuggested { get; set; } = false;
    }
}