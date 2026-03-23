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
        public string IsPaid { get; set; } //this would be selected via drop down (?),
        public List<string> IsPaidOptions { get; set; } = new List<string> { "Yes", "No" };

        [Required]
        //public static int MinDays { get; set; } = 1; //what should the default be? 1?
        public int MinDays { get; set; } = 1;

        [Required]
        //[Range(minimum: MinDays, 10)]
        public int MaxDays { get; set; } //should there be a range for this attribute?


    }
}
