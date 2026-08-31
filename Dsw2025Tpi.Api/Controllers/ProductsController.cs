using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 1. Crear un producto  -> POST /api/products
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        var created = await _productService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }



    // ---------------------------------------------------------------------------------------------------------------------
    // 2. Obtener todos los productos (activos) con paginación
    //    GET /api/products?pageNumber=1&pageSize=10&search=termo
    //    Público (sin autenticación)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var paged = await _productService.GetAllAsync(pageNumber, pageSize, search);

        return Ok(paged);  // 200 con PagedResult (lanza excepción si no hay productos)
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 3. Obtener un producto por ID -> GET /api/products/{id}
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
            return NotFound();                // 404 si no existe

        return Ok(product);                   // 200
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 4. Actualizar un producto -> PUT /api/products/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateRequest request)
    {
        var updated = await _productService.UpdateAsync(id, request);

        if (updated is null)
            return NotFound();                // 404 si no existe

        return Ok(updated);                   // 200 con producto actualizado
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 5. Inhabilitar un producto -> PATCH /api/products/{id}/disable
    [HttpPatch("{id:guid}/disable")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _productService.DisableAsync(id);
        return NoContent();                   // 204 si fue exitoso
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 6. Habilitar un producto -> PATCH /api/products/{id}/enable
    [HttpPatch("{id:guid}/enable")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Enable(Guid id)
    {
        await _productService.EnableAsync(id);
        return NoContent();                   // 204 si fue exitoso
    }


    // ---------------------------------------------------------------------------------------------------------------------
    // 7. Listado paginado con filtro SOLO para administrador
    //    GET /api/products/admin?status=enabled&search=abc&pageNumber=1&pageSize=10
    [HttpGet("admin")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetPagedForAdmin([FromQuery] FilterProductRequest filter, CancellationToken cancellationToken)
    {
        var paged = await _productService.GetPagedAsync(filter, cancellationToken);

        return Ok(paged);  // 200 con PagedResult (lanza excepción si no hay productos)
    }


}
