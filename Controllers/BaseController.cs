using APIBaseLib.Models;
using APIBaseLib.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIBaseLib.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<TEntity, TService> : ControllerBase
        where TEntity : BaseEntity
        where TService : IService<TEntity>
    {
        protected readonly TService _service;

        public BaseController(TService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a paginated list of all entities.
        /// </summary>
        /// <param name="page">The page number (default is 1).</param>
        /// <param name="pageSize">The page size (default is 10).</param>
        /// <returns>A paginated list of entities.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TEntity>>> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var entities = await _service.GetAllAsync(page, pageSize);
                return this.GenerateHateoasLinks(entities);
            }
            catch (Exception ex)
            {
                return this.HandleException(ex);
            }
        }

        /// <summary>
        /// Retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The requested entity.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TEntity>> GetById(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                return this.GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return this.HandleException(ex);
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The created entity.</returns>
        [HttpPost]
        public async Task<ActionResult<TEntity>> Create(TEntity entity)
        {
            try
            {
                await _service.AddAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, this.GenerateHateoasLinks(entity));
            }
            catch (Exception ex)
            {
                return this.HandleException(ex);
            }
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="entity">The updated entity.</param>
        /// <returns>The updated entity.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<TEntity>> Update(int id, TEntity entity)
        {
            try
            {
                entity.Id = id;
                await _service.UpdateAsync(entity);
                return this.GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return this.HandleException(ex);
            }
        }

        /// <summary>
        /// Updates specific fields of an existing entity.
        /// </summary>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="fieldsToUpdate">The dictionary of fields to update.</param>
        /// <returns>The updated entity.</returns>
        [HttpPatch("{id}")]
        public async Task<ActionResult<TEntity>> UpdateFields(int id, Dictionary<string, object> fieldsToUpdate)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                await _service.UpdateFieldsAsync(entity, fieldsToUpdate);
                return this.GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return this.HandleException(ex);
            }
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A no-content response.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        private ActionResult<T> GenerateHateoasLinks<T>(T entity)
        {
            var entityType = typeof(T);
            var idProperty = entityType.GetProperty("Id");

            var result = new
            {
                data = entity,
                links = new
                {
                    self = Url.Action("GetById", new { id = idProperty?.GetValue(entity) }),
                    update = Url.Action("Update", new { id = idProperty?.GetValue(entity) }),
                    delete = Url.Action("Delete", new { id = idProperty?.GetValue(entity) })
                }
            };

            return Ok(result);
        }

        private ActionResult<IEnumerable<T>> GenerateHateoasLinks<T>(IEnumerable<T> entities)
        {
            var result = new
            {
                data = entities,
                links = new
                {
                    self = Url.Action("GetAll")
                }
            };

            return Ok(result);
        }

        private ActionResult HandleException(Exception ex)
        {
            switch (ex)
            {
                case ArgumentException argEx:
                    return BadRequest(argEx.Message);
                case KeyNotFoundException notFoundEx:
                    return NotFound(notFoundEx.Message);
                default:
                    return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}