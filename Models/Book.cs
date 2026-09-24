namespace Proyecto2.Models;

public class Book
{
    public int Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public Book() { }
    public Book(int isbn, string title, string author, string category)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
        Category = category;
    }
}