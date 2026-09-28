using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace DeckToolBox.App.Framework.Controls;

/// <summary>
/// 将内容文本中的轻量标记渲染为 TextBlock 内联元素：
///   **加粗**   `代码`
/// 行首的 "• " / "- " 按一级 / 二级列表缩进；以 "1. " 等序号开头的行加粗作为小标题。
/// 用法：&lt;TextBlock ctl:RichText.Markup="{Binding}" /&gt;
/// </summary>
public static class RichText
{
    public static readonly AttachedProperty<string?> MarkupProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, string?>("Markup", typeof(RichText));

    private static readonly IBrush CodeForeground = new SolidColorBrush(Color.Parse("#79C0FF"));
    private static readonly IBrush CodeBackground = new SolidColorBrush(Color.Parse("#1F2A37"));
    private static readonly FontFamily CodeFont = new("Cascadia Code, JetBrains Mono, Hack, Noto Sans Mono, monospace");

    static RichText()
    {
        MarkupProperty.Changed.AddClassHandler<TextBlock>((tb, e) => Render(tb, e.GetNewValue<string?>()));
    }

    public static string? GetMarkup(TextBlock element) => element.GetValue(MarkupProperty);
    public static void SetMarkup(TextBlock element, string? value) => element.SetValue(MarkupProperty, value);

    private static void Render(TextBlock tb, string? text)
    {
        tb.Inlines ??= new InlineCollection();
        tb.Inlines.Clear();
        if (string.IsNullOrEmpty(text)) return;

        var body = text;
        var indent = 0.0;
        var heading = false;

        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("- "))
        {
            indent = 36;
            body = "◦ " + trimmed[2..];
        }
        else if (trimmed.StartsWith("• "))
        {
            indent = 16;
        }
        else if (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && (trimmed[1] == '.' || (char.IsDigit(trimmed[1]) && trimmed.Length > 3 && trimmed[2] == '.')))
        {
            heading = true;
        }

        tb.Margin = new Thickness(indent, heading ? 6 : 1, 0, 1);

        // 序号行只把 "1. 小标题：" 部分加粗，冒号后的正文保持常规字重
        if (heading)
        {
            var colon = body.IndexOfAny(['：', ':']);
            body = colon > 0 && colon < body.Length - 1
                ? $"**{body[..(colon + 1)].Replace("**", "")}**{body[(colon + 1)..]}"
                : $"**{body.Replace("**", "")}**";
        }

        var bold = false;
        var code = false;
        var start = 0;
        for (var i = 0; i < body.Length; i++)
        {
            if (!code && i + 1 < body.Length && body[i] == '*' && body[i + 1] == '*')
            {
                Flush(i);
                bold = !bold;
                start = i + 2;
                i++;
            }
            else if (body[i] == '`')
            {
                Flush(i);
                code = !code;
                start = i + 1;
            }
        }
        Flush(body.Length);

        void Flush(int end)
        {
            if (end <= start) return;
            var run = new Run(body[start..end]);
            if (bold) run.FontWeight = FontWeight.Bold;
            if (code)
            {
                run.FontFamily = CodeFont;
                run.Foreground = CodeForeground;
                run.Background = CodeBackground;
            }
            tb.Inlines!.Add(run);
        }
    }
}
