
using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Services.Interfaces;
using Spleet.ViewModels;

namespace Spleet.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly SpleetDbContext _context;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _groupMemberRepository;

        public ExpenseService(
            SpleetDbContext context,
            IExpenseRepository expenseRepository,
            IGroupRepository groupRepository,
            IGroupMemberRepository groupMemberRepository)
        {
            _context = context;
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        public Task<bool> IsMemberAsync(Guid groupId, Guid userId)
        {
            return _groupRepository.IsUserMemberAsync(groupId, userId);
        }

        public Task<Group?> GetGroupAsync(Guid groupId)
        {
            return _groupRepository.GetByIdWithMembersAsync(groupId);
        }

        public async Task<List<Expense>> GetGroupExpensesAsync(Guid groupId)
        {
            return (await _expenseRepository.GetForGroup(groupId)).ToList();
        }

        public Task<Expense?> GetDetailsAsync(Guid expenseId)
        {
            return _expenseRepository.GetWithSplits(expenseId);
        }

        public async Task<ExpenseCreateViewModel?> GetCreateDataAsync(
            Guid groupId,
            Guid currentUserId)
        {
            var group = await _groupRepository.GetByIdWithMembersAsync(groupId);

            if (group == null ||
                group.IsArchived ||
                !group.Members.Any(m =>
                    m.UserId == currentUserId && m.IsActive))
            {
                return null;
            }

            return new ExpenseCreateViewModel
            {
                GroupId = groupId,
                PaidByUserId = currentUserId,
                ExpenseDate = DateTime.Today,

                Members = group.Members
                    .Where(m => m.IsActive && m.User != null)
                    .OrderBy(m => m.User!.FullName)
                    .Select(m => new MemberOptionViewModel
                    {
                        UserId = m.UserId,
                        Name = string.IsNullOrWhiteSpace(m.User!.FullName)
                            ? m.User.UserName ?? "User"
                            : m.User.FullName,
                        Email = m.User.Email ?? ""
                    })
                    .ToList(),

                SelectedUserIds = group.Members
                    .Where(m => m.IsActive)
                    .Select(m => m.UserId)
                    .ToList()
            };
        }

        public async Task<ExpenseEditViewModel?> GetEditDataAsync(
            Guid expenseId,
            Guid currentUserId)
        {
            var expense = await _expenseRepository.GetWithSplits(expenseId);

            if (expense == null || expense.IsDeleted)
                return null;

            if (!await IsMemberAsync(expense.GroupId, currentUserId))
                return null;

            var group = await _groupRepository.GetByIdWithMembersAsync(
                expense.GroupId);

            if (group == null)
                return null;

            return new ExpenseEditViewModel
            {
                Id = expense.Id,
                GroupId = expense.GroupId,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                PaidByUserId = expense.PaidByUserId,
                SplitType = expense.SplitType,
                ExpenseDate = expense.ExpenseDate,

                Members = group.Members
                    .Where(m => m.IsActive && m.User != null)
                    .OrderBy(m => m.User!.FullName)
                    .Select(m => new MemberOptionViewModel
                    {
                        UserId = m.UserId,
                        Name = string.IsNullOrWhiteSpace(m.User!.FullName)
                            ? m.User.UserName ?? "User"
                            : m.User.FullName,
                        Email = m.User.Email ?? ""
                    })
                    .ToList(),

                SelectedUserIds = expense.Splits
                    .Select(s => s.UserId)
                    .ToList(),

                CustomShares = expense.Splits
                    .ToDictionary(s => s.UserId, s => s.ShareAmount),

                Contributions = await _context.ExpenseContributions
                    .Where(c => c.ExpenseId == expense.Id)
                    .ToDictionaryAsync(c => c.UserId, c => c.Amount)
            };
        }

        public async Task<Guid> CreateAsync(
            ExpenseCreateViewModel model,
            Guid currentUserId)
        {
            var amount = decimal.Round(model.Amount, 2);

            await ValidateExpenseAsync(
                model.GroupId,
                model.PaidByUserId,
                amount,
                model.SplitType,
                model.SelectedUserIds,
                model.CustomShares,
                currentUserId);

            var contributionAmounts = await ValidateContributionsAsync(
                model.GroupId,
                amount,
                model.Contributions);

            var selectedIds = model.SelectedUserIds!
                .Distinct()
                .ToList();

            var splits = BuildSplits(
                amount,
                model.SplitType,
                selectedIds,
                model.CustomShares);

            var expense = new Expense
            {
                GroupId = model.GroupId,
                Description = model.Description.Trim(),
                Amount = amount,

                Category = string.IsNullOrWhiteSpace(model.Category)
                    ? "general"
                    : model.Category.Trim(),

                PaidByUserId = model.PaidByUserId,
                SplitType = model.SplitType,

                ExpenseDate = model.ExpenseDate == default
                    ? DateTime.UtcNow
                    : model.ExpenseDate,

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId,
                IsDeleted = false,
                Splits = splits,

                Contributions = contributionAmounts
                    .Select(c => new ExpenseContribution
                    {
                        Id = Guid.NewGuid(),
                        UserId = c.Key,
                        Amount = c.Value,
                        Note = null,
                        PaidAt = DateTime.UtcNow
                    })
                    .ToList()
            };

            foreach (var split in splits)
                split.ExpenseId = expense.Id;

            foreach (var contribution in expense.Contributions)
                contribution.ExpenseId = expense.Id;

            await _expenseRepository.AddExpense(expense, splits);
            await _context.SaveChangesAsync();

            return expense.Id;
        }

        public async Task UpdateAsync(
            ExpenseEditViewModel model,
            Guid currentUserId)
        {
            var expense = await _expenseRepository.GetWithSplits(model.Id);

            if (expense == null || expense.IsDeleted)
                throw new ArgumentException("Expense not found.");

            if (!await IsMemberAsync(expense.GroupId, currentUserId))
                throw new UnauthorizedAccessException(
                    "You are not a member of this group.");

            var amount = decimal.Round(model.Amount, 2);

            await ValidateExpenseAsync(
                expense.GroupId,
                model.PaidByUserId,
                amount,
                model.SplitType,
                model.SelectedUserIds,
                model.CustomShares,
                currentUserId);

            var selectedIds = model.SelectedUserIds!
                .Distinct()
                .ToList();

            var newSplits = BuildSplits(
                amount,
                model.SplitType,
                selectedIds,
                model.CustomShares);

            // Preserve existing contributions when editing the expense.
            // Contribution editing will be added separately.

            expense.Description = model.Description.Trim();
            expense.Amount = amount;

            expense.Category = string.IsNullOrWhiteSpace(model.Category)
                ? "general"
                : model.Category.Trim();

            expense.PaidByUserId = model.PaidByUserId;
            expense.SplitType = model.SplitType;
            expense.ExpenseDate = model.ExpenseDate;

            _context.ExpenseSplits.RemoveRange(expense.Splits);
            await _context.SaveChangesAsync();

            foreach (var split in newSplits)
                split.ExpenseId = expense.Id;

            await _context.ExpenseSplits.AddRangeAsync(newSplits);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid expenseId, Guid currentUserId)
        {
            var expense = await _expenseRepository.GetWithSplits(expenseId);

            if (expense == null || expense.IsDeleted)
                return;

            if (!await IsMemberAsync(expense.GroupId, currentUserId))
                throw new UnauthorizedAccessException();

            expense.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid?> GetGroupIdAsync(Guid expenseId)
        {
            return await _context.Expenses
                .Where(e => e.Id == expenseId)
                .Select(e => (Guid?)e.GroupId)
                .FirstOrDefaultAsync();
        }

        private async Task<Dictionary<Guid, decimal>> ValidateContributionsAsync(
            Guid groupId,
            decimal expenseAmount,
            Dictionary<Guid, decimal>? contributions)
        {
            if (contributions == null || contributions.Count == 0)
            {
                throw new ArgumentException(
                    "Enter the actual amount paid by each contributing member.");
            }

            var group = await _groupRepository.GetByIdWithMembersAsync(groupId);

            if (group == null || group.IsArchived)
                throw new ArgumentException("Group is not available.");

            var activeIds = group.Members
                .Where(m => m.IsActive)
                .Select(m => m.UserId)
                .ToHashSet();

            if (contributions.Keys.Any(id => !activeIds.Contains(id)))
            {
                throw new ArgumentException(
                    "Contributors must be active members of this group.");
            }

            if (contributions.Values.Any(value => value < 0))
            {
                throw new ArgumentException(
                    "Contribution amounts cannot be negative.");
            }

            var positiveContributions = contributions
                .Where(c => c.Value > 0)
                .ToDictionary(
                    c => c.Key,
                    c => decimal.Round(c.Value, 2));

            if (positiveContributions.Count == 0)
            {
                throw new ArgumentException(
                    "At least one member must contribute to the expense.");
            }

            var total = decimal.Round(positiveContributions.Values.Sum(), 2);

            if (total != expenseAmount)
            {
                throw new ArgumentException(
                    $"Actual contributions must add up to {expenseAmount:0.00}. " +
                    $"The entered contributions total {total:0.00}.");
            }

            return positiveContributions;
        }

        private async Task ValidateExpenseAsync(
            Guid groupId,
            Guid paidByUserId,
            decimal amount,
            SplitType splitType,
            List<Guid>? selectedUserIds,
            Dictionary<Guid, decimal>? customShares,
            Guid currentUserId)
        {
            if (!await IsMemberAsync(groupId, currentUserId))
            {
                throw new ArgumentException(
                    "You are not a member of this group.");
            }

            if (amount <= 0)
                throw new ArgumentException(
                    "Expense amount must be greater than zero.");

            var group = await _groupRepository.GetByIdWithMembersAsync(groupId);

            if (group == null || group.IsArchived)
                throw new ArgumentException("Group is not available.");

            var activeIds = group.Members
                .Where(m => m.IsActive)
                .Select(m => m.UserId)
                .ToHashSet();

            if (!activeIds.Contains(paidByUserId))
                throw new ArgumentException(
                    "The payer must be an active group member.");

            if (selectedUserIds == null || selectedUserIds.Count == 0)
                throw new ArgumentException(
                    "Select at least one person to split the expense.");

            var distinctIds = selectedUserIds.Distinct().ToList();

            if (distinctIds.Any(id => !activeIds.Contains(id)))
                throw new ArgumentException(
                    "All selected split members must belong to the group.");

            if (splitType == SplitType.Custom)
            {
                if (customShares == null)
                    throw new ArgumentException(
                        "Enter the custom share for each selected member.");

                foreach (var id in distinctIds)
                {
                    if (!customShares.TryGetValue(id, out var share) ||
                        share <= 0)
                    {
                        throw new ArgumentException(
                            "Every selected member must have a positive share.");
                    }
                }

                var total = distinctIds.Sum(id => customShares[id]);

                if (decimal.Round(total, 2) != decimal.Round(amount, 2))
                {
                    throw new ArgumentException(
                        $"Custom shares must add up to {amount:0.00}.");
                }
            }
        }

        private static List<ExpenseSplit> BuildSplits(
            decimal amount,
            SplitType splitType,
            List<Guid> selectedIds,
            Dictionary<Guid, decimal>? customShares)
        {
            var result = new List<ExpenseSplit>();

            if (splitType == SplitType.Custom)
            {
                foreach (var userId in selectedIds)
                {
                    result.Add(new ExpenseSplit
                    {
                        ExpenseId = Guid.Empty,
                        UserId = userId,
                        ShareAmount = decimal.Round(customShares![userId], 2)
                    });
                }

                return result;
            }

            var baseShare = decimal.Round(
                amount / selectedIds.Count, 2);

            var allocated = 0m;

            for (var i = 0; i < selectedIds.Count; i++)
            {
                var share = i == selectedIds.Count - 1
                    ? decimal.Round(amount - allocated, 2)
                    : baseShare;

                result.Add(new ExpenseSplit
                {
                    ExpenseId = Guid.Empty,
                    UserId = selectedIds[i],
                    ShareAmount = share
                });

                allocated += share;
            }

            return result;
        }
    }
}
