namespace RedsHTMLBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Initalization.Start(args);

            using (TimeLogger tl = new TimeLogger("Work.MainVoid", LogLevel.Information, P.Logger, 1))
            {
                Work.MainVoid();
            }
        }
    }
}
