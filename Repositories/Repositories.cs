using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories
{
    // User Repository
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }

    // Task Repository
    public interface ITaskRepository
    {
        Task<Models.Task> GetByIdAsync(int id);
        Task<List<Models.Task>> GetAllAsync();
        Task<List<Models.Task>> GetByUserIdAsync(int userId);
        Task<Models.Task> CreateAsync(Models.Task task);
        Task<Models.Task> UpdateAsync(Models.Task task);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status);
    }

    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Models.Task> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.TaskID == id);
        }

        public async Task<List<Models.Task>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .ToListAsync();
        }

        public async Task<List<Models.Task>> GetByUserIdAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedTo == userId)
                .ToListAsync();
        }

        public async Task<Models.Task> CreateAsync(Models.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(task.TaskID);
        }

        public async Task<Models.Task> UpdateAsync(Models.Task task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(task.TaskID);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            task.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // TaskUpdate Repository
    public interface ITaskUpdateRepository
    {
        Task<TaskUpdate> GetByIdAsync(int id);
        Task<List<TaskUpdate>> GetByTaskIdAsync(int taskId);
        Task<TaskUpdate> CreateAsync(TaskUpdate taskUpdate);
    }

    public class TaskUpdateRepository : ITaskUpdateRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskUpdateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskUpdate> GetByIdAsync(int id)
        {
            return await _context.TaskUpdates
                .Include(tu => tu.User)
                .FirstOrDefaultAsync(tu => tu.UpdateID == id);
        }

        public async Task<List<TaskUpdate>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskUpdates
                .Include(tu => tu.User)
                .Where(tu => tu.TaskID == taskId)
                .OrderByDescending(tu => tu.UpdateDate)
                .ToListAsync();
        }

        public async Task<TaskUpdate> CreateAsync(TaskUpdate taskUpdate)
        {
            _context.TaskUpdates.Add(taskUpdate);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(taskUpdate.UpdateID);
        }
    }

    // TaskReview Repository
    public interface ITaskReviewRepository
    {
        Task<TaskReview> GetByIdAsync(int id);
        Task<List<TaskReview>> GetByTaskIdAsync(int taskId);
        Task<TaskReview> CreateAsync(TaskReview taskReview);
        Task<TaskReview> UpdateAsync(TaskReview taskReview);
        Task<bool> DeleteAsync(int id);
    }

    public class TaskReviewRepository : ITaskReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskReview> GetByIdAsync(int id)
        {
            return await _context.TaskReviews
                .Include(tr => tr.Reviewer)
                .FirstOrDefaultAsync(tr => tr.ReviewID == id);
        }

        public async Task<List<TaskReview>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskReviews
                .Include(tr => tr.Reviewer)
                .Where(tr => tr.TaskID == taskId)
                .OrderByDescending(tr => tr.ReviewDate)
                .ToListAsync();
        }

        public async Task<TaskReview> CreateAsync(TaskReview taskReview)
        {
            _context.TaskReviews.Add(taskReview);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(taskReview.ReviewID);
        }

        public async Task<TaskReview> UpdateAsync(TaskReview taskReview)
        {
            _context.TaskReviews.Update(taskReview);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(taskReview.ReviewID);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.TaskReviews.FindAsync(id);
            if (review == null) return false;

            _context.TaskReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}