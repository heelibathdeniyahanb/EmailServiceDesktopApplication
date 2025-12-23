using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmailServiceApplication.Services;
using EmailServiceApplication.Models;
using Microsoft.AspNetCore.Authorization;


namespace EmailServiceApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var user = new User
            {
                Username = req.Username,
                FullName = req.FullName,
                Email = req.Email,
                Role = req.Role ?? "User",
                IsActive = true,
                Department = req.Department
            };
            var created = await _userService.CreateAsync(user, req.Password);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _userService.LoginAsync(req.Username, req.Password);

            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var (token, user) = result.Value;

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Department
                }
            });
        }


        [HttpGet]
        [Route("{id:int}")]
       // [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
       // [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var result = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                FullName = u.FullName,
                u.Department,
                u.Role,
                u.IsActive,
                SentEmails = u.SentEmails?.Select(e => new EmailSummary(e.Id, e.Subject, e.SentAt)).ToArray() ?? Array.Empty<EmailSummary>(),
                ReceivedEmails = u.ReceivedEmails?.Select(er => new ReceivedSummary(er.Id, er.IsRead, er.ReadAt, er.Email != null ? new EmailSummary(er.Email.Id, er.Email.Subject, er.Email.SentAt) : null)).ToArray() ?? Array.Empty<ReceivedSummary>(),
                DepartmentGroups = u.DepartmentGroups?.Select(g => new GroupSummary(g.Id, g.Name)).ToArray() ?? Array.Empty<GroupSummary>()
            });

            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var ok = await _userService.UpdateAsync(user);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _userService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        public record RegisterRequest(string Username, string Password, string FullName, string Email, string? Role, string? Department);
        public record LoginRequest(string Username, string Password);
        private record EmailSummary(int Id, string Subject, DateTime SentAt);
        private record ReceivedSummary(int Id, bool IsRead, DateTime? ReadAt, EmailSummary? Email);
        private record GroupSummary(int Id, string Name);
    }
}
