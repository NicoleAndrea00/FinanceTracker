using BCrypt.Net;
using FinanceTracker.Data;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            // Check if email already exists
            if (_context.Users.Any(u => u.Email== email))
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            var user = new User
            {
                Name = name,
                Email= email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto login after register
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Index", "Transactions");
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        //GET: Account/Index
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login");

            return View(user);
        }
        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email==email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Index", "Transactions");
        }

        //Post: Account/change password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                ViewBag.Error = "Current password is incorrect.";
                return View("Index", user);
            }

            // Check new passwords match
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New passwords do not match.";
                return View("Index", user);
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();

            ViewBag.Success = "Password changed successfully!";
            return View("Index", user);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}