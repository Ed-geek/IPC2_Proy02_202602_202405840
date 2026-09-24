using System.Xml.Linq;
using Proyecto2.Models;

namespace Proyecto2.Services;

public class XmlService
{
    private readonly CatalogService _catalog;
    public XmlService(CatalogService catalog) => _catalog = catalog;

    public string LoadFromString(string xml) => Load(XDocument.Parse(xml));

    private string Load(XDocument doc)
    {
        int cats = 0, libs = 0;
        var root = doc.Root;
        if (root == null) return "XML vacío";

        var catsEl = root.Element("listaCategorias") ?? root.Element("listaCategorías");
        if (catsEl != null)
        {
            foreach (var c in catsEl.Elements("categoria"))
            {
                var name = (string?)c.Attribute("nombre");
                var parent = (string?)c.Attribute("padre");
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (_catalog.AddCategory(name!, parent)) cats++;
            }
        }

        var libsEl = root.Element("listaLibros");
        if (libsEl != null)
        {
            foreach (var l in libsEl.Elements("libro"))
            {
                if (!int.TryParse((string?)l.Attribute("isbn"), out int isbn)) continue;
                var title = (string?)l.Attribute("titulo") ?? "";
                var author = (string?)l.Attribute("autor") ?? "";
                var cat = (string?)l.Attribute("categoria") ?? "";
                if (_catalog.AddBook(new Book(isbn, title, author, cat))) libs++;
            }
        }
        return $"Categorías agregadas: {cats} | Libros agregados: {libs}";
    }
}