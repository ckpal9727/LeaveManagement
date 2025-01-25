using EmployeeLeaveManagement.Domain.ApplicationDbContext;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeManagement4.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement4.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminUserManagementController : ControllerBase
    {
        private readonly ApplicationDb _applicationDb;

        public AdminUserManagementController(ApplicationDb applicationDb)
        {
            _applicationDb = applicationDb;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                var user= await _applicationDb.Users.Include(u=>u.UserType).Select(user=>new { UserId = user.Id,UserName=user.Name,Email=user.Email,UserType=user.UserType.Name}).ToListAsync();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllUserType")]
        public async Task<IActionResult> GetAllUserTypes()
        {
            try
            {
                var userTypes = await _applicationDb.UserTypes.Select(userType => new { UserId = userType.Id, UserTypeName = userType.Name }).ToListAsync();

                return Ok(userTypes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser(RegisterDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(await _applicationDb.Users.AnyAsync(u=>u.Email == input.Email))
            {
                return Conflict("User already Existe");
            }
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = input.Name,
                Email = input.Email,
                //Password = _passwordHasher.HashPassword(null, input.Password) // Hash the password
                Password = input.Password// Hash the password
            };
            try
            {

                await _applicationDb.Users.AddAsync(user);
                await _applicationDb.SaveChangesAsync();

                // Return DTO without exposing sensitive data
                var userDto = new RegisterDto
                {

                    Name = user.Name,
                    Email = user.Email
                };
                return CreatedAtAction(nameof(AddUser),new { name= user.Name}, userDto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
        [HttpPost("updateUser/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, UserUpdateDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user= await _applicationDb.Users.FirstOrDefaultAsync(u=>u.Id == userId);
                user.Email = input.Email;
                user.Password = input.Password;
                user.Name = input.Name;
                 _applicationDb.Users.Update(user);
                await _applicationDb.SaveChangesAsync();

                var userDto = new 
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                };
                return Ok(user);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("addUserType")]
        public async Task<IActionResult> AddUserType(UserTypeDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userType = new UserType()
            {
                Id= Guid.NewGuid(),
                Name = input.Name,
            };
            try
            {
                var result = await _applicationDb.UserTypes.AddAsync(userType);
                await _applicationDb.SaveChangesAsync();

                
                return CreatedAtAction(nameof(AddUserType), userType);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                // Find the user by ID
                var user = await _applicationDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    // Return 404 if user not found
                    return NotFound($"User with ID {userId} not found.");
                }

                // Remove user
                _applicationDb.Users.Remove(user);
                await _applicationDb.SaveChangesAsync();

                // Return 204 No Content
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                // Return 500 Internal Server Error
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}
