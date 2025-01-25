using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Domain.ApplicationDbContext
{
    public class ApplicationDb : DbContext
    {
        public ApplicationDb(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeavesType> LeavesTypes { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LeaveBalance>()
           .HasOne(lb => lb.LeaveType)
           .WithMany()
           .HasForeignKey(lb => lb.LeaveTypeId)
           .OnDelete(DeleteBehavior.NoAction);
           
        }
    }
}
