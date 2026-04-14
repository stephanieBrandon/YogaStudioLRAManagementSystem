using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.ViewModels;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize]  //you must be logged in to complete any actions heres
    public class LeaveRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index(string? statusFilter, int? employeeIdFilter)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var role = user.Role;
            var employeeId = user.EmployeeId;

            var myRequests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

           
            var teamRequests = role == UserRoles.MANAGER
                ? await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .Where(l => l.EmployeeId != employeeId)
                    .OrderByDescending(l => l.StartDate)
                    .ToListAsync()
                : new List<LeaveRequest>();

            //since admin should be able to see all the requests plus filter thru them, we create a separate query:
            var adminQuery = _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .AsQueryable();

            if (role == UserRoles.ADMIN)
            {
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    adminQuery = adminQuery.Where(l => l.Status == statusFilter);
                }

                if (employeeIdFilter.HasValue)
                {
                    adminQuery = adminQuery.Where(l => l.EmployeeId == employeeIdFilter.Value);
                }
            }

            var allRequests = role == UserRoles.ADMIN
                ? await adminQuery.OrderByDescending(l => l.StartDate).ToListAsync()
                : new List<LeaveRequest>();

            var model = new LeaveRequestViewModel
            {
                MyRequests = myRequests,
                TeamRequests = teamRequests,
                AllRequests = allRequests,
                Role = role
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == id);
            if (leaveRequest == null)
            {
                return NotFound();
            }
           
            return View(leaveRequest);
        }
               

        [HttpGet] 
        [Authorize(Roles = "STAFF, ADMIN")] //admin shouldnt be able to submit requests
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "STAFF, ADMIN")]
        public async Task<IActionResult> Create(LeaveRequest leaveRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
                return View(leaveRequest);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            leaveRequest.EmployeeId = user.EmployeeId;
            leaveRequest.Status = LeaveRequestStatus.PENDING;

            _context.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = "STAFF, ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == id);

            if (leaveRequest == null)
                return NotFound();

            //Ownership check:
            if (leaveRequest.EmployeeId != user.EmployeeId)
                return Forbid();

            //Only pending edit-able:
            if (leaveRequest.Status != LeaveRequestStatus.PENDING)
            {
                return RedirectToAction("Details", new { id = leaveRequest.RequestId });
            }

            return View(leaveRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "STAFF, ADMIN")]
        public async Task<IActionResult> Edit(int id, LeaveRequest formRequest)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var existingRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == id);

            if (existingRequest == null)
                return NotFound();

            //Ownership check:
            if (existingRequest.EmployeeId != user.EmployeeId)
                return Forbid();

            //Only pending requests should be edit-able:
            if (existingRequest.Status != LeaveRequestStatus.PENDING)
            {
                return RedirectToAction("Details", new { id = existingRequest.RequestId });
            }

            existingRequest.StartDate = formRequest.StartDate;
            existingRequest.EndDate = formRequest.EndDate;
            existingRequest.Reason = formRequest.Reason;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN, MANAGER")]
        [HttpGet]
        public async Task<IActionResult> Approve_Deny(int id)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            return View(request);
        }


        //allow managers/admin to approve requests:
        [Authorize(Roles = "ADMIN, MANAGER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int requestId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be approved.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = LeaveRequestStatus.APPROVED;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request approved!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN, MANAGER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int requestId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be denied.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = LeaveRequestStatus.DENIED;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == id);

            if (leaveRequest == null)
                return NotFound();

            if (leaveRequest.EmployeeId != user.EmployeeId)
                return Forbid();

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool LeaveRequestExists(int id)
        {
            return _context.LeaveRequests.Any(e => e.RequestId == id);
        }
    }
}
