using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spleet.Models;

namespace Spleet.Data
{
    public class SpleetDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public SpleetDbContext(DbContextOptions<SpleetDbContext> options)
            : base(options)
        {
        }

        // Identity already provides DbSet<User> Users.

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
            // Identity must configure its tables first.
            base.OnModelCreating(modelBuilder);

           
         

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
           

            // CreatedByUserId is intentionally a soft reference.
            // It is NOT configured as a foreign key.
            modelBuilder.Entity<Group>()
                .Ignore(g => g.CreatedBy);
           
     
            

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
                .HasOne(s => s.Group)
                .WithMany(g => g.Settlements)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasOne(gi => gi.Group)
                .WithMany(g => g.Invites)
                .HasForeignKey(gi => gi.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupInvite>()
                .HasIndex(gi => gi.Token)
                .IsUnique();


            modelBuilder.Entity<ActivityLogEntry>()
                .HasOne(a => a.Group)
                .WithMany(g => g.ActivityLog)
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityLogEntry>()
                .HasOne(a => a.Actor)
                .WithMany()
                .HasForeignKey(a => a.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<RecurringExpenseTemplate>()
                .HasOne(r => r.Group)
                .WithMany()
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // CreatedByUserId / CreatedBy are intentionally soft references.
            modelBuilder.Entity<RecurringExpenseTemplate>()
                .Ignore(r => r.CreatedBy);
        }
    }
}