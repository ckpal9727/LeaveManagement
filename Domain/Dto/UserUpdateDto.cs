using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement4.Domain.Dto
{
    public class UserUpdateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
