using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class LeaveRequestViewModel : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestViewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<LeaveRequest> MyRequests { get; set; }
        public IEnumerable<LeaveRequest> TeamRequests { get; set; } 


        public async Task<IActionResult> Index(User.UserId id) 
            //this action should give us all requests belonging to the current users
            //as well as all the 'team requests' which are everyone BUT the current manager's requests
        {
            //so this list should give us all the requests that belong to the currently logged in user:
            MyRequests = await _context.LeaveRequests
                .Include(l => l.EmployeeId == id)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .ToListAsync();

            TeamRequests = await._context.LeaveRequests
                .Include(l => l.EmployeeId != id)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .ToListAsync();

            return View(MyRequests, TeamRequests);
        }
    }
}
