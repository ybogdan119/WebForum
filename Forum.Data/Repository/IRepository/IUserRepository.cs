using Forum.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Forum.Data.Repository.IRepository
{
    public interface IUserRepository
    {
        public Task<ICollection<ForumUser>> GetAllAsync(Expression<Func<ForumUser, bool>>? filter = null);
        public Task<ForumUser?> GetAsync(Expression<Func<ForumUser, bool>> filter);
        public Task<ICollection<ForumUser>> GetAllByRoleAsync(string role);
        public Task<List<string>> CreateAsync(ForumUser user, string password, string role);
        public Task<List<string>> UpdateAsync(ForumUser user, string? password, string? role);
        public Task<List<string>> DeleteAsync(string id);
        public Task SaveAsync();
    }
}
