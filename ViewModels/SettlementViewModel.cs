
using Spleet.Models;

namespace Spleet.ViewModels
{
    public class DebtSuggestionViewModel
    {
        public Guid PayerUserId { get; set; }
        public Guid PayeeUserId { get; set; }

        public string PayerName { get; set; } = "";
        public string PayeeName { get; set; } = "";

        public decimal Amount { get; set; }
    }

    public class SettlementViewModel
    {
        public Guid GroupId { get; set; }
        public Guid CurrentUserId { get; set; }

        public List<DebtSuggestionViewModel> Suggestions { get; set; } = new();

        // Payments awaiting confirmation or dispute resolution.
        public List<Settlement> PendingSettlements { get; set; } = new();

        // Completed and disputed payments for the settlement history.
        public List<Settlement> SettlementHistory { get; set; } = new();
    }
}
