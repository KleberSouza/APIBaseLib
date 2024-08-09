using System.Linq.Expressions;
using APIBaseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace APIBaseLib.Repositories
{
    public class Repository<TEntity, TContext> : IRepository<TEntity>
            where TEntity : BaseEntity
            where TContext : DbContext
    {
        protected readonly TContext _context;

        public Repository(TContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetAllAsync(int page = 1, int pageSize = 10,
                                               Expression<Func<TEntity, bool>> whereClause = null,
                                               params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                var query = _context.Set<TEntity>().AsQueryable();

                if (whereClause != null)
                {
                    query = query.Where(whereClause);
                }

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var totalCount = await query.CountAsync();
                var entities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new
                {
                    Items = entities,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve data.", ex);
            }
        }

        public async Task<TEntity> GetByIdAsync(int id, Expression<Func<TEntity, bool>> whereClause = null,
                                  params Expression<Func<TEntity, object>>[] includes)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0.");

            try
            {
                var query = _context.Set<TEntity>().AsQueryable();

                if (whereClause != null)
                {
                    query = query.Where(whereClause);
                }

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var entity = await query.FirstOrDefaultAsync(e => e.Id == id)
                    ?? throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {id} not found.");

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve entity with ID {id}.", ex);
            }
        }

        public async Task AddAsync(TEntity entity)
        {
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add entity.", ex);
            }
        }

        public async Task UpdateAsync(TEntity entity)
        {
            try
            {
                _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update entity with ID {entity.Id}.", ex);
            }
        }

        public async Task UpdateFieldsAsync(TEntity entity, Dictionary<string, object> fieldsToUpdate)
        {
            try
            {
                var dbEntity = await GetByIdAsync(entity.Id);

                foreach (var field in fieldsToUpdate)
                {
                    _context.Entry(dbEntity).Property(field.Key).CurrentValue = field.Value;
                    _context.Entry(dbEntity).Property(field.Key).IsModified = true;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update fields for entity with ID {entity.Id}.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete entity with ID {id}.", ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            try
            {
                return await _context.Set<TEntity>().AnyAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check existence of entity with ID {id}.", ex);
            }
        }
    }
}
