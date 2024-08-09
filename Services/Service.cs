using APIBaseLib.Models;
using APIBaseLib.Repositories;
using Microsoft.EntityFrameworkCore;

namespace APIBaseLib.Services
{
    public class Service<TEntity, TContext> : IService<TEntity>
            where TEntity : BaseEntity
            where TContext : DbContext
    {
        protected readonly IRepository<TEntity> _repository;

        public Service(IRepository<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository), "Repository cannot be null.");
        }

        public virtual async Task<dynamic> GetAllAsync(int page = 1, int pageSize = 10)
        {
            ValidatePaginationParameters(page, pageSize);

            try
            {
                return await _repository.GetAllAsync(page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving entities.", ex);
            }
        }

        public virtual async Task<TEntity> GetByIdAsync(int id)
        {
            ValidateId(id);

            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {id} not found.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving entity with ID {id}.", ex);
            }
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            ValidateEntity(entity);

            try
            {
                await _repository.AddAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the entity.", ex);
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            ValidateEntity(entity);

            try
            {
                if (await _repository.ExistsAsync(entity.Id))
                {
                    await _repository.UpdateAsync(entity);
                }
                else
                {
                    throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {entity.Id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the entity with ID {entity.Id}.", ex);
            }
        }

        public virtual async Task UpdateFieldsAsync(TEntity entity, Dictionary<string, object> fieldsToUpdate)
        {
            ValidateEntity(entity);
            ValidateFieldsToUpdate(fieldsToUpdate);

            try
            {
                if (await _repository.ExistsAsync(entity.Id))
                {
                    await _repository.UpdateFieldsAsync(entity, fieldsToUpdate);
                }
                else
                {
                    throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {entity.Id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating fields for the entity with ID {entity.Id}.", ex);
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            ValidateId(id);

            try
            {
                if (await _repository.ExistsAsync(id))
                {
                    await _repository.DeleteAsync(id);
                }
                else
                {
                    throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the entity with ID {id}.", ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            ValidateId(id);

            try
            {
                return await _repository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while checking existence of the entity with ID {id}.", ex);
            }
        }

        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0.");
        }

        private void ValidateEntity(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
        }

        private void ValidatePaginationParameters(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");
        }

        private void ValidateFieldsToUpdate(Dictionary<string, object> fieldsToUpdate)
        {
            if (fieldsToUpdate == null || fieldsToUpdate.Count == 0)
                throw new ArgumentException("The 'fieldsToUpdate' dictionary cannot be null or empty.");
        }
    }
}
