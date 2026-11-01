namespace HtmlTools;

/// <summary>
/// מייצגת תגית HTML אחת בעץ.
/// </summary>
public class HtmlElement
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Classes { get; set; } = new();
    public string InnerHtml { get; set; } = "";

    public HtmlElement? Parent { get; set; }
    public List<HtmlElement> Children { get; set; } = new();

    /// <summary>
    /// כל הצאצאים מכל הדורות (כולל האלמנט עצמו, כראשון ברשימה).
    /// ממומש עם תור ולא בריקורסיה, כדי לא לחרוג מגבולות ה-Stack.
    /// </summary>
    public IEnumerable<HtmlElement> Descendants()
    {
        var queue = new Queue<HtmlElement>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            HtmlElement current = queue.Dequeue();
            yield return current;

            foreach (var child in current.Children)
                queue.Enqueue(child);
        }
    }

    /// <summary>
    /// כל האבות מכל הדורות - מהאבא הישיר ועד השורש.
    /// </summary>
    public IEnumerable<HtmlElement> Ancestors()
    {
        HtmlElement? current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public override string ToString()
    {
        string id = string.IsNullOrEmpty(Id) ? "" : $"#{Id}";
        string classes = Classes.Count == 0 ? "" : "." + string.Join(".", Classes);
        return $"<{Name}{id}{classes}>";
    }
}
