using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentServiceRequestSystem.Data;
using StudentServiceRequestSystem.Models;

namespace StudentServiceRequestSystem1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // REGISTER PAGE
        // =====================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =====================================
        // REGISTER POST
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            bool studentExists = await _context.Users
                .AnyAsync(u => u.StudentId == user.StudentId);

            if (studentExists)
            {
                ViewBag.Error = "This Student ID is already registered.";
                return View(user);
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
            {
                ViewBag.Error = "This email is already registered.";
                return View(user);
            }

            user.Role = "Student";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Account created successfully. Please login.";

            return RedirectToAction("Login");
        }

        // =====================================
        // LOGIN PAGE
        // =====================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =====================================
        // LOGIN POST
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32(
                "UserId",
                user.UserId);

            HttpContext.Session.SetString(
                "StudentId",
                user.StudentId);

            HttpContext.Session.SetString(
                "UserName",
                user.Name);

            HttpContext.Session.SetString(
                "Role",
                user.Role);

            // Staff goes to Staff Dashboard
            if (user.Role == "Staff")
            {
                return RedirectToAction("StaffDashboard");
            }

            // Student goes to Student Dashboard
            return RedirectToAction("Dashboard");
        }

        // =====================================
        // STUDENT DASHBOARD
        // =====================================
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            var role =
                HttpContext.Session.GetString("Role");

            // Staff should not use student dashboard
            if (role == "Staff")
            {
                return RedirectToAction("StaffDashboard");
            }

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName");

            ViewBag.StudentId =
                HttpContext.Session.GetString("StudentId");

            return View();
        }

        // =====================================
        // STAFF DASHBOARD
        // =====================================
        [HttpGet]
        public async Task<IActionResult> StaffDashboard()
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            var role =
                HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (role != "Staff")
            {
                return RedirectToAction("Dashboard");
            }

            var requests = await _context.ServiceRequests
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName");

            ViewBag.Total =
                requests.Count;

            ViewBag.Pending =
                requests.Count(x =>
                    x.Status == "Pending");

            ViewBag.Processing =
                requests.Count(x =>
                    x.Status == "Processing");

            ViewBag.Completed =
                requests.Count(x =>
                    x.Status == "Completed");

            ViewBag.Rejected =
                requests.Count(x =>
                    x.Status == "Rejected");

            return View(requests);
        }

        // =====================================
        // LOGOUT
        // =====================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}