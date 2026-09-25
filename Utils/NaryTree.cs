using Proyecto2.Models;

namespace Proyecto2.Utils;

public class NaryTree
{
    public CategoryNode Root { get; private set; }
    public bool RootDeclared { get; private set; }
    private StringBag _names = new();   

    public NaryTree() => Root = new CategoryNode("Catalogo");

    public void Reset()
    {
        Root = new CategoryNode("Catalogo");
        RootDeclared = false;
        _names = new StringBag();       
    }

    // ─────────────────────────────────────────────────────────
    // Búsqueda / existencia
    // ─────────────────────────────────────────────────────────

    private bool NameExists(string name)
    {
        if (name == "Catalogo") return RootDeclared;
        var arr = _names.ToArray();
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == name) return true;
        return false;
    }

    public CategoryNode? Find(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name == "Catalogo") return RootDeclared ? Root : null;
        if (!NameExists(name)) return null;
        return Find(Root, name);
    }

    private CategoryNode? Find(CategoryNode node, string name)
    {
        if (node.Name.Equals(name, StringComparison.Ordinal)) return node;
        var child = node.FirstChild;
        while (child != null)
        {
            var found = Find(child, name);
            if (found != null) return found;
            child = child.NextSibling;
        }
        return null;
    }

    // ─────────────────────────────────────────────────────────
    // Carga masiva con linking diferido
    // ─────────────────────────────────────────────────────────

    public LoadResult AddCategoriesBatch(CategoryInput[] cats, int count)
    {
        var res = new LoadResult();

        // Cola inicial (arreglo de tamaño máximo = count)
        var pending = new CategoryInput[count];
        int pendingCount = 0;

        for (int i = 0; i < count; i++)
            ProcessCategory(cats[i], res, pending, ref pendingCount);

        // Iterar mientras haya pendientes Y progreso
        bool progress = true;
        while (progress && pendingCount > 0)
        {
            progress = false;
            var next = new CategoryInput[pendingCount];
            int nextCount = 0;

            for (int i = 0; i < pendingCount; i++)
            {
                int before = res.CategoriesAdded + res.CategoriesRejected;
                ProcessCategory(pending[i], res, next, ref nextCount);
                if (res.CategoriesAdded + res.CategoriesRejected > before)
                    progress = true;
            }

            pending = next;
            pendingCount = nextCount;
        }

        
        for (int i = 0; i < pendingCount; i++)
        {
            res.AddDetail($"categoria '{pending[i].Name}': el padre '{pending[i].Parent}' no existe, se ignora");
            res.CategoriesRejected++;
        }

        return res;
    }

    private void ProcessCategory(CategoryInput c, LoadResult res,
                                 CategoryInput[] pending, ref int pendingCount)
    {
        // 1) Duplicado
        if (NameExists(c.Name))
        {
            res.AddDetail($"categoria '{c.Name}' duplicada, se ignora");
            res.CategoriesRejected++;
            return;
        }

        // 2) Declaración de la raíz
        if (c.Name == "Catalogo" && string.IsNullOrEmpty(c.Parent))
        {
            RootDeclared = true;
            res.AddDetail("categoria raiz 'Catalogo' agregada");
            res.CategoriesAdded++;
            return;
        }

        // 3) Padre (default = Catalogo)
        string parentName = string.IsNullOrEmpty(c.Parent) ? "Catalogo" : c.Parent!;

        // 4) Padre aún no disponible → diferir
        if (parentName != "Catalogo" && !NameExists(parentName))
        {
            pending[pendingCount++] = c;
            return;
        }
        if (parentName == "Catalogo" && !RootDeclared)
        {
            pending[pendingCount++] = c;
            return;
        }

        // 5) Resolver padre e insertar
        var parentNode = parentName == "Catalogo" ? Root : Find(parentName);
        if (parentNode == null)
        {
            pending[pendingCount++] = c;
            return;
        }

        var newNode = new CategoryNode(c.Name);
        InsertSorted(parentNode, newNode);
        _names.Add(c.Name);
        res.AddDetail($"categoria '{c.Name}' agregada bajo '{parentName}'");
        res.CategoriesAdded++;
    }

    private void InsertSorted(CategoryNode parent, CategoryNode newNode)
    {
        if (parent.FirstChild == null ||
            string.Compare(newNode.Name, parent.FirstChild.Name,
                           StringComparison.OrdinalIgnoreCase) < 0)
        {
            newNode.NextSibling = parent.FirstChild;
            parent.FirstChild = newNode;
            return;
        }
        var prev = parent.FirstChild;
        while (prev.NextSibling != null &&
               string.Compare(prev.NextSibling.Name, newNode.Name,
                              StringComparison.OrdinalIgnoreCase) < 0)
            prev = prev.NextSibling;
        newNode.NextSibling = prev.NextSibling;
        prev.NextSibling = newNode;
    }

    // ─────────────────────────────────────────────────────────
    // Recorridos
    // ─────────────────────────────────────────────────────────

    public string[] GetAllNames()
    {
        var acc = new string[CountNodes(Root)];
        int i = 0;
        CollectArray(Root, acc, ref i);
        return acc;
    }

    private int CountNodes(CategoryNode n)
    {
        int c = 1;
        var child = n.FirstChild;
        while (child != null) { c += CountNodes(child); child = child.NextSibling; }
        return c;
    }

    private void CollectArray(CategoryNode n, string[] arr, ref int i)
    {
        arr[i++] = n.Name;
        var child = n.FirstChild;
        while (child != null) { CollectArray(child, arr, ref i); child = child.NextSibling; }
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

    // ─────────────────────────────────────────────────────────
    // Graphviz
    // ─────────────────────────────────────────────────────────

    public string ToDot(string? startAt = null)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  node [shape=box, style=\"filled,rounded\", fillcolor=\"#cfe2ff\", fontname=\"Helvetica\"];");
        sb.AppendLine("  edge [arrowhead=vee];");
        var start = string.IsNullOrEmpty(startAt) ? Root : (Find(startAt!) ?? Root);
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