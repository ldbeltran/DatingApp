using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dto
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }    

        [Required]
        [StringLength(8, MinimumLength=5, ErrorMessage="Error lenght should be between 5 and 8")]
        public string Password { get; set; }
    }
}