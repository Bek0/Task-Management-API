using TaskManagement.API.DTOs;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services
{
    public interface ITaskService
    {
        Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, int createdByUserId);
        Task<ApiResponse<List<TaskDto>>> GetAllTasksAsync(int userId, string userRole);
        Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int id, int userId, string userRole);
        Task<ApiResponse<TaskDto>> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);
        Task<ApiResponse<bool>> DeleteTaskAsync(int id);
        Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto statusDto, int userId);
    }

    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, int createdByUserId)
        {
            var response = new ApiResponse<TaskDto>();

            // Verify assigned user exists
            var assignedUser = await _userRepository.GetByIdAsync(createTaskDto.AssignedTo);
            if (assignedUser == null)
            {
                response.Success = false;
                response.Message = "Assigned user not found.";
                return response;
            }

            var task = new Models.Task
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                AssignedTo = createTaskDto.AssignedTo,
                DueDate = createTaskDto.DueDate,
                CreatedBy = createdByUserId,
                Status = "Pending"
            };

            var createdTask = await _taskRepository.CreateAsync(task);

            response.Success = true;
            response.Message = "Task created successfully.";
            response.Data = MapToTaskDto(createdTask);

            return response;
        }

        public async Task<ApiResponse<List<TaskDto>>> GetAllTasksAsync(int userId, string userRole)
        {
            var response = new ApiResponse<List<TaskDto>>();

            List<Models.Task> tasks;

            // Manager can see all tasks, Employee only their assigned tasks
            if (userRole == "Manager")
            {
                tasks = await _taskRepository.GetAllAsync();
            }
            else
            {
                tasks = await _taskRepository.GetByUserIdAsync(userId);
            }

            response.Success = true;
            response.Data = tasks.Select(MapToTaskDto).ToList();

            return response;
        }

        public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int id, int userId, string userRole)
        {
            var response = new ApiResponse<TaskDto>();
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            // Employee can only view their assigned tasks
            if (userRole == "Employee" && task.AssignedTo != userId)
            {
                response.Success = false;
                response.Message = "You don't have permission to view this task.";
                return response;
            }

            response.Success = true;
            response.Data = MapToTaskDto(task);

            return response;
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
        {
            var response = new ApiResponse<TaskDto>();
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            // Verify assigned user exists
            var assignedUser = await _userRepository.GetByIdAsync(updateTaskDto.AssignedTo);
            if (assignedUser == null)
            {
                response.Success = false;
                response.Message = "Assigned user not found.";
                return response;
            }

            // Validate status
            if (!IsValidStatus(updateTaskDto.Status))
            {
                response.Success = false;
                response.Message = "Invalid status. Must be 'Pending', 'InProgress', or 'Done'.";
                return response;
            }

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.AssignedTo = updateTaskDto.AssignedTo;
            task.DueDate = updateTaskDto.DueDate;
            task.Status = updateTaskDto.Status;

            var updatedTask = await _taskRepository.UpdateAsync(task);

            response.Success = true;
            response.Message = "Task updated successfully.";
            response.Data = MapToTaskDto(updatedTask);

            return response;
        }

        public async Task<ApiResponse<bool>> DeleteTaskAsync(int id)
        {
            var response = new ApiResponse<bool>();
            var deleted = await _taskRepository.DeleteAsync(id);

            if (!deleted)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            response.Success = true;
            response.Message = "Task deleted successfully.";
            response.Data = true;

            return response;
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto statusDto, int userId)
        {
            var response = new ApiResponse<TaskDto>();
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
            {
                response.Success = false;
                response.Message = "Task not found.";
                return response;
            }

            // Validate status
            if (!IsValidStatus(statusDto.Status))
            {
                response.Success = false;
                response.Message = "Invalid status. Must be 'Pending', 'InProgress', or 'Done'.";
                return response;
            }

            // Employee can only update their own tasks
            if (task.AssignedTo != userId)
            {
                response.Success = false;
                response.Message = "You can only update the status of your own tasks.";
                return response;
            }

            await _taskRepository.UpdateStatusAsync(id, statusDto.Status);
            var updatedTask = await _taskRepository.GetByIdAsync(id);

            response.Success = true;
            response.Message = "Task status updated successfully.";
            response.Data = MapToTaskDto(updatedTask);

            return response;
        }

        private bool IsValidStatus(string status)
        {
            return status == "Pending" || status == "InProgress" || status == "Done";
        }

        private TaskDto MapToTaskDto(Models.Task task)
        {
            return new TaskDto
            {
                TaskID = task.TaskID,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                CreatedBy = task.CreatedBy,
                CreatorName = task.Creator?.FullName,
                AssignedTo = task.AssignedTo,
                AssignedToName = task.AssignedUser?.FullName
            };
        }
    }
}