using System.Linq;

public static class DataUtils
{
    public static string GetFormattedPropertyName(string property)
    {
        return string.Join("", property.ToLower().Split(' ')
            .Select((word, index) => index == 0 
                ? char.ToLower(word[0]) + word.Substring(1)
                : char.ToUpper(word[0]) + word.Substring(1)));
    }
}