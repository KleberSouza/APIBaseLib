using APIBaseLib.Models;

namespace APIBaseLib.Services
{
    public interface IService<TEntity>
            where TEntity : BaseEntity
    {
        Task<dynamic> GetAllAsync(int page, int pageSize);
        Task<TEntity> GetByIdAsync(int id);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task UpdateFieldsAsync(TEntity entity, Dictionary<string, object> fieldsToUpdate);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}