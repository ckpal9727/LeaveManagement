using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeManagement4.Domain.Dto
{
    public class LeaveUpdateDto
    {
        public Guid LeaveTypeId { get; set; }
        //public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveStatus LeaveStatus { get; set; }
        public float LeaveCount { get; set; }
    }
}
