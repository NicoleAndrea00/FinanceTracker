using Xunit;
using FinanceTracker.Models;

namespace FinanceTracker.Tests
{
    public class FinanceTrackerTests
    {
        // Test 1: Saving Rate Calculation
 
        [Fact]
        public void SavingsRate_ShouldCalculateCorrectly()
        {
       
            decimal totalIncome = 4300;
            decimal totalExpenses = 140;

         
            decimal savingsRate = Math.Round((totalIncome - totalExpenses) / totalIncome * 100, 2);

            Assert.Equal(96.74m, savingsRate);
        }

        // Test 2: Savings rate should be 0 when income is 0
        [Fact]
        public void SavingsRate_ShouldBeZero_WhenIncomeIsZero()
        {

            decimal totalIncome = 0;
            decimal totalExpenses = 100;


            decimal savingsRate = totalIncome > 0
                ? Math.Round((totalIncome - totalExpenses) / totalIncome * 100, 2)
                : 0;

  
            Assert.Equal(0, savingsRate);
        }

        // Test 3: Transaction amount must be positive
        [Fact]
        public void Transaction_Amount_ShouldBePositive()
        {

            var transaction = new Transaction
            {
                Amount = 50.00m,
                Description = "Tesco",
                Date = DateTime.Now
            };


            Assert.True(transaction.Amount > 0);
        }
    }
}