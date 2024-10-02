using System.Drawing;

namespace Kori.Users;

public class AvatarColor
{
    public AvatarColor()
    {
        Background = BackgroundColors[new Random().Next(BackgroundColors.Count)];
        Foreground = CalculateForegroundColor(Background);
    }
    
    public string Background { get; set; }
    public string Foreground { get; set; }

    static List<string> BackgroundColors =>
    [
        // generated from http://phrogz.net/css/distinct-colors.html 
        "#F0DE38", "#47CE6D", "#5696BE", "#7361E6", "#EF5DA8", "#EA281B", "#E17723",
        "#F9EF9B", "#9DE2B1", "#A3C5DA", "#BBB1F5", "#F4A6CE", "#FFB8B4", "#FFBE71"
    ];

    static string CalculateForegroundColor(string backgroundColor)
    {
        // derived from https://stackoverflow.com/a/1626175
        var color = ColorTranslator.FromHtml(backgroundColor);
        var hue = color.GetHue();

        var foreground = BackgroundColors.IndexOf(backgroundColor) < 7 // dark colors
            ? FromHSV(hue, 1, 0.4)
            : FromHSV(hue, 1, 0.6);

        return ColorTranslator.ToHtml(foreground);
    }

    static Color FromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }
}
