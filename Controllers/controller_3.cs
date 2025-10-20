using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class ReviewsController : ControllerBase
    {
        private readonly ITaskReviewService _taskReviewService;

        public ReviewsController(ITaskReviewService taskReviewService)
        {
            _taskReviewService = taskReviewService;
        }

        /// <summary>
        /// Update a task review (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateTaskReviewDto updateDto)
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

            var response = await _taskReviewService.UpdateTaskReviewAsync(id, updateDto);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Delete a task review (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var response = await _taskReviewService.DeleteTaskReviewAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}