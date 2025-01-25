using EmployeeLeaveManagement.Domain.ApplicationDbContext;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeManagement4.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace EmployeeManagement4.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLeaveManagementController : ControllerBase
    {
        private readonly ApplicationDb _applicationDb;

        public AdminLeaveManagementController(ApplicationDb applicationDb)
        {
            _applicationDb = applicationDb;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeaves()
        {
            try
            {
                var leaves = await _applicationDb.Leaves.Include(u => u.User).Include(lt => lt.LeaveType).Select(l => new
                {
                    Id = l.Id,
                    LeaveType = l.LeaveType.Name,
                    User = l.User.Name,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    LeaveStatus = l.LeaveStatus,
                    LeaveCount = l.LeaveCount
                }).ToListAsync();

                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GeAllLeaveTypes")]
        public async Task<IActionResult> GetAllLeaveTypes()
        {
            try
            {
                var leavetypes = await _applicationDb.LeavesTypes.Select(l => new
                {
                    Id = l.Id,
                    Name = l.Name
                }).ToListAsync();

                return Ok(leavetypes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GeAllLeaveBalance")]
        public async Task<IActionResult> GetAllLeaveBalance(Guid? balanceId)
        {
            try
            {
                var query = _applicationDb.LeaveBalances
                  .Include(lt => lt.LeaveType)
                  .Include(u => u.User)
                  .Select(l => new
                  {
                      Id = l.Id,
                      Name = l.User.Name,
                      LeaveType = l.LeaveType.Name,
                      Balance = l.LeaveBalanceCount
                  });

                // Conditionally apply the filter if balanceId is provided
                if (balanceId != null && balanceId != Guid.Empty)
                {
                    query = query.Where(l => l.Id == balanceId);
                }

                // Execute the query
                var leaveBalances = await query.ToListAsync();

                return Ok(leaveBalances);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("updateLeaveBalance/{leaveBalanceId}")]
        public async Task<IActionResult> UpdateLeaveBalance(Guid leaveBalanceId,LeaveBalanceUpdateDto input)
        {
            if (!await _applicationDb.LeaveBalances.AnyAsync(x => x.Id == leaveBalanceId)) return NotFound("Leave balance not found");
            if (!await _applicationDb.LeavesTypes.AnyAsync(x => x.Id == input.LeaveTypeId)) return NotFound("Leave Type is not found");
            if (!await _applicationDb.Users.AnyAsync(x => x.Id == input.UserId)) return NotFound("User is not found");
            try
            {
                var leaveBalance = await _applicationDb.LeaveBalances.FirstOrDefaultAsync(x => x.Id == leaveBalanceId);
                leaveBalance.LeaveBalanceCount = input.LeaveBalanceCount;
                leaveBalance.UserId = input.UserId;
                leaveBalance.LeaveTypeId = input.LeaveTypeId;
                _applicationDb.LeaveBalances.Update(leaveBalance);
                await _applicationDb.SaveChangesAsync();
                return Ok(leaveBalance);



            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("addLeaveType")]
        public async Task<IActionResult> AddLeaveType(LeaveTypeDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _applicationDb.LeavesTypes.AnyAsync(x => x.Name == input.Name)) return Conflict("Leave type is already Exist");
            var leaveType = new LeavesType()
            {
                Id = Guid.NewGuid(),
                Name = input.Name
            };
            try
            {
                var result = await _applicationDb.LeavesTypes.AddAsync(leaveType);
                await _applicationDb.SaveChangesAsync();
                return CreatedAtAction(nameof(AddLeaveType), leaveType);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPost("addLeaveBalance")]
        public async Task<IActionResult> AddLeaveBalance(LeaveBalanceDto input)
        {
            if (!await _applicationDb.LeavesTypes.AnyAsync(x => x.Id == input.LeaveTypeId)) return NotFound("Leave Type is not found");
            if (!await _applicationDb.Users.AnyAsync(x => x.Id == input.UserId)) return NotFound("User is not found");

            var leaveBalance = new LeaveBalance()
            {
                LeaveTypeId = input.LeaveTypeId,
                UserId = input.UserId,
                LeaveBalanceCount = input.LeaveBalanceCount
            };
            try
            {
                var result = await _applicationDb.LeaveBalances.AddAsync(leaveBalance);
                await _applicationDb.SaveChangesAsync();
                return CreatedAtAction(nameof(AddLeaveBalance), leaveBalance);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPost("updateLeaveOfUser/{leaveId}")]
        public async Task<IActionResult> UpdateLeaveOfUser(Guid leaveId,LeaveUpdateDto input)
        {
            if (!await _applicationDb.Leaves.AnyAsync(x => x.Id == leaveId)) return NotFound("Leave is not found");
            if (!await _applicationDb.LeavesTypes.AnyAsync(x => x.Id == input.LeaveTypeId)) return NotFound("Leave Type is not found");
            //if (!await _applicationDb.Users.AnyAsync(x => x.Id == input.UserId)) return NotFound("User is not found");
            try
            {
                var leave = await _applicationDb.Leaves.FirstOrDefaultAsync(x => x.Id == leaveId);
                leave.StartDate=input.StartDate;
                leave.EndDate=input.EndDate;
                //leave.UserId=input.UserId;
                leave.LeaveTypeId=input.LeaveTypeId;
                leave.LeaveStatus=input.LeaveStatus;
                leave.LeaveCount=input.LeaveCount;
                _applicationDb.Leaves.Update(leave);
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
