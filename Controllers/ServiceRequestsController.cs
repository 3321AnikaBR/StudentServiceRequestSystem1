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


        // ==========================================
        // GET LOGGED-IN STUDENT ID
        // ==========================================
        private int? GetLoggedInStudentId()
        {
            string? studentIdString =
                HttpContext.Session.GetString("StudentId");

            if (string.IsNullOrWhiteSpace(studentIdString))
            {
                return null;
            }

            if (int.TryParse(studentIdString, out int studentId))
            {
                return studentId;
            }

            return null;
        }


        // ==========================================
        // INDEX
        // Student = Own Requests
        // Staff = All Requests
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role == "Staff")
            {
                var allRequests =
                    await _context.ServiceRequests
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                return View(allRequests);
            }

            int? studentId =
                GetLoggedInStudentId();

            if (studentId == null)
            {
                TempData["Error"] =
                    "Invalid Student ID. Please login again.";

                return RedirectToAction(
                    "Dashboard",
                    "Account");
            }

            var myRequests =
                await _context.ServiceRequests
                .Where(r => r.StudentId == studentId.Value)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(myRequests);
        }


        // ==========================================
        // DETAILS
        // ==========================================
        public async Task<IActionResult> Details(
            int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var request =
                await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId =
                    GetLoggedInStudentId();

                if (studentId == null ||
                    request.StudentId != studentId.Value)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(request);
        }


        // ==========================================
        // CREATE GET
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            int? studentId =
                GetLoggedInStudentId();

            if (studentId == null)
            {
                TempData["Error"] =
                    "Your Student ID must contain numbers only.";

                return RedirectToAction(
                    "Dashboard",
                    "Account");
            }

            var request =
                new ServiceRequest
                {
                    StudentId = studentId.Value,
                    Status = "Pending",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

            return View(request);
        }


        // ==========================================
        // CREATE POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceRequest serviceRequest)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            int? studentId =
                GetLoggedInStudentId();

            if (studentId == null)
            {
                TempData["Error"] =
                    "Invalid Student ID.";

                return RedirectToAction(
                    "Dashboard",
                    "Account");
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
                _context.ServiceRequests
                    .Add(serviceRequest);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Service request submitted successfully.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(serviceRequest);
        }


        // ==========================================
        // EDIT GET
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(
            int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId =
                    GetLoggedInStudentId();

                if (studentId == null ||
                    serviceRequest.StudentId != studentId.Value)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(serviceRequest);
        }


        // ==========================================
        // EDIT POST
        // ==========================================
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

            var existingRequest =
                await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.RequestId == requestId);

            if (existingRequest == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId =
                    GetLoggedInStudentId();

                if (studentId == null ||
                    existingRequest.StudentId != studentId.Value)
                {
                    return RedirectToAction("Index");
                }
            }

            serviceRequest.StudentId =
                existingRequest.StudentId;

            serviceRequest.CreatedDate =
                existingRequest.CreatedDate;

            serviceRequest.UpdatedDate =
                DateTime.Now;

            // Student cannot change status
            if (role != "Staff")
            {
                serviceRequest.Status =
                    existingRequest.Status;
            }


            ModelState.Remove("StudentId");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("UpdatedDate");

            if (role != "Staff")
            {
                ModelState.Remove("Status");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(
                        serviceRequest);

                    await _context
                        .SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(
                        serviceRequest.RequestId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(
                    nameof(Index));
            }

            return View(serviceRequest);
        }


        // ==========================================
        // DELETE GET
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(
            int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.RequestId == requestId);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId =
                    GetLoggedInStudentId();

                if (studentId == null ||
                    serviceRequest.StudentId != studentId.Value)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(serviceRequest);
        }


        // ==========================================
        // DELETE POST
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int requestId)
        {
            var serviceRequest =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (serviceRequest == null)
            {
                return RedirectToAction(
                    nameof(Index));
            }

            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                int? studentId =
                    GetLoggedInStudentId();

                if (studentId == null ||
                    serviceRequest.StudentId != studentId.Value)
                {
                    return RedirectToAction("Index");
                }
            }

            _context.ServiceRequests
                .Remove(serviceRequest);

            await _context
                .SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }


        // ==========================================
        // STAFF UPDATE STATUS GET
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(
            int requestId)
        {
            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Staff")
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var request =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }


        // ==========================================
        // STAFF UPDATE STATUS POST
        // ==========================================
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
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var request =
                await _context.ServiceRequests
                .FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;

            request.UpdatedDate =
                DateTime.Now;

            await _context
                .SaveChangesAsync();

            TempData["Success"] =
                "Request status updated successfully.";

            return RedirectToAction(
                "StaffDashboard",
                "Account");
        }


        // ==========================================
        // EXISTS
        // ==========================================
        private bool ServiceRequestExists(
            int id)
        {
            return _context.ServiceRequests
                .Any(e => e.RequestId == id);
        }
    }
}