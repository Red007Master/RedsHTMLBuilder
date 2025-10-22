namespace RedsHTMLBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // args = "--nocomp --prodout --gatoradd js,csharp1,canvas,jsNew,HTML_CSS,godot".Split(" ");

            Initalization.Start(args);

            using (TimeLogger tl = new TimeLogger("Work.MainVoid", LogLevel.Information, P.Logger, 1))
            {
                Work.MainVoid();
            }
        }
    }
}
