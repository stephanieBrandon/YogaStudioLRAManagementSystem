using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("LEAVE_REQUESTS")]
    public class LeaveRequest 

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")] 
        public DateOnly StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly EndDate { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "Status is required")]

        //Possible Values: "Absent", "Present", "On Leave"
        public string Status {  get; set; } //this would be selected via drop down
        public List<string> StatusOptions { get; set; } = new List<string> {};

        [StringLength(150, MinimumLength = 2)]
        public string? Reason { get; set; }

        //Foreign keys:
        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]

        public Employee Employee { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }
        [ForeignKey("LeaveTypeId")]

        public LeaveType LeaveType { get; set; }
    }
}
