using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs
{
    // Auth DTOs
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // Employee or Manager
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }

    // User DTOs
    public class UserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Task DTOs
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public string Status { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        [Required]
        public string Status { get; set; } // Pending, InProgress, Done
    }

    public class TaskDto
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public int AssignedTo { get; set; }
        public string AssignedToName { get; set; }
    }

    // Task Update DTOs
    public class CreateTaskUpdateDto
    {
        [Required]
        public string Update_text { get; set; }

        public string? AttachmentURL { get; set; }
    }

    public class TaskUpdateDto
    {
        public int UpdateID { get; set; }
        public int TaskID { get; set; }
        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public string Update_text { get; set; }
        public string? AttachmentURL { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    // Task Review DTOs
    public class CreateTaskReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comments { get; set; }
    }

    public class UpdateTaskReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comments { get; set; }
    }

    public class TaskReviewDto
    {
        public int ReviewID { get; set; }
        public int TaskID { get; set; }
        public int ReviewedBy { get; set; }
        public string ReviewerName { get; set; }
        public string Comments { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    // Response DTOs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }
    }
}