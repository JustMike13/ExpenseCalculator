using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ExpenseCalculator.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [NotNull]
        public string Payer { get; set; }
        public string Name { get; set; }
        [AllowNull]
        public float Ammount { get; set; }
        [AllowNull]
        public int ExpenseId { get; set; }

    }
}
