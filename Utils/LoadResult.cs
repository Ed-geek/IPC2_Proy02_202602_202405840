namespace Proyecto2.Utils;

/// <summary>Arreglo dinámico de strings — TDA propio (sin List de C#).</summary>
public class StringBag
{
    private string[] _items = new string[32];
    private int _count = 0;

    public int Count => _count;

    public void Add(string s)
    {
        if (_count == _items.Length)
        {
            var bigger = new string[_items.Length * 2];
            for (int i = 0; i < _count; i++) bigger[i] = _items[i];
            _items = bigger;
        }
        _items[_count++] = s;
    }

    public string[] ToArray()
    {
        var r = new string[_count];
        for (int i = 0; i < _count; i++) r[i] = _items[i];
        return r;
    }
}

/// <summary>Resultado de una carga XML (contadores + detalles).</summary>
public class LoadResult
{
    public int CategoriesAdded { get; set; }
    public int CategoriesRejected { get; set; }
    public int BooksAdded { get; set; }
    public int BooksRejected { get; set; }

    private readonly StringBag _details = new();

    public string[] Details => _details.ToArray();

    public void AddDetail(string s) => _details.Add(s);
}

/// <summary>Par (nombre, padre) para procesar categorías en lote.</summary>
public struct CategoryInput
{
    public string Name;
    public string? Parent;
}