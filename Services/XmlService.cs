using System.Xml.Linq;
using Proyecto2.Models;
using Proyecto2.Utils;

namespace Proyecto2.Services;

public class XmlService
{
    private readonly CatalogService _catalog;
    public XmlService(CatalogService catalog) => _catalog = catalog;

    public LoadResult LoadFromString(string xml) => Load(XDocument.Parse(xml));

    private LoadResult Load(XDocument doc)
    {
        var root = doc.Root;
        if (root == null) return new LoadResult();

        // ─── 1) Parsear categorías a un arreglo (sin List) ───
        var catsEl = root.Element("listaCategorias") ?? root.Element("listaCategorías");
        int totalCats = catsEl == null ? 0 : CountElements(catsEl, "categoria");
        var cats = new CategoryInput[totalCats];
        int c = 0;
        if (catsEl != null)
        {
            foreach (var el in catsEl.Elements("categoria"))
            {
                var name = el.Value.Trim();
                var parent = ((string?)el.Attribute("padre"))?.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                cats[c++] = new CategoryInput { Name = name, Parent = parent };
            }
        }

        // ─── 2) Insertar con linking diferido ───
        var res = _catalog.Categories.AddCategoriesBatch(cats, c);

        // ─── 3) Libros ───
        var booksEl = root.Element("listaLibros");
        if (booksEl != null)
        {
            foreach (var l in booksEl.Elements("libro"))
                ProcessBook(l, res);
        }

        return res;
    }

    private static int CountElements(XElement parent, string tag)
    {
        int n = 0;
        foreach (var _ in parent.Elements(tag)) n++;
        return n;
    }

    private void ProcessBook(XElement l, LoadResult res)
    {
        // 3.1) ISBN
        var isbnText = (l.Element("ISBN")?.Value
                     ?? l.Element("isbn")?.Value
                     ?? "").Trim();

        if (!int.TryParse(isbnText, out int isbn))
        {
            res.AddDetail($"ISBN invalido '{isbnText}'");
            res.BooksRejected++;
            return;
        }

        // 3.2) Duplicado
        if (_catalog.Books.Search(isbn) != null)
        {
            res.AddDetail($"ISBN {isbn} duplicado, se ignora");
            res.BooksRejected++;
            return;
        }

        // 3.3) Datos
        var title    = (l.Element("titulo")?.Value    ?? "").Trim();
        var author   = (l.Element("autor")?.Value     ?? "").Trim();
        var category = (l.Element("categoria")?.Value ?? "").Trim();

        // 3.4) Categoría existe
        if (_catalog.Categories.Find(category) == null)
        {
            res.AddDetail($"libro {isbn}: categoria '{category}' no existe");
            res.BooksRejected++;
            return;
        }

        // 3.5) Agregar
        var book = new Book(isbn, title, author, category);
        _catalog.AddBook(book);
        res.AddDetail($"libro {isbn} agregado a '{category}'");
        res.BooksAdded++;
    }
}