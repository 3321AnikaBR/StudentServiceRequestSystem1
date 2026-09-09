using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentServiceRequestSystem.Data;
using StudentServiceRequestSystem.Models;

namespace StudentServiceRequestSystem1.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get logged-in student's numeric ID
        private int? GetStudentId()
        {
            string? value = HttpContext.Session.GetString("StudentId");

            if (int.TryParse(value, out int studentId))
            {
                return studentId;
            }

            return null;
        }


        // ===============================
        // MY REQUESTS / ALL REQUESTS
        // ===============================
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string? role =
                HttpContext.Session.GetString("Role");

            // Staff sees everything
            if (role == "Staff")
            {
                return View(
                    await _context.ServiceRequests
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync());
            }

            int? studentId = GetStudentId();

            if (studentId == null)
            {
                return RedirectToAction("Dashboard", "Account");
            }

            return View(
                await _context.ServiceRequests
                .Where(x => x.StudentId == studentId.Value)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync());
        }


        // ===============================
        // DETAILS
        // ===============================
        public async Task<IActionResult> Details(int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(x =>
                    x.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId = GetStudentId();

                if (studentId == null ||
                    request.StudentId != studentId.Value)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(request);
        }


        // ===============================
        // CREATE GET
        // ===============================
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? studentId = GetStudentId();

            if (studentId == null)
            {
                return RedirectToAction("Dashboard", "Account");
            }

            ServiceRequest request = new ServiceRequest
            {
                StudentId = studentId.Value,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            return View(request);
        }


        // ===============================
        // CREATE POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceRequest serviceRequest)
        {
            int? studentId = GetStudentId();

            if (studentId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            serviceRequest.StudentId =
                studentId.Value;

            serviceRequest.Status =
                "Pending";

            serviceRequest.CreatedDate =
                DateTime.Now;

            serviceRequest.UpdatedDate =
                DateTime.Now;

            ModelState.Remove("StudentId");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("UpdatedDate");

            if (ModelState.IsValid)
            {
                _context.ServiceRequests.Add(serviceRequest);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Request submitted successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(serviceRequest);
        }


        // ===============================
        // EDIT GET
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Edit(
            int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var request =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId = GetStudentId();

                if (studentId == null ||
                    request.StudentId != studentId.Value)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(request);
        }


        // ===============================
        // EDIT POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int requestId,
            ServiceRequest serviceRequest)
        {
            if (requestId != serviceRequest.RequestId)
            {
                return NotFound();
            }

            var original = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.RequestId == requestId);

            if (original == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId = GetStudentId();

                if (studentId == null ||
                    original.StudentId != studentId.Value)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Student cannot change status
                serviceRequest.Status =
                    original.Status;
            }

            serviceRequest.StudentId =
                original.StudentId;

            serviceRequest.CreatedDate =
                original.CreatedDate;

            serviceRequest.UpdatedDate =
                DateTime.Now;

            ModelState.Remove("StudentId");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("UpdatedDate");

            if (role != "Staff")
            {
                ModelState.Remove("Status");
            }

            if (ModelState.IsValid)
            {
                _context.Update(serviceRequest);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(serviceRequest);
        }


        // ===============================
        // DELETE GET
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Delete(
            int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(x =>
                    x.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }


        // ===============================
        // DELETE POST
        // ===============================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int requestId)
        {
            var request =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (request != null)
            {
                _context.ServiceRequests.Remove(request);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ===============================
        // STAFF STATUS UPDATE
        // Figure 6
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int requestId,
            string status)
        {
            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            var request =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            string[] allowedStatuses =
            {
                "Pending",
                "Processing",
                "Completed",
                "Rejected"
            };

            if (!allowedStatuses.Contains(status))
            {
                return RedirectToAction(
                    "Details",
                    new { requestId });
            }

            request.Status = status;
            request.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Request status updated successfully.";

            return RedirectToAction(
                "Details",
                new { requestId });
        }
    }
}