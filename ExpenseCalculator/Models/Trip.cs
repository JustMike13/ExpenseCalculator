using System.Diagnostics.CodeAnalysis;

namespace ExpenseCalculator.Models
{
    public class Trip
    {
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public int CreatorId { get; set; }
    }
}
