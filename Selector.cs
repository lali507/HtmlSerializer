using System.Text.RegularExpressions;

namespace HtmlTools;

/// <summary>
/// מייצג רמה אחת ב-CSS Selector. הרמות מקושרות ב-Parent/Child (עץ בינארי מנוון).
/// </summary>
public class Selector
{
    public string? TagName { get; set; }
    public string? Id { get; set; }
    public List<string> Classes { get; set; } = new();

    public Selector? Parent { get; set; }
    public Selector? Child { get; set; }

    /// <summary>
    /// ממיר מחרוזת שאילתה (למשל "div #mydiv .class-name") לשרשרת סלקטורים.
    /// מחזיר את הסלקטור העליון (השורש).
    /// </summary>
    public static Selector Parse(string query)
    {
        // כל חלק מופרד ברווח = רמה נוספת בעומק
        string[] levels = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (levels.Length == 0)
            throw new ArgumentException("Selector query is empty", nameof(query));

        Selector? root = null;
        Selector? current = null;

        foreach (string level in levels)
        {
            var selector = new Selector();

            // פיצול לפני כל # או . (lookahead שומר על התו המפריד)
            // "div#mydiv.class-name" => "div", "#mydiv", ".class-name"
            string[] parts = Regex.Split(level, @"(?=[#.])")
                                  .Where(p => p.Length > 0)
                                  .ToArray();

            foreach (string part in parts)
            {
                if (part.StartsWith("#"))
                    selector.Id = part.Substring(1);
                else if (part.StartsWith("."))
                    selector.Classes.Add(part.Substring(1));
                else if (HtmlHelper.Instance.IsHtmlTag(part))
                    selector.TagName = part.ToLower();
            }

            if (root == null)
            {
                root = selector;
            }
            else
            {
                selector.Parent = current;
                current!.Child = selector;
            }
            current = selector;
        }

        return root!;
    }

    /// <summary>האם האלמנט עונה על כל התנאים של הרמה הזו</summary>
    public bool Matches(HtmlElement element)
    {
        if (TagName != null && element.Name != TagName)
            return false;

        if (Id != null && element.Id != Id)
            return false;

        return Classes.All(c => element.Classes.Contains(c));
    }

    public override string ToString()
    {
        string self = (TagName ?? "")
                    + (Id != null ? "#" + Id : "")
                    + string.Concat(Classes.Select(c => "." + c));
        return Child == null ? self : self + " " + Child;
    }
}
