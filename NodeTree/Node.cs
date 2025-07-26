namespace RedsNodeTree;

public class TreeNode
{
    public TreeNode(string name)
    {
        Title = name;
    }

    public string? Color { get; set; } = null;

    public string Title { get; set; }
    public List<TreeNode> Children { get; } = new List<TreeNode>();

    public string? Comment { get; set; } = null;
    public string? Prefix { get; set; } = null;
}