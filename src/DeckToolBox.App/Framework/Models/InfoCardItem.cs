using System.Collections.Generic;

namespace DeckToolBox.App.Framework.Models;

public class InfoCardItem
{
    public string Icon { get; set; } = "📌";
    public string Title { get; set; } = string.Empty;
    public string BorderBrush { get; set; } = "#1F6FEB";
    public string HeaderForeground { get; set; } = "#58A6FF";
    public List<string> Points { get; set; } = new();

    public InfoCardItem() { }

    public InfoCardItem(string icon, string title, string borderBrush, string headerForeground, IEnumerable<string> points)
    {
        Icon = icon;
        Title = title;
        BorderBrush = borderBrush;
        HeaderForeground = headerForeground;
        Points.AddRange(points);
    }
}
