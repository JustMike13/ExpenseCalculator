using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ExpenseCalculator.Models
{
    public class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PayedBy { get; set; }
        public string Name { get; set; }
        public float TotalAmmount { get; set; }
        public bool EquallyDivided { get; set; }
        public float OwnContribution { get; set; }
        [NotNull]
        public int TripId { get; set; }

        public Expense()
        {

        }
    }
}
