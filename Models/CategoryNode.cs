using Proyecto2.Utils;

namespace Proyecto2.Models;

public class CategoryNode
{
    public string Name { get; set; }
    public CategoryNode? FirstChild { get; set; }
    public CategoryNode? NextSibling { get; set; }
    public AvlTree Books { get; } = new();

    public CategoryNode(string name) => Name = name;
}