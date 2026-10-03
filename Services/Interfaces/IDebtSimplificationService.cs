using Spleet.ViewModels;

namespace Spleet.Services.Interfaces
{
    public interface IDebtSimplificationService
    {
        Task<List<DebtSuggestionViewModel>> GetSuggestionsAsync(Guid groupId);
    }
}
