using Proyecto2.Models;

namespace Proyecto2.Utils;

//Arbol n-ario de categorias

public class NaryTree
{
    public CategoryNode Root { get; set; }

    public NaryTree() => Root = new CategoryNode("Catalogo");

    public void Reset() => Root = new CategoryNode("Catalogo");

    public CategoryNode? Find(string name) => Find(Root, name);

    private CategoryNode? Find(CategoryNode node, string name)
    {
        if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return node;
        var child = node.FirstChild;
        while (child != null)
        {
            var found = Find(child, name);
            if (found != null) return found;
            child = child.NextSibling;
        }
        return null;
    }

    public bool AddCategory(string name, string? parentName)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (Find(name) != null) return false; // nombre único global

        var parent = string.IsNullOrWhiteSpace(parentName) ? Root : Find(parentName!);
        if (parent == null) return false;

        var newNode = new CategoryNode(name);
        InsertSorted(parent, newNode);
        return true;
    }

    private void InsertSorted(CategoryNode parent, CategoryNode newNode)
    {
        if (parent.FirstChild == null ||
            string.Compare(newNode.Name, parent.FirstChild.Name, StringComparison.OrdinalIgnoreCase) < 0)
        {
            newNode.NextSibling = parent.FirstChild;
            parent.FirstChild = newNode;
            return;
        }
        var prev = parent.FirstChild;
        while (prev.NextSibling != null &&
               string.Compare(prev.NextSibling.Name, newNode.Name, StringComparison.OrdinalIgnoreCase) < 0)
            prev = prev.NextSibling;
        newNode.NextSibling = prev.NextSibling;
        prev.NextSibling = newNode;
    }

    public string[] GetAllNames()
    {
        int total = CountNodes(Root);
        var arr = new string[total];
        int i = 0;
        Collect(Root, arr, ref i);
        return arr;
    }

    private int CountNodes(CategoryNode n)
    {
        int c = 1;
        var child = n.FirstChild;
        while (child != null) { c += CountNodes(child); child = child.NextSibling; }
        return c;
    }

    private void Collect(CategoryNode n, string[] arr, ref int i)
    {
        arr[i++] = n.Name;
        var child = n.FirstChild;
        while (child != null) { Collect(child, arr, ref i); child = child.NextSibling; }
    }

    public AvlTree CollectSubtreeBooks(CategoryNode start)
    {
        var result = new AvlTree();
        CollectBooks(start, result);
        return result;
    }

    private void CollectBooks(CategoryNode n, AvlTree acc)
    {
        n.Books.InOrder(b => acc.Insert(b));
        var child = n.FirstChild;
        while (child != null) { CollectBooks(child, acc); child = child.NextSibling; }
    }

    public string ToDot(string? startAt = null)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  node [shape=box, style=\"filled,rounded\", fillcolor=\"#cfe2ff\", fontname=\"Helvetica\"];");
        sb.AppendLine("  edge [arrowhead=vee];");
        var start = startAt == null ? Root : (Find(startAt) ?? Root);
        int c = 0;
        BuildDot(start, sb, ref c);
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string BuildDot(CategoryNode n, System.Text.StringBuilder sb, ref int c)
    {
        var id = $"c{c++}";
        sb.AppendLine($"  {id} [label=\"{n.Name} ({n.Books.Count()})\"];");
        var child = n.FirstChild;
        while (child != null)
        {
            var cid = BuildDot(child, sb, ref c);
            sb.AppendLine($"  {id} -> {cid};");
            child = child.NextSibling;
        }
        return id;
    }
}