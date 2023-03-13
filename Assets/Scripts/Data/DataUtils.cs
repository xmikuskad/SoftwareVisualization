using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataUtils
{
    public static string GetFormattedPropertyName(string property)
    {
        return string.Join("", property.ToLower().Split(' ')
            .Select((word, index) => index == 0 
                ? char.ToLower(word[0]) + word.Substring(1)
                : char.ToUpper(word[0]) + word.Substring(1)));
    }
    
    public static T DeepClone<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }
}