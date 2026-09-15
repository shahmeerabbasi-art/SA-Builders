using Microsoft.EntityFrameworkCore;
using SA_Builders.Data;

namespace SA_Builders.Services
{
    public interface IClientAccessGuard
    {
        // Returns true only if this user has an active ProjectAssignment
        // to this specific project. Every client-facing endpoint MUST call
        // this before returning any project-scoped data — never trust a
        // projectId passed in the URL alone.
        Task<bool> UserCanAccessProjectAsync(int userId, int projectId);

        // Returns every project ID this user is assigned to — used for
        // "list my projects" style endpoints.
        Task<List<int>> GetAssignedProjectIdsAsync(int userId);
    }

    public class ClientAccessGuard : IClientAccessGuard
    {
        private readonly AppDbContext _db;
        public ClientAccessGuard(AppDbContext db) { _db = db; }

        public async Task<bool> UserCanAccessProjectAsync(int userId, int projectId)
        {
            return await _db.ProjectAssignments
                .AnyAsync(pa => pa.UserId == userId && pa.ProjectId == projectId);
        }

        public async Task<List<int>> GetAssignedProjectIdsAsync(int userId)
        {
            return await _db.ProjectAssignments
                .Where(pa => pa.UserId == userId)
                .Select(pa => pa.ProjectId)
                .ToListAsync();
        }
    }
}