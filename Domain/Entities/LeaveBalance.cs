using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLeaveManagement.Domain.Entities
{
    public class LeaveBalance
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("leaveTypeId")]
        public Guid LeaveTypeId { get; set; }
        public virtual LeavesType LeaveType { get; set; }

        [ForeignKey("userId")]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public float LeaveBalanceCount { get; set; }

    }
}
