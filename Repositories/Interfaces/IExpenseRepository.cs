using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetForGroup(Guid groupId);

        Task<IEnumerable<Expense>> GetRecentForGroup(
            Guid groupId,
            int count = 5);

        Task<Expense?> GetWithSplits(Guid expenseId);

        Task AddExpense(
            Expense expense,
            List<ExpenseSplit> splits);
    }
}