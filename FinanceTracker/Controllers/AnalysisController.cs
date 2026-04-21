using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;

namespace FinanceTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalysisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/analysis/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == 1)
                .ToListAsync();

            var totalIncome = transactions
                .Where(t => t.Category.Type == "Income" &&
                            t.Date.Month == DateTime.Now.Month &&
                            t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Category.Type == "Expense" &&
                            t.Date.Month == DateTime.Now.Month &&
                            t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);

            var savingsRate = totalIncome > 0
                ? Math.Round((totalIncome - totalExpenses) / totalIncome * 100, 2)
                : 0;

            var topCategory = transactions
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => t.Category.Name)
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .FirstOrDefault()?.Key ?? "No expenses yet";

            var monthlyTrend = transactions
                .Where(t => t.Date >= DateTime.Now.AddMonths(-1))
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Income = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount)
                })
                .ToList();

            return Ok(new
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetSavings = totalIncome - totalExpenses,
                SavingsRate = savingsRate,
                TopExpenseCategory = topCategory,
                MonthlyTrend = monthlyTrend
            });
        }
    }
}