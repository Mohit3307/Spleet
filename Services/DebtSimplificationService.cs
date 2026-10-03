using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Services.Interfaces;
using Spleet.ViewModels;

namespace Spleet.Services
{
    public class DebtSimplificationService : IDebtSimplificationService
    {
        private readonly SpleetDbContext _context;

        public DebtSimplificationService(SpleetDbContext context)
        {
            _context = context;
        }

        public async Task<List<DebtSuggestionViewModel>> GetSuggestionsAsync(Guid groupId)
        {
            var balances = await CalculateBalancesAsync(groupId);

            var creditors = balances
                .Where(x => x.Value > 0.005m)
                .Select(x => new BalanceEntry(x.Key, x.Value))
                .OrderByDescending(x => x.Amount)
                .ToList();

            var debtors = balances
                .Where(x => x.Value < -0.005m)
                .Select(x => new BalanceEntry(x.Key, -x.Value))
                .OrderByDescending(x => x.Amount)
                .ToList();

            var result = new List<DebtSuggestionViewModel>();
            var memberNames = await _context.Users
                .Where(u => balances.Keys.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => string.IsNullOrWhiteSpace(u.FullName) ? u.UserName ?? "User" : u.FullName);

            var i = 0;
            var j = 0;

            while (i < debtors.Count && j < creditors.Count)
            {
                var amount = decimal.Round(
                    Math.Min(debtors[i].Amount, creditors[j].Amount), 2);

                if (amount > 0)
                {
                    result.Add(new DebtSuggestionViewModel
                    {
                        PayerUserId = debtors[i].UserId,
                        PayeeUserId = creditors[j].UserId,
                        PayerName = memberNames.GetValueOrDefault(debtors[i].UserId, "User"),
                        PayeeName = memberNames.GetValueOrDefault(creditors[j].UserId, "User"),
                        Amount = amount
                    });
                }

                debtors[i].Amount -= amount;
                creditors[j].Amount -= amount;

                if (debtors[i].Amount <= 0.005m) i++;
                if (creditors[j].Amount <= 0.005m) j++;
            }

            return result;
        }

        private async Task<Dictionary<Guid, decimal>> CalculateBalancesAsync(Guid groupId)
        {
            var balances = await _context.GroupMembers
                .Where(m => m.GroupId == groupId && m.IsActive)
                .Select(m => m.UserId)
                .ToDictionaryAsync(id => id, _ => 0m);

            var expenses = await _context.Expenses
                .Include(e => e.Splits)
                .Where(e => e.GroupId == groupId && !e.IsDeleted)
                .ToListAsync();

            foreach (var expense in expenses)
            {
                if (balances.ContainsKey(expense.PaidByUserId))
                    balances[expense.PaidByUserId] += expense.Amount;

                foreach (var split in expense.Splits)
                {
                    if (balances.ContainsKey(split.UserId))
                        balances[split.UserId] -= split.ShareAmount;
                }
            }

            var settlements = await _context.Settlements
                .Where(s => s.GroupId == groupId && s.Status == SettlementStatus.Completed)
                .ToListAsync();

            foreach (var settlement in settlements)
            {
                if (balances.ContainsKey(settlement.PayerUserId))
                    balances[settlement.PayerUserId] -= settlement.Amount;

                if (balances.ContainsKey(settlement.PayeeUserId))
                    balances[settlement.PayeeUserId] += settlement.Amount;
            }

            return balances;
        }

        private sealed class BalanceEntry
        {
            public BalanceEntry(Guid userId, decimal amount)
            {
                UserId = userId;
                Amount = amount;
            }

            public Guid UserId { get; }
            public decimal Amount { get; set; }
        }
    }
}
