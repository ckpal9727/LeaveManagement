namespace EmployeeManagement4.Domain.Dto
{
    public class LeaveBalanceUpdateDto
    {
        public Guid LeaveTypeId { get; set; }
        public Guid UserId { get; set; }
        public float LeaveBalanceCount { get; set; }
    }
}
