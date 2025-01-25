using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement4.Domain.Dto
{
    public class LeaveTypeDto
    {

        [Required]
        public string Name { get; set; }
    }
}
