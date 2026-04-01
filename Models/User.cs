using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public class User
    {
        public int UserId {  get; set; }
        public string Name { get; set; }

        [Required]
        [RegularExpression( @"^[^@\s]+@[^@\s]+\.(com|ie)$", ErrorMessage = "Email must be in the format name@example.com or name@example.ie")]
        public string Email {  get; set; }
        public string PasswordHash { get; set; }   

    }
}
