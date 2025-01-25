using EmployeeLeaveManagement.Domain.ApplicationDbContext;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeManagement4.Domain.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDb _applicationDb;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDb applicationDb,IConfiguration configuration)
        {
            _applicationDb = applicationDb;
            _configuration = configuration;
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> Register(RegisterDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _applicationDb.Users.AnyAsync(u => u.Email == input.Email))
            {
                return Conflict("Email already exists.");
            }

            // Create user entity
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
                var userData=await _applicationDb.Users.AddAsync(user);
                await _applicationDb.SaveChangesAsync();

                // Return DTO without exposing sensitive data
                var userDto = new RegisterDto
                {

                    Name = user.Name,
                    Email = user.Email
                };

                return CreatedAtAction(nameof(Register), new { name = user.Name }, userData);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user= await _applicationDb.Users.Include(u=>u.UserType).FirstOrDefaultAsync(u => u.Email == input.Email && u.Password==input.Password);
                if (user != null)
                {
                    var token = GenerateToken(user);
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized("User name or Password invalid");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occured during process your request");
            }
                
        }
        private string GenerateToken(User user)
        {
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.UserType?.Name ?? "user")
    };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);  // Ensure that this is returning a valid JWT string
        }


    }


}

