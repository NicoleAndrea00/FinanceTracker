using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Models;

namespace FinanceTracker.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 1;

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t=> t.Date)
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

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.NetSavings = totalIncome - totalExpenses;
            ViewBag.SavingsRate = savingsRate;
            ViewBag.TopCategory = topCategory;

            return View(transactions);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,Amount,Description,Date,CategoryId")] Transaction transaction)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            transaction.UserId = HttpContext.Session.GetInt32("UserId") ?? 1;
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryID", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryID", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Savings
        public async Task<IActionResult> Savings()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 1;

            var savings = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .Where(t => t.UserId == userId && t.Category.Type == "Savings")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var totalSavings = savings.Sum(t => t.Amount);
            ViewBag.TotalSavings = totalSavings;

            return View(savings);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,Amount,Description,Date,CategoryId")] Transaction transaction)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id != transaction.TransactionId) return NotFound();

            transaction.UserId = HttpContext.Session.GetInt32("UserId") ?? 1;
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryID", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}