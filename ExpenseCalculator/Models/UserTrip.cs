using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExpenseCalculator.Models
{
    public class UserTrip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {  get; set; }
        public string UserId { get; set; }
        public int TripId { get; set; }
        public bool Owner {  get; set; }
    }
}
