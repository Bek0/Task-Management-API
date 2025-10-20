using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.API.Models
{
    // User Entity
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Employee or Manager

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<Task> CreatedTasks { get; set; }
        public ICollection<Task> AssignedTasks { get; set; }
        public ICollection<TaskUpdate> TaskUpdates { get; set; }
        public ICollection<TaskReview> TaskReviews { get; set; }
    }

    // Task Entity
    public class Task
    {
        [Key]
        public int TaskID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Done

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public int AssignedTo { get; set; }

        // Navigation Properties
        [ForeignKey("CreatedBy")]
        public User Creator { get; set; }

        [ForeignKey("AssignedTo")]
        public User AssignedUser { get; set; }

        public ICollection<TaskUpdate> TaskUpdates { get; set; }
        public ICollection<TaskReview> TaskReviews { get; set; }
    }

    // TaskUpdate Entity
    public class TaskUpdate
    {
        [Key]
        public int UpdateID { get; set; }

        [Required]
        public int TaskID { get; set; }

        [Required]
        public int UpdatedBy { get; set; }

        [Required]
        public string Update_text { get; set; }

        [MaxLength(500)]
        public string AttachmentURL { get; set; }

        public DateTime UpdateDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("TaskID")]
        public Task Task { get; set; }

        [ForeignKey("UpdatedBy")]
        public User User { get; set; }
    }

    // TaskReview Entity
    public class TaskReview
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        public int TaskID { get; set; }

        [Required]
        public int ReviewedBy { get; set; }

        public string Comments { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("TaskID")]
        public Task Task { get; set; }

        [ForeignKey("ReviewedBy")]
        public User Reviewer { get; set; }
    }
}