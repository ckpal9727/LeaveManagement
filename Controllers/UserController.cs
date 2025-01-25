using EmployeeLeaveManagement.Domain.ApplicationDbContext;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeManagement4.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDb _applicationDb;

        public UserController(ApplicationDb applicationDb)
        {
            _applicationDb = applicationDb;

        }
        [Authorize]
        [HttpPost("RequestForLeave")]
        public async Task<IActionResult> RequestLeave(LeaveAddDto input)
        {
            if (!await _applicationDb.Users.AnyAsync(x => x.Id == input.UserId)) return NotFound("User is not found");
            if (!await _applicationDb.LeavesTypes.AnyAsync(x => x.Id == input.LeaveTypeId)) return NotFound("Leave Type is not found");

            //check has balance 
            var leaveBalance = await _applicationDb.LeaveBalances.Where(x => x.UserId == input.UserId && x.LeaveTypeId == input.LeaveTypeId).FirstOrDefaultAsync();
            if (leaveBalance == null || leaveBalance.LeaveBalanceCount <= 0) return NotFound("No Leave Balance found");

            if (input.EndDate <= input.StartDate)
            {
                return BadRequest("End Should be greater than the start Date");
            }
            var leaveCount = (input.EndDate.Date - input.StartDate.Date).Days;

            try
            {
                var leave = new Leave()
                {
                    Id = Guid.NewGuid(),
                    LeaveTypeId = input.LeaveTypeId,
                    UserId = input.UserId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    LeaveCount = leaveCount,
                    LeaveStatus = LeaveStatus.initiated,
                };

                leaveBalance.LeaveBalanceCount = leaveBalance.LeaveBalanceCount - leaveCount;
                await _applicationDb.Leaves.AddAsync(leave);
                _applicationDb.Update(leaveBalance);
                await _applicationDb.SaveChangesAsync();
                return Ok(leaveBalance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpGet("leaves/{userId}")]
        public async Task<IActionResult> GetAllLeaves(Guid userId)
        {
            if (!await _applicationDb.Users.AnyAsync(x => x.Id == userId)) return NotFound("User is not found");
            try
            {
                var leaves = await _applicationDb.Leaves.Where(x => x.UserId == userId).Include(u => u.User).Include(lt => lt.LeaveType).Select(x => new
                {
                    Id = x.Id,
                    LeaveType = x.LeaveType.Name,
                    LeaveCount = x.LeaveCount
                }).ToListAsync();
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("updateLeave/{leaveId}")]
        public async Task<IActionResult> UpdateLeave(Guid leaveId, LeaveUpdateDto input)
        {
            var leave = await _applicationDb.Leaves.Where(x => x.Id == leaveId).FirstOrDefaultAsync();
            if (leave == null)
            {
                return NotFound("Leave is not found");

            }
            if (leave.LeaveStatus == LeaveStatus.rejected || leave.LeaveStatus == LeaveStatus.approved)
            {
                return BadRequest("You Can't modify leave");
            }
            
            try
            {
                var leaveBalance = await _applicationDb.LeaveBalances.Where(x => x.UserId == leave.UserId && x.LeaveTypeId == input.LeaveTypeId).FirstOrDefaultAsync();
                if (leaveBalance == null || leaveBalance.LeaveBalanceCount <= 0) return NotFound("No Leave Balance found");

                if (input.EndDate <= input.StartDate)
                {
                    return BadRequest("End Should be greater than the start Date");
                }
                var leaveCount = (input.EndDate.Date - input.StartDate.Date).Days;
                if(leaveCount > leaveBalance.LeaveBalanceCount+leave.LeaveCount) return NotFound("Ineffient balance");
               
                //leave balance update
                leaveBalance.LeaveBalanceCount = leaveBalance.LeaveBalanceCount+leave.LeaveCount - leaveCount;
                _applicationDb.Update(leaveBalance);
                //leave update
                leave.StartDate = input.StartDate;
                leave.EndDate = input.EndDate;
                leave.LeaveStatus = input.LeaveStatus;
                leave.LeaveCount = leaveCount;
                leave.LeaveTypeId = input.LeaveTypeId;
                _applicationDb.Update(leave);
                await _applicationDb.SaveChangesAsync();

                return Ok(leave);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
