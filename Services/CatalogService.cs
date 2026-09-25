using Proyecto2.Models;
using Proyecto2.Utils;

namespace Proyecto2.Services;

public class CatalogService
{
    public NaryTree Categories { get; } = new();
    public AvlTree Books { get; } = new();

    public void Reset()
    {
        Categories.Reset();
        Books.Clear();
    }

    public bool AddCategory(string name, string? parent)
    {
        // Uso individual: 1 categoría
        var arr = new CategoryInput[1] { new CategoryInput { Name = name, Parent = parent } };
        var res = Categories.AddCategoriesBatch(arr, 1);
        return res.CategoriesAdded == 1;
    }

    public bool AddBook(Book book)
    {
        if (Books.Search(book.Isbn) != null) return false;
        var cat = Categories.Find(book.Category);
        if (cat == null) return false;
        Books.Insert(book);
        cat.Books.Insert(book);
        return true;
    }

    public bool DeleteBook(int isbn)
    {
        var book = Books.Search(isbn);
        if (book == null) return false;
        Books.Delete(isbn);
        Categories.Find(book.Category)?.Books.Delete(isbn);
        return true;
    }

    public Book? SearchBook(int isbn) => Books.Search(isbn);
    public Book? MinBook() => Books.Min();
    public Book? MaxBook() => Books.Max();
}