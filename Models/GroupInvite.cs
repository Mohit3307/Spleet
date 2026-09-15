using System.ComponentModel.DataAnnotations;

namespace Spleet.Models
{
    public class GroupInvite
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        [Required, MaxLength(64)]
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public bool IsRevoked { get; set; } = false;
    }
}