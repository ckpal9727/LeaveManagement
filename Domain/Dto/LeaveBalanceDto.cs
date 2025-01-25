namespace EmployeeManagement4.Domain.Dto
{
    public class LeaveBalanceDto
    {
        public Guid LeaveTypeId {  get; set; }
        public Guid UserId { get; set; }
        public float LeaveBalanceCount { get; set; }
    }
}
