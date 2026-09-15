using System.ComponentModel.DataAnnotations;

namespace Spleet.Models
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(30)]
        public string GroupType { get; set; } = "other";

        [Required]
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsArchived { get; set; } = false;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
        public ICollection<ActivityLogEntry> ActivityLog { get; set; } = new List<ActivityLogEntry>();
        public ICollection<GroupInvite> Invites { get; set; } = new List<GroupInvite>();
    }
}