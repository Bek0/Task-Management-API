using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITaskUpdateService _taskUpdateService;
        private readonly ITaskReviewService _taskReviewService;

        public TasksController(
            ITaskService taskService,
            ITaskUpdateService taskUpdateService,
            ITaskReviewService taskReviewService)
        {
            _taskService = taskService;
            _taskUpdateService = taskUpdateService;
            _taskReviewService = taskReviewService;
        }

        /// <summary>
        /// Create a new task (Manager can assign to anyone, Employee can create for themselves)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Employee can only create tasks for themselves
            if (userRole == "Employee" && createTaskDto.AssignedTo != userId)
            {
                return Forbid();
            }

            var response = await _taskService.CreateTaskAsync(createTaskDto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetTaskById), new { id = response.Data.TaskID }, response);
        }

        /// <summary>
        /// Get all tasks (Manager sees all, Employee sees only their tasks)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var response = await _taskService.GetAllTasksAsync(userId, userRole);
            return Ok(response);
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var response = await _taskService.GetTaskByIdAsync(id, userId, userRole);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Update task (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _taskService.UpdateTaskAsync(id, updateTaskDto);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Delete task (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var response = await _taskService.DeleteTaskAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Update task status (Employee can update their own tasks)
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var response = await _taskService.UpdateTaskStatusAsync(id, statusDto, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Add update to a task (Employee only - for their own tasks)
        /// </summary>
        [HttpPost("{id}/updates")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> AddTaskUpdate(int id, [FromBody] CreateTaskUpdateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var response = await _taskUpdateService.CreateTaskUpdateAsync(id, createDto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetTaskUpdates), new { id }, response);
        }

        /// <summary>
        /// Get all updates for a task
        /// </summary>
        [HttpGet("{id}/updates")]
        public async Task<IActionResult> GetTaskUpdates(int id)
        {
            var response = await _taskUpdateService.GetTaskUpdatesAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Add review to a task (Manager only - for completed tasks)
        /// </summary>
        [HttpPost("{id}/reviews")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddTaskReview(int id, [FromBody] CreateTaskReviewDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var response = await _taskReviewService.CreateTaskReviewAsync(id, createDto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetTaskReviews), new { id }, response);
        }

        /// <summary>
        /// Get all reviews for a task
        /// </summary>
        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetTaskReviews(int id)
        {
            var response = await _taskReviewService.GetTaskReviewsAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}