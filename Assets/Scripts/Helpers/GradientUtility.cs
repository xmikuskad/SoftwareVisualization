using UnityEngine;

public static class GradientUtility
{
    public static Color32 CreateGradient(float percentage, Color32 color)
    {
        // Convert the percentage value to a range from 0 to 1
        percentage = Mathf.Clamp01(percentage);

        if (percentage <= 0.00001f)
            return new Color32(255, 255, 255, 255);
        
        // Calculate the red, green, and blue components of the gradient color
        byte r = (byte)Mathf.Lerp(200, color.r, percentage);
        byte g = (byte)Mathf.Lerp(200, color.g, percentage);
        byte b = (byte)Mathf.Lerp(200, color.b, percentage);

        // Return the gradient color
        return new Color32(r, g, b, 255);
    }
}