using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string username { get; set; }  
        [Required]  
        [StringLength(8,MinimumLength=4,ErrorMessage="passwor must be 6 chararcter")]
        public string password { get; set; }
    }
}