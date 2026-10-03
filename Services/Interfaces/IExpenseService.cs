using Spleet.Models;
using Spleet.ViewModels;

namespace Spleet.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<bool> IsMemberAsync(Guid groupId, Guid userId);
        Task<Group?> GetGroupAsync(Guid groupId);
        Task<List<Expense>> GetGroupExpensesAsync(Guid groupId);
        Task<Expense?> GetDetailsAsync(Guid expenseId);
        Task<ExpenseCreateViewModel?> GetCreateDataAsync(Guid groupId, Guid currentUserId);
        Task<ExpenseEditViewModel?> GetEditDataAsync(Guid expenseId, Guid currentUserId);
        Task<Guid> CreateAsync(ExpenseCreateViewModel model, Guid currentUserId);
        Task UpdateAsync(ExpenseEditViewModel model, Guid currentUserId);
        Task SoftDeleteAsync(Guid expenseId, Guid currentUserId);
        Task<Guid?> GetGroupIdAsync(Guid expenseId);
    }
}
