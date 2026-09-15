using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spleet.Models
{
    public class GroupMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public GroupRole Role { get; set; } = GroupRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public SplitType LastUsedSplitType { get; set; } = SplitType.Equal;

        [Column(TypeName = "decimal(12,2)")]
        public decimal CurrentBalance { get; set; } = 0m;
    }
}