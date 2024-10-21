using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseCalculator.Models;

namespace ExpenseCalculator.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ExpenseCalculator.Models.Expense> Expense { get; set; } = default!;
        public DbSet<ExpenseCalculator.Models.Payment> Payment { get; set; } = default!;
        public DbSet<ExpenseCalculator.Models.Trip> Trip { get; set; } = default!;
        public DbSet<ExpenseCalculator.Models.UserTrip> UserTrip { get; set; } = default!;
    }
}
