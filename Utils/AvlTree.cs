using Proyecto2.Models;

namespace Proyecto2.Utils;

//Árbol AVL de libros por ISBN)


public delegate void BookAction(Book book);

public class BookNode
{
    public Book Book { get; set; }
    public BookNode? Left { get; set; }
    public BookNode? Right { get; set; }
    public int Height { get; set; } = 1;
    public BookNode(Book b) => Book = b;
}

public class AvlTree
{
    public BookNode? Root { get; private set; }

    public void Clear() => Root = null;

    public void Insert(Book book) => Root = Insert(Root, book);

    private BookNode Insert(BookNode? n, Book book)
    {
        if (n == null) return new BookNode(book);
        if (book.Isbn < n.Book.Isbn) n.Left = Insert(n.Left, book);
        else if (book.Isbn > n.Book.Isbn) n.Right = Insert(n.Right, book);
        else return n;

        n.Height = 1 + Math.Max(H(n.Left), H(n.Right));
        int bal = Balance(n);

        if (bal > 1 && book.Isbn < n.Left!.Book.Isbn) return RotR(n);
        if (bal < -1 && book.Isbn > n.Right!.Book.Isbn) return RotL(n);
        if (bal > 1 && book.Isbn > n.Left!.Book.Isbn) { n.Left = RotL(n.Left!); return RotR(n); }
        if (bal < -1 && book.Isbn < n.Right!.Book.Isbn) { n.Right = RotR(n.Right!); return RotL(n); }
        return n;
    }

    public Book? Search(int isbn)
    {
        var cur = Root;
        while (cur != null)
        {
            if (isbn == cur.Book.Isbn) return cur.Book;
            cur = isbn < cur.Book.Isbn ? cur.Left : cur.Right;
        }
        return null;
    }

    public bool Delete(int isbn)
    {
        bool deleted = false;
        Root = Delete(Root, isbn, ref deleted);
        return deleted;
    }

    private BookNode? Delete(BookNode? n, int isbn, ref bool deleted)
    {
        if (n == null) return null;
        if (isbn < n.Book.Isbn) n.Left = Delete(n.Left, isbn, ref deleted);
        else if (isbn > n.Book.Isbn) n.Right = Delete(n.Right, isbn, ref deleted);
        else
        {
            deleted = true;
            if (n.Left == null) return n.Right;
            if (n.Right == null) return n.Left;
            var succ = MinValue(n.Right);
            n.Book = succ.Book;
            n.Right = Delete(n.Right, succ.Book.Isbn, ref deleted);
        }
        n.Height = 1 + Math.Max(H(n.Left), H(n.Right));
        int bal = Balance(n);
        if (bal > 1 && Balance(n.Left) >= 0) return RotR(n);
        if (bal > 1 && Balance(n.Left) < 0) { n.Left = RotL(n.Left!); return RotR(n); }
        if (bal < -1 && Balance(n.Right) <= 0) return RotL(n);
        if (bal < -1 && Balance(n.Right) > 0) { n.Right = RotR(n.Right!); return RotL(n); }
        return n;
    }

    public Book? Min() => Root == null ? null : MinValue(Root).Book;
    public Book? Max() => Root == null ? null : MaxValue(Root).Book;

    private BookNode MinValue(BookNode n) { while (n.Left != null) n = n.Left; return n; }
    private BookNode MaxValue(BookNode n) { while (n.Right != null) n = n.Right; return n; }

    private int H(BookNode? n) => n?.Height ?? 0;
    private int Balance(BookNode? n) => n == null ? 0 : H(n.Left) - H(n.Right);

    private BookNode RotR(BookNode y)
    {
        var x = y.Left!; var t = x.Right;
        x.Right = y; y.Left = t;
        y.Height = 1 + Math.Max(H(y.Left), H(y.Right));
        x.Height = 1 + Math.Max(H(x.Left), H(x.Right));
        return x;
    }

    private BookNode RotL(BookNode x)
    {
        var y = x.Right!; var t = y.Left;
        y.Left = x; x.Right = t;
        x.Height = 1 + Math.Max(H(x.Left), H(x.Right));
        y.Height = 1 + Math.Max(H(y.Left), H(y.Right));
        return y;
    }

    public int Count() => Count(Root);
    private int Count(BookNode? n) => n == null ? 0 : 1 + Count(n.Left) + Count(n.Right);

    public void InOrder(BookAction action) => InOrder(Root, action);
    private void InOrder(BookNode? n, BookAction action)
    {
        if (n == null) return;
        InOrder(n.Left, action);
        action(n.Book);
        InOrder(n.Right, action);
    }

    public string ToDot(string title = "Libros")
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  rankdir=TB;");
        sb.AppendLine($"  label=\"{title}\"; labelloc=t; fontsize=18;");
        sb.AppendLine("  node [shape=record, style=filled, fillcolor=\"#ffe9b3\"];");
        int c = 0;
        BuildDot(Root, sb, ref c);
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string? BuildDot(BookNode? n, System.Text.StringBuilder sb, ref int c)
    {
        if (n == null) return null;
        var id = $"b{c++}";
        sb.AppendLine($"  {id} [label=\"{{ISBN: {n.Book.Isbn}|{Esc(n.Book.Title)}|{Esc(n.Book.Author)}}}\"];");
        var l = BuildDot(n.Left, sb, ref c);
        if (l != null) sb.AppendLine($"  {id} -> {l} [label=\"L\"];");
        var r = BuildDot(n.Right, sb, ref c);
        if (r != null) sb.AppendLine($"  {id} -> {r} [label=\"R\"];");
        return id;
    }

    private static string Esc(string s) => s.Replace("\"", "\\\"");
}