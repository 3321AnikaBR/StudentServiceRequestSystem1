
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentServiceRequestSystem.Models;
using StudentServiceRequestSystem.Data;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SERVICEREQUESTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ServiceRequests.ToListAsync());
    }

    // GET: SERVICEREQUESTS/Details/5
    public async Task<IActionResult> Details(int? requestid)
    {
        if (requestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.RequestId == requestid);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SERVICEREQUESTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("RequestId,StudentId,RequestType,Description,Status,CreatedDate,UpdatedDate")] ServiceRequest servicerequest)
    {
        if (ModelState.IsValid)
        {
            _context.Add(servicerequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Edit/5
    public async Task<IActionResult> Edit(int? requestid)
    {
        if (requestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests.FindAsync(requestid);
        if (servicerequest == null)
        {
            return NotFound();
        }
        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? requestid, [Bind("RequestId,StudentId,RequestType,Description,Status,CreatedDate,UpdatedDate")] ServiceRequest servicerequest)
    {
        if (requestid != servicerequest.RequestId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(servicerequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequestExists(servicerequest.RequestId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Delete/5
    public async Task<IActionResult> Delete(int? requestid)
    {
        if (requestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.RequestId == requestid);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? requestid)
    {
        var servicerequest = await _context.ServiceRequests.FindAsync(requestid);
        if (servicerequest != null)
        {
            _context.ServiceRequests.Remove(servicerequest);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ServiceRequestExists(int? requestid)
    {
        return _context.ServiceRequests.Any(e => e.RequestId == requestid);
    }
}
