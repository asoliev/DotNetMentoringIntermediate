using CatalogService.DTOs;
using CatalogService.Models;
using CatalogService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger = logger;

    /// <summary> Get all categories </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary> Get a category by id </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    /// <summary> Create a new category </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        var createdCategory = await categoryService.CreateCategoryAsync(category);
        return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
    }

    /// <summary> Update an existing category </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = new Category
        {
            Id = id,
            Name = request.Name,
            Description = request.Description
        };

        var updatedCategory = await categoryService.UpdateCategoryAsync(id, category);
        if (updatedCategory == null)
            return NotFound();

        return Ok(updatedCategory);
    }

    /// <summary> Delete a category and all its related items </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await categoryService.DeleteCategoryAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}