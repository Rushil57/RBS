using System.IO;
using System.Reflection;

public class TestUtil
{
    public static string GetResourceFileAsString(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = filename;
        string result = string.Empty;

        using (Stream stream = assembly.GetManifestResourceStream("plweb.Resources." + resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            result = reader.ReadToEnd();
        }

        return result;
    }
}
