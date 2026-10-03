using System.ComponentModel.DataAnnotations;
using Spleet.Models;

namespace Spleet.ViewModels
{
    public class ExpenseCreateViewModel
    {
        public Guid GroupId { get; set; }

        [Required, StringLength(150)]
        public string Description { get; set; } = "";

        [Range(0.01, 100000000)]
        public decimal Amount { get; set; }

        [StringLength(30)]
        public string Category { get; set; } = "general";

        [Required]
        public Guid PaidByUserId { get; set; }

        public SplitType SplitType { get; set; } = SplitType.Equal;

        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        public List<Guid> SelectedUserIds { get; set; } = new();

        public Dictionary<Guid, decimal> CustomShares { get; set; } = new();

        public List<MemberOptionViewModel> Members { get; set; } = new();
    }

    public class ExpenseEditViewModel : ExpenseCreateViewModel
    {
        public Guid Id { get; set; }
    }

    public class MemberOptionViewModel
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
