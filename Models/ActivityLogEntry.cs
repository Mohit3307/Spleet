using System.ComponentModel.DataAnnotations;

namespace Spleet.Models
{
    public class ActivityLogEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        public ActivityType Type { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }
        public User? Actor { get; set; }

        [Required, MaxLength(300)]
        public string Summary { get; set; } = "";

        public Guid? RelatedEntityId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}