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
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")] //what date format do we prefer?
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        //How to set it 'less than' the start date?
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "Status is required")]
        //Possible Values: ACTIVE || CLOSED
        public string Status {  get; set; } //this would be selected via drop down (?),
        public List<string> StatusOptions { get; set; } = new List<string> {"Active", "Closed"};

        [StringLength(150, MinimumLength = 2)]
        public string? Reason { get; set; } //I don't think you should be saying always why you want an off-day?

        //Foreign keys:

        [ForeignKey("EmployeeId")]

        [ForeignKey("LeaveTypeId")]



    }
}
