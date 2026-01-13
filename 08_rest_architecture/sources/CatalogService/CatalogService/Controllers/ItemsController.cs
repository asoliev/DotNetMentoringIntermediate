using CatalogService.DTOs;
using CatalogService.Models;
using CatalogService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController(IItemService itemService, ILogger<ItemsController> logger) : ControllerBase
{
    private readonly ILogger<ItemsController> _logger = logger;

    /// <summary>
    /// Get items with optional category filter and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Item>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Item>>> GetItems(
        [FromQuery] int? categoryId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
            return BadRequest("Page number must be greater than 0.");

        if (pageSize is < 1 or > 100)
            return BadRequest("Page size must be between 1 and 100.");

        var result = await itemService.GetItemsAsync(categoryId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary> Get an item by id </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Item>> GetItem(int id)
    {
        var item = await itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    /// <summary> Create a new item </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Item), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Item>> CreateItem([FromBody] CreateItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = new Item
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId
        };

        try
        {
            var createdItem = await itemService.CreateItemAsync(item);
            return CreatedAtAction(nameof(GetItem), new { id = createdItem.Id }, createdItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary> Update an existing item </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Item>> UpdateItem(int id, [FromBody] UpdateItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = new Item
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId
        };

        try
        {
            var updatedItem = await itemService.UpdateItemAsync(id, item);
            if (updatedItem == null)
                return NotFound();
            return Ok(updatedItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary> Delete an item </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var deleted = await itemService.DeleteItemAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}