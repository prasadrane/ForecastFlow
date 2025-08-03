using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForecastFlow.Core.Interfaces;
using ForecastFlow.Core.Models;
using System.Collections.Generic;
namespace ForecastFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IAppTaskRepository _taskRepository;
        private readonly IAppUserRepository _userRepository;

        public TasksController(IAppTaskRepository taskRepository, IAppUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        // GET: api/tasks
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppTask>>> GetTasks()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized();

            var tasks = await _taskRepository.GetByUserIdAsync(userId.Value);
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AppTask>> GetTask(int id)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized();

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null || task.UserId != userId)
                return NotFound();

            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AppTask>> CreateTask([FromBody] TaskDto dto)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized();

            var task = new AppTask
            {
                Title = dto.Title,
                Description = dto.Description,
                LocationName = dto.LocationName,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                TaskDateTime = dto.TaskDateTime,
                UserId = userId.Value,
                IsCompleted = dto.IsCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Priority = dto.Priority,
                Category = dto.Category,
                ReminderDateTime = dto.ReminderDateTime
            };

            await _taskRepository.AddAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto dto)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized();

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null || task.UserId != userId)
                return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.LocationName = dto.LocationName;
            task.Latitude = dto.Latitude;
            task.Longitude = dto.Longitude;
            task.TaskDateTime = dto.TaskDateTime;
            task.IsCompleted = dto.IsCompleted;
            task.UpdatedAt = DateTime.UtcNow;
            task.Priority = dto.Priority;
            task.Category = dto.Category;
            task.ReminderDateTime = dto.ReminderDateTime;

            await _taskRepository.UpdateAsync(task);
            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized();

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null || task.UserId != userId)
                return NotFound();

            await _taskRepository.DeleteAsync(id);
            return NoContent();
        }

        // Helper method to get authenticated user's ID (assumes claim "sub" or "id" is present)
        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == "id" || c.Type.EndsWith("nameidentifier"));
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }
    }

    // DTO for task creation and update
    public class TaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime TaskDateTime { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int? Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? ReminderDateTime { get; set; }
    }
}