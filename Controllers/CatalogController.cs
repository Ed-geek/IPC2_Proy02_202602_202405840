using Microsoft.AspNetCore.Mvc;
using Proyecto2.Models;
using Proyecto2.Services;

namespace Proyecto2.Controllers;

public class CategoryDto
{
    public string Name { get; set; } = "";
    public string? Parent { get; set; }
}

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalog;
    private readonly XmlService _xml;
    private readonly GraphvizService _gv;

    public CatalogController(CatalogService c, XmlService x, GraphvizService g)
    {
        _catalog = c;
        _xml = x;
        _gv = g;
    }

    // ═══════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ═══════════════════════════════════════════════════════════

    [HttpPost("init")]
    public IActionResult Init()
    {
        _catalog.Reset();
        return Ok(new { message = "Sistema inicializado" });
    }

    // ═══════════════════════════════════════════════════════════
    // CARGA DE XML
    // ═══════════════════════════════════════════════════════════

    [HttpPost("load-xml")]
    public async Task<IActionResult> LoadXml(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Archivo no válido" });

        using var sr = new StreamReader(file.OpenReadStream());
        var xml = await sr.ReadToEndAsync();

        try
        {
            var res = _xml.LoadFromString(xml);
            return Ok(res);   // { categoriesAdded, categoriesRejected, booksAdded, booksRejected, details[] }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error XML: " + ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════
    // CATEGORÍAS
    // ═══════════════════════════════════════════════════════════

    [HttpGet("categories")]
    public IActionResult ListCategories() => Ok(_catalog.Categories.GetAllNames());

    [HttpPost("categories")]
    public IActionResult AddCategory([FromBody] CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Nombre requerido" });

        var ok = _catalog.AddCategory(dto.Name, dto.Parent);
        return ok
            ? Ok(new { message = "Categoría agregada" })
            : BadRequest(new { message = "Nombre duplicado o padre inexistente" });
    }

    [HttpGet("categories/dot")]
    public IActionResult CategoriesDot([FromQuery] string? start)
    {
        var dot = _catalog.Categories.ToDot(start);
        var url = _gv.Generate(dot, "cats_" + Guid.NewGuid().ToString("N"));
        return Ok(new { url });
    }

    [HttpGet("categories/{name}/books/dot")]
    public IActionResult CategoryBooksDot(string name)
    {
        var cat = _catalog.Categories.Find(name);
        if (cat == null)
            return NotFound(new { message = "Categoría no encontrada" });

        var subtree = _catalog.Categories.CollectSubtreeBooks(cat);
        var dot = subtree.ToDot($"Libros en '{name}'");
        var url = _gv.Generate(dot, "books_" + Guid.NewGuid().ToString("N"));
        return Ok(new { url });
    }

    // ═══════════════════════════════════════════════════════════
    // LIBROS
    // ═══════════════════════════════════════════════════════════
    // IMPORTANTE: las rutas específicas (min, max) van ANTES que
    // la ruta con parámetro {isbn:int} para evitar conflictos.

    [HttpGet("books/min")]
    public IActionResult MinBook()
    {
        var b = _catalog.MinBook();
        return b == null
            ? NotFound(new { message = "Catálogo vacío" })
            : Ok(b);
    }

    [HttpGet("books/max")]
    public IActionResult MaxBook()
    {
        var b = _catalog.MaxBook();
        return b == null
            ? NotFound(new { message = "Catálogo vacío" })
            : Ok(b);
    }

    [HttpPost("books")]
    public IActionResult AddBook([FromBody] Book b)
    {
        var ok = _catalog.AddBook(b);
        return ok
            ? Ok(new { message = "Libro agregado" })
            : BadRequest(new { message = "ISBN duplicado o categoría inexistente" });
    }

    [HttpGet("books/{isbn:int}")]
    public IActionResult GetBook(int isbn)
    {
        var b = _catalog.SearchBook(isbn);
        return b == null
            ? NotFound(new { message = "No encontrado" })
            : Ok(b);
    }

    [HttpDelete("books/{isbn:int}")]
    public IActionResult DeleteBook(int isbn)
    {
        return _catalog.DeleteBook(isbn)
            ? Ok(new { message = "Libro eliminado" })
            : NotFound(new { message = "Libro no encontrado" });
    }
}