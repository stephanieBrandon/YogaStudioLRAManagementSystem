using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("ATTENDANCES")]
    public class Attendance
    {
        [Key]
        [Column("attendance_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int AttendanceId { get; set; }

        //[Required]
        //[ForeignKey("EmployeeId")]
        //EmployeeId (FK) - multitable 
        //waiting on getting the code from Katherine
        //public int Employee Employee { get; set; }

        [Required]
        [Column("date")]
        [DataType(DataType.Date)]
        public DateOnly Date {  get; set; }

        [Column("clock_in")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm tt}")]
        [Display(Name ="Clock In")]
        public DateTime? ClockIn { get; set; } //nullable - not clocked in yet

        [Column("clock_out")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm tt}")]
        [Display(Name = "Clock Out")]
        public DateTime? ClockOut { get; set; } //nullable - not clocked out yet

        [Column("total_hours")]
        [Display(Name ="Total Hours")]
        public double? TotalHours { get; set; } //nullable - calculated after clock out

        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Absent"; //default: Absent, Present, On Leave
    }
}
