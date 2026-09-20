using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spleet.Models;

namespace Spleet.Data
{
    public class SpleetDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public SpleetDbContext(DbContextOptions<SpleetDbContext> options)
            : base(options) { }

        // DbSet<User> "Users" is already provided by IdentityDbContext — not redeclared here

        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
        public DbSet<Settlement> Settlements => Set<Settlement>();
        public DbSet<GroupInvite> GroupInvites => Set<GroupInvite>();
        public DbSet<ActivityLogEntry> ActivityLogEntries => Set<ActivityLogEntry>();
        public DbSet<RecurringExpenseTemplate> RecurringExpenseTemplates => Set<RecurringExpenseTemplate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // MUST run first — sets up Identity's own AspNetUsers/AspNetRoles tables

            // Note: no manual unique index on User.Email needed anymore —
            // Identity already enforces uniqueness via its own NormalizedEmail index

            modelBuilder.Entity<GroupMember>()
                .HasIndex(gm => new { gm.GroupId, gm.UserId })
                .IsUnique();

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Expenses)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidBy)
                .WithMany(u => u.ExpensesPaid)
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExpenseSplit>()
                .HasOne(es => es.Expense)
                .WithMany(e => e.Splits)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpenseSplit>()
                .HasOne(es => es.User)
                .WithMany(u => u.ExpenseSplits)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExpenseSplit>()
                .HasIndex(es => new { es.ExpenseId, es.UserId })
                .IsUnique();

            modelBuilder.Entity<Settlement>()
                .HasOne(s => s.Payer)
                .WithMany()
                .HasForeignKey(s => s.PayerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Settlement>()
                .HasOne(s => s.Payee)
                .WithMany()
                .HasForeignKey(s => s.PayeeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupInvite>()
                .HasIndex(gi => gi.Token)
                .IsUnique();
        }
    }
}