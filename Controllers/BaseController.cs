using APIBaseLib.Models;
using APIBaseLib.Services;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var entities = await _service.GetAllAsync(page, pageSize);
                return GenerateHateoasLinks(entities);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The requested entity.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TEntity>> GetById(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                return GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The created entity.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TEntity>> Create(TEntity entity)
        {
            try
            {
                await _service.AddAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, GenerateHateoasLinks(entity));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="entity">The updated entity.</param>
        /// <returns>The updated entity.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TEntity>> Update(int id, TEntity entity)
        {
            try
            {
                entity.Id = id;
                await _service.UpdateAsync(entity);
                return GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Updates specific fields of an existing entity.
        /// </summary>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="fieldsToUpdate">The dictionary of fields to update.</param>
        /// <returns>The updated entity.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TEntity>> UpdateFields(int id, Dictionary<string, object> fieldsToUpdate)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                await _service.UpdateFieldsAsync(entity, fieldsToUpdate);
                return GenerateHateoasLinks(entity);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A no-content response.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public ActionResult<T> GenerateHateoasLinks<T>(T entity)
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

        public ActionResult<IEnumerable<T>> GenerateHateoasLinks<T>(IEnumerable<T> entities)
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

        public ActionResult HandleException(Exception ex)
        {
            var (statusCode, errorCode, message) = ex switch
            {
                ArgumentException argEx => (StatusCodes.Status400BadRequest, "INVALID_ARGUMENT", argEx.Message),
                KeyNotFoundException notFoundEx => (StatusCodes.Status404NotFound, "NOT_FOUND", notFoundEx.Message),
                _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An error occurred while processing the request.")
            };

            var response = new
            {
                errorCode,
                message
            };

            return StatusCode(statusCode, response);
        }
    }
}