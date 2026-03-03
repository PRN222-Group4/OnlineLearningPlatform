using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace OnlineLearningPlatform.DataAccess.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(List<T> entities);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter);

        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        Task RemoveIdAsync(Guid id);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,
                                               Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                               int? pageIndex = null,
                                               int? pageSize = null);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> filter);
        void Update(T entity);
        Task<int> CountTotalPaging(Expression<Func<T, bool>>? filter = null);
        IQueryable<T> GetQueryable();
    }
}
