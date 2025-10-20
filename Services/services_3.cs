using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services
{
    // TaskUpdate Service
    public interface ITaskUpdateService
    {
        Task<ApiResponse<TaskUpdateDto>> CreateTaskUpdateAsync(int taskId, CreateTaskUpdateDto createDto, int userId);
        Task<ApiResponse<List<TaskUpdateDto>>> GetTaskUpdatesAsync(int taskId);
    }

    public class TaskUpdateService : ITaskUpdateService
    {
        private readonly ITaskUpdateRepository _taskUpdateRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskUpdateService(ITaskUpdateRepository taskUpdateRepository, ITaskRepository taskRepository)
        {
            _taskUpdateRepository = taskUpdateRepository;
            _taskRepository = taskRepository;
        }

        public async Task<ApiResponse<TaskUpdateDto>> CreateTaskUpdateAsync(int taskId, CreateTaskUpdateDto createDto, int userId)
        {
            var response = new ApiResponse<TaskUpdateDto>();

            // Verify task exists
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            // Verify user is assigned to the task
            if (task.AssignedTo != userId)
            {
                response.Success = false;
                response.Message = "You can only add updates to your own tasks.";
                return response;
            }

            var taskUpdate = new TaskUpdate
            {
                TaskID = taskId,
                UpdatedBy = userId,
                Update_text = createDto.Update_text,
                AttachmentURL = createDto.AttachmentURL
            };

            var createdUpdate = await _taskUpdateRepository.CreateAsync(taskUpdate);

            response.Success = true;
            response.Message = "Task update added successfully.";
            response.Data = MapToTaskUpdateDto(createdUpdate);

            return response;
        }

        public async Task<ApiResponse<List<TaskUpdateDto>>> GetTaskUpdatesAsync(int taskId)
        {
            var response = new ApiResponse<List<TaskUpdateDto>>();

            // Verify task exists
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            var updates = await _taskUpdateRepository.GetByTaskIdAsync(taskId);

            response.Success = true;
            response.Data = updates.Select(MapToTaskUpdateDto).ToList();

            return response;
        }

        private TaskUpdateDto MapToTaskUpdateDto(TaskUpdate update)
        {
            return new TaskUpdateDto
            {
                UpdateID = update.UpdateID,
                TaskID = update.TaskID,
                UpdatedBy = update.UpdatedBy,
                UpdatedByName = update.User?.FullName,
                Update_text = update.Update_text,
                AttachmentURL = update.AttachmentURL,
                UpdateDate = update.UpdateDate
            };
        }
    }

    // TaskReview Service
    public interface ITaskReviewService
    {
        Task<ApiResponse<TaskReviewDto>> CreateTaskReviewAsync(int taskId, CreateTaskReviewDto createDto, int reviewerId);
        Task<ApiResponse<List<TaskReviewDto>>> GetTaskReviewsAsync(int taskId);
        Task<ApiResponse<TaskReviewDto>> UpdateTaskReviewAsync(int reviewId, UpdateTaskReviewDto updateDto);
        Task<ApiResponse<bool>> DeleteTaskReviewAsync(int reviewId);
    }

    public class TaskReviewService : ITaskReviewService
    {
        private readonly ITaskReviewRepository _taskReviewRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskReviewService(ITaskReviewRepository taskReviewRepository, ITaskRepository taskRepository)
        {
            _taskReviewRepository = taskReviewRepository;
            _taskRepository = taskRepository;
        }

        public async Task<ApiResponse<TaskReviewDto>> CreateTaskReviewAsync(int taskId, CreateTaskReviewDto createDto, int reviewerId)
        {
            var response = new ApiResponse<TaskReviewDto>();

            // Verify task exists
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            // Only allow review if task is Done
            if (task.Status != "Done")
            {
                response.Success = false;
                response.Message = "You can only review completed tasks.";
                return response;
            }

            var taskReview = new TaskReview
            {
                TaskID = taskId,
                ReviewedBy = reviewerId,
                Comments = createDto.Comments,
                Rating = createDto.Rating
            };

            var createdReview = await _taskReviewRepository.CreateAsync(taskReview);

            response.Success = true;
            response.Message = "Task review added successfully.";
            response.Data = MapToTaskReviewDto(createdReview);

            return response;
        }

        public async Task<ApiResponse<List<TaskReviewDto>>> GetTaskReviewsAsync(int taskId)
        {
            var response = new ApiResponse<List<TaskReviewDto>>();

            // Verify task exists
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            var reviews = await _taskReviewRepository.GetByTaskIdAsync(taskId);

            response.Success = true;
            response.Data = reviews.Select(MapToTaskReviewDto).ToList();

            return response;
        }

        public async Task<ApiResponse<TaskReviewDto>> UpdateTaskReviewAsync(int reviewId, UpdateTaskReviewDto updateDto)
        {
            var response = new ApiResponse<TaskReviewDto>();

            var review = await _taskReviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                response.Success = false;
                response.Message = "Review not found.";
                return response;
            }

            review.Comments = updateDto.Comments;
            review.Rating = updateDto.Rating;

            var updatedReview = await _taskReviewRepository.UpdateAsync(review);

            response.Success = true;
            response.Message = "Review updated successfully.";
            response.Data = MapToTaskReviewDto(updatedReview);

            return response;
        }

        public async Task<ApiResponse<bool>> DeleteTaskReviewAsync(int reviewId)
        {
            var response = new ApiResponse<bool>();

            var deleted = await _taskReviewRepository.DeleteAsync(reviewId);
            if (!deleted)
            {
                response.Success = false;
                response.Message = "Review not found.";
                return response;
            }

            response.Success = true;
            response.Message = "Review deleted successfully.";
            response.Data = true;

            return response;
        }

        private TaskReviewDto MapToTaskReviewDto(TaskReview review)
        {
            return new TaskReviewDto
            {
                ReviewID = review.ReviewID,
                TaskID = review.TaskID,
                ReviewedBy = review.ReviewedBy,
                ReviewerName = review.Reviewer?.FullName,
                Comments = review.Comments,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate
            };
        }
    }
}