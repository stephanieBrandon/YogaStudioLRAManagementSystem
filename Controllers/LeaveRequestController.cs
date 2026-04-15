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
        public async Task<IActionResult> Details(int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == requestId);
            if (leaveRequest == null)
            {
                return NotFound();
            }
           
            return View(leaveRequest);
        }
               

        [HttpGet] 
        [Authorize(Roles = "Staff, Manager")] //admin shouldnt be able to submit requests
        public IActionResult Create()
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Manager")]
        public async Task<IActionResult> Create(LeaveRequest leaveRequest)
        {
            ModelState.Remove("Employee");
            ModelState.Remove("LeaveType");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var employeeId = int.Parse(User.FindFirstValue("EmployeeId"));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            leaveRequest.EmployeeId = user.EmployeeId;
            leaveRequest.Status = LeaveRequestStatus.PENDING;

            if (ModelState.IsValid)
            {
                _context.Add(leaveRequest);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Leave request submitted!";
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        TempData["Error"] =  $"FIELD: {state.Key} ERROR: {error.ErrorMessage}";
                    }
                }
                //TempData["Error"] = "Error occured in submitting leave request";
                ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
                return View(leaveRequest);
            }

                return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> Edit(int? requestId)
        {
            if (requestId == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (leaveRequest == null)
                return NotFound();

            //Ownership check:
            if (leaveRequest.EmployeeId != user.EmployeeId)
                return Forbid();

            //Only pending edit-able:
            if (leaveRequest.Status != LeaveRequestStatus.PENDING)
            {
                return RedirectToAction("Details", new { requestId = leaveRequest.RequestId });
            }

            return View(leaveRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> Edit(int requestId, LeaveRequest formRequest)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var existingRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (existingRequest == null)
                return NotFound();

            //Ownership check:
            if (existingRequest.EmployeeId != user.EmployeeId)
                return Forbid();

            //Only pending requests should be edit-able:
            if (existingRequest.Status != LeaveRequestStatus.PENDING)
            {
                return RedirectToAction("Details", new { requestId = existingRequest.RequestId });
            }

            existingRequest.StartDate = formRequest.StartDate;
            existingRequest.EndDate = formRequest.EndDate;
            existingRequest.Reason = formRequest.Reason;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Approve_Deny(int requestId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.RequestId == requestId); 

            if (request == null)
                return NotFound();

            return View(request);
        }


        //allow managers/admin to approve requests:
        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "Admin, Manager")]
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

        public async Task<IActionResult> Delete(int? requestId)
        {
            if (requestId == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.RequestId == requestId);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int requestId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

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
