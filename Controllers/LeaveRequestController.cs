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

        //-------------------------------------MANAGER + ADMIN---------------------------------------------------------------
        [HttpGet]
        //View all leave requests in a 'manager approval view' - only managers and admin have access:
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Index(LeaveRequestViewModel model)
        {
            var requests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                
                .ToListAsync();
            
            return View(requests);
        }

        [HttpGet]
        //here we can view a specific leave request -- again only managers
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Details(int? id)
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
        //---------------------------------------------BOTH----------------------------------------------------------------        

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,StartDate,EndDate,Reason,EmployeeId,LeaveTypeId")] LeaveRequest leaveRequest)
        {
            if (ModelState.IsValid)
            {

                leaveRequest.Status = LeaveRequestStatus.PENDING;

                _context.Add(leaveRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag["EmployeeId"] = leaveRequest.EmployeeId;
            ViewBag["LeaveTypeId"] = leaveRequest.LeaveTypeId;
            return View(leaveRequest);
        }


        [HttpGet]
        //We are only editing PENDING requests -- Managers and Admin have access
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .AsQueryable()
                .Where(r => r.Status.ToString() == "PENDING") //request status has to be pending
                .FirstOrDefaultAsync(r => r.RequestId == id); // querying based on the request id given

            if (leaveRequest == null)
            {
                return NotFound();
            }
            ViewBag["EmployeeId"] = leaveRequest.EmployeeId;
            ViewBag["LeaveTypeId"] = leaveRequest.LeaveTypeId; //needed for the form

            return View(leaveRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //this action should be allowed by only managers and admin
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,StartDate,EndDate,Status,Reason,EmployeeId,LeaveTypeId")] LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveRequestExists(leaveRequest.RequestId))
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
            ViewData["EmployeeId"] = leaveRequest.EmployeeId;
            ViewData["LeaveTypeId"] = leaveRequest.LeaveTypeId;
            return View(leaveRequest);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Approve_Deny(int id)
        {
            return RedirectToAction(nameof(Approve_Deny), id);
        }


        //allow managers to approve requests:
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> Approve(int requestId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.RequestId)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }
            request.Status = LeaveRequestStatus.APPROVED; //using the 'approved' constant to have less 'harcoded' code

            _context.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request approved!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN, EMPLOYEE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int requestId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }
            request.Status = LeaveRequestStatus.DENIED; //again using status constant
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }

        //-------------------------------------------------------------------------------------------------------------------

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
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest != null)
            {
                _context.LeaveRequests.Remove(leaveRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveRequestExists(int id)
        {
            return _context.LeaveRequests.Any(e => e.RequestId == id);
        }
    }
}
