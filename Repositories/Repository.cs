using Microsoft.EntityFrameworkCore;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Data;

namespace Spleet.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SpleetDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(SpleetDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetById(Guid id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAll() => await _dbSet.ToListAsync();
        public async Task Add(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}