using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public string? Certification { get; set; }

        [Required]
        public int LeaveBalance { get; set; } // in days

        // Foreign key to StudioRole
        [Required]
        public int StudioRoleId { get; set; }

        [ForeignKey("StudioRoleId")]
        public StudioRole StudioRole { get; set; } = null!;
    }
}