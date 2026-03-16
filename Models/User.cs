using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("USERS")]
    public class User
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int UserId { get; set; }

        [Column("user_name")]
        [Required]
        [StringLength(50,MinimumLength = 3, ErrorMessage ="Username is required.")]
        public string UserName { get; set; }

        [Column("password")]
        [Required]
        public string Password { get; set; }

        [Column("email_id")]
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Column("profile_role")]
        [Required]
        public string Role { get; set; }

        //EmployeeId (FK) - multitable 
        //waiting on getting the code from Katherine


    }
}
