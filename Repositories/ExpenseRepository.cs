using Microsoft.EntityFrameworkCore;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Data;

namespace Spleet.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(SpleetDbContext context) : base(context) { }

        public async Task<IEnumerable<Expense>> GetForGroup(Guid groupId)
        {
            return await _context.Expenses
                .Where(e => e.GroupId == groupId && !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<Expense?> GetWithSplits(Guid expenseId)
        {
            return await _context.Expenses
                .Include(e => e.Splits)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.Id == expenseId);
        }

        public async Task AddExpense(Expense expense, List<ExpenseSplit> splits)
        {
            expense.Splits = splits;
            await _context.Expenses.AddAsync(expense);
        }
    }
}