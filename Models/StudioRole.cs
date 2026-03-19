using System.ComponentModel.DataAnnotations;

namespace YogaStudioLRAManagementSystem.Models
{
    public class StudioRole
    {
        [Key]
        public int SutdioRoleId { get; set ;}

        [Required]
        public string RoleName { get; set; } = null!;

        public bool RequiresCertification { get; set; }

        // Navigation property
        public ICollection<Employee> Employees { get; set; } = null!;



    }
}