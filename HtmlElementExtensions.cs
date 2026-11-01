namespace HtmlTools;

/// <summary>
/// פונקציות הרחבה לחיפוש אלמנטים בעץ לפי Selector.
/// </summary>
public static class HtmlElementExtensions
{
    /// <summary>
    /// מחזיר את כל האלמנטים שעונים לסלקטור.
    /// HashSet מונע כפילויות (אלמנט שעונה לתנאי דרך כמה אבות).
    /// </summary>
    public static HashSet<HtmlElement> FindElements(this HtmlElement element, Selector selector)
    {
        var current = new HashSet<HtmlElement> { element };

        // מעבר רמה אחר רמה בסלקטור. HashSet בכל רמה מונע חיפוש חוזר
        // מאותו אלמנט כשהוא נמצא דרך כמה אבות (מונע סיבוכיות מעריכית).
        for (Selector? level = selector; level != null; level = level.Child)
        {
            var next = new HashSet<HtmlElement>();
            foreach (var el in current)
                FindLevel(el, level, next);
            current = next;
        }

        return current;
    }

    /// <summary>גרסה נוחה שמקבלת מחרוזת שאילתה ישירות</summary>
    public static HashSet<HtmlElement> Query(this HtmlElement element, string query)
        => element.FindElements(Selector.Parse(query));

    // כל הצאצאים של האלמנט שעונים לרמה הנוכחית (Skip(1) - בלי האלמנט עצמו, כמו ב-CSS)
    private static void FindLevel(HtmlElement element, Selector selector, HashSet<HtmlElement> result)
    {
        foreach (var match in element.Descendants().Skip(1).Where(selector.Matches))
            result.Add(match);
    }
}
