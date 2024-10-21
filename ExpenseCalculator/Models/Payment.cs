using System.Diagnostics.CodeAnalysis;

namespace ExpenseCalculator.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [NotNull]
        public int Payer {  get; set; }
        public string Name { get; set; }
        [AllowNull]
        public float Ammount { get; set; }
        [AllowNull]
        public int ExpenseId { get; set; }


    }
}
