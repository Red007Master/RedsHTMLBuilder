
namespace RedsNodeTree;

public class NodeTreeGenerator
{
    // Constants for drawing lines and spaces
    private const string _cross = " ├─";
    private const string _corner = " └─";
    private const string _vertical = " │ ";
    private const string _space = "&nbsp;&nbsp;";

    public string WriteNodeHTML(TreeNode node, string indent){
        string result = "";

        WriteNodeHTML(node, ref result, indent);

        return result;
    }

    private void WriteNodeHTML(TreeNode node, ref string outputString, string indent)
    {
        //Console.WriteLine(node.Name);

        if (node.Prefix == null || node.Prefix.Length == 0)
        {

        }
        else
        {
            outputString += node.Prefix + "<br>";
        }

        if (node.Color != null)
        {
            node.Title = $"<span style='color:{node.Color}'>{node.Title}</span>";
        }

        if (node.Comment == null || node.Comment.Length == 0)
        {
            outputString += node.Title + "<br>";
        }
        else
        {
            outputString += node.Title + indent + "(" + node.Comment + ")<br>";
        }

        // Loop through the children recursively, passing in the
        // indent, and the isLast parameter
        var numberOfChildren = node.Children.Count;
        for (var i = 0; i < numberOfChildren; i++)
        {
            var child = node.Children[i];
            var isLast = (i == (numberOfChildren - 1));
            WriteChildNodeHTML(child, ref outputString, indent, isLast);
        }
    }

    private void WriteChildNodeHTML(TreeNode node, ref string outputString, string indent, bool isLast)
    {
        // Print the provided pipes/spaces indent
        outputString += indent;

        // Depending if this node is a last child, print the
        // corner or cross, and calculate the indent that will
        // be passed to its children
        if (isLast)
        {
            outputString += _corner;

            indent += _space;
        }
        else
        {
            outputString += _cross;

            indent += _vertical;
        }

        WriteNodeHTML(node, ref outputString, indent);
    }
}