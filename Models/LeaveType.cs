using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("LEAVE_TYPES")]
    public class LeaveType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeaveTypeId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Leave name is required.")]
        public string Name { get; set; }

        [Required]
        public bool IsPaid { get; set; } //true -> is paid, false -> not paid

        [Required]
        public int MinDays { get; set; } = 1;

        [Required]
        public int MaxDays { get; set; } 


    }
}
