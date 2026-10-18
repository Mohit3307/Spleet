
using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(SpleetDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> GetForGroup(Guid groupId)
        {
            return await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Splits)
                .Include(e => e.Contributions)
                .Where(e =>
                    e.GroupId == groupId &&
                    !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetRecentForGroup(
            Guid groupId,
            int count = 5)
        {
            return await _context.Expenses
                .Include(e => e.PaidBy)
                .Where(e =>
                    e.GroupId == groupId &&
                    !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Expense?> GetWithSplits(Guid expenseId)
        {
            return await _context.Expenses
                .Include(e => e.Splits)
                    .ThenInclude(s => s.User)
                .Include(e => e.Contributions)
                    .ThenInclude(c => c.User)
                .Include(e => e.PaidBy)
                .FirstOrDefaultAsync(e => e.Id == expenseId);
        }

        public async Task AddExpense(
            Expense expense,
            List<ExpenseSplit> splits)
        {
            expense.Splits = splits;

            await _context.Expenses.AddAsync(expense);
        }
    }
}
