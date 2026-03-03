using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OnlineLearningPlatform.DataAccess.IRepositories;
using System.Linq;
using System.Linq.Expressions;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public readonly DbSet<T> _dbSet;
        public readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(List<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {

            IQueryable<T> query = _dbSet;
            if (include != null)
            {
                query = include(query);
            }
            return await query.FirstOrDefaultAsync(filter);

        }
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter)
        {
            if (filter != null) return await _dbSet.Where(filter).ToListAsync();
            return await _dbSet.ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _dbSet;
            return await query.AnyAsync(filter);
        }

        public async Task RemoveIdAsync(Guid id)
        {
            T? exisiting = await _dbSet.FindAsync(id);
            if (exisiting != null)
            {
                _dbSet.Remove(exisiting);
            }
            else throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id '{id}' was not found.");

        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,
                                               Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                               int? pageIndex = null,
                                               int? pageSize = null)
        {
            IQueryable<T> query = _dbSet;


            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }
            if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
            {
                int skip = (pageIndex.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync() => await _dbSet.CountAsync();
        public async Task<int> CountAsync(Expression<Func<T, bool>> filter) => await _dbSet.CountAsync(filter);
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public async Task<int> CountTotalPaging(Expression<Func<T, bool>>? filter = null)
        {

            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
