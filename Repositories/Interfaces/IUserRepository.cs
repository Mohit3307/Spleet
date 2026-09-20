using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmail(string email);
        Task<bool> EmailExists(string email);
    }
}
