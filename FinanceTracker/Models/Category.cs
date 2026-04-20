using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^(Income|Expense)$", ErrorMessage = "Type must be either 'Income' or 'Expense'")]
        public string Type {  get; set; } //Income or Expense
    }
}
