using System.Linq.Expressions;
using APIBaseLib.Models;

namespace APIBaseLib.Repositories
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<dynamic> GetAllAsync(int page = 1, int pageSize = 10,
                                  Expression<Func<TEntity, bool>> whereClause = null,
                                  params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> GetByIdAsync(int id, Expression<Func<TEntity, bool>> whereClause = null,
                                  params Expression<Func<TEntity, object>>[] includes);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task UpdateFieldsAsync(TEntity entity, Dictionary<string, object> fieldsToUpdate);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}