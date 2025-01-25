using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Domain.Entities
{
    public class LeavesType
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
