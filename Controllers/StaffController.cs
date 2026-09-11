using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentServiceRequestSystem.Data;

namespace StudentServiceRequestSystem1.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        // STAFF DASHBOARD
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(requests);
        }

        // OPEN ONE REQUEST
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // UPDATE STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int requestId, string status)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            request.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = requestId });
        }
    }
}