namespace RedsHTMLBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Initalization.Start();

            using (TimeLogger tl = new TimeLogger("Work.MainVoid", LogLevel.Information, P.Logger, 1))
            {
                Work.MainVoid();
            }
        }
    }
}
