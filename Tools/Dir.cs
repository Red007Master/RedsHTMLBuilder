using System.Reflection;

public class Dir
{
    public static void CreateAllDirsInObject<T>(T inputObjectOfClass)
    {
        foreach (PropertyInfo propertyInfo in inputObjectOfClass.GetType().GetProperties())
        {
            var value = propertyInfo.GetValue(inputObjectOfClass);

            if (value is string)
            {
                string path = (string)value;

                if (!Directory.Exists(path) && IsItDirectory(path))
                    Directory.CreateDirectory(path);
            }
        }
    }

    private static bool IsItDirectory(string path)
    {
        string[] buffer = path.Split(Convert.ToChar(Path.DirectorySeparatorChar));

        return !buffer[buffer.Length - 1].Contains("."); //kinda ugly but who the duck gonna have dir like "hello.name"?
    }
}