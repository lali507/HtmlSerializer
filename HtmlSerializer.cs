using System.Text.RegularExpressions;

namespace HtmlTools;

/// <summary>
/// קורא דף אינטרנט וממיר את ה-HTML שלו לעץ של אוביקטי HtmlElement.
/// </summary>
public class HtmlSerializer
{
    // HttpClient אחד לכל חיי התוכנית (מומלץ ע"י מיקרוסופט)
    private static readonly HttpClient _client = CreateClient();

    // מזהה attribute: שם, ואופציונלית ערך בגרשיים כפולים / בודדים / בלי גרשיים
    private static readonly Regex AttributeRegex = new(
        @"([\w\-:@]+)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'>]+)))?",
        RegexOptions.Compiled);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        // חלק מהאתרים חוסמים בקשות בלי User-Agent
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (HtmlSerializer)");
        return client;
    }

    // ---------- 1. קריאה לדף אינטרנט ----------
    public async Task<string> Load(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return html;
    }

    // ---------- 2. פירוק לפי תגיות ----------
    public List<string> SplitToParts(string html)
    {
        // הסרת הערות <!-- ... -->
        html = Regex.Replace(html, @"<!--[\s\S]*?-->", "");

        // ריקון התוכן של script ו-style (עלול להכיל < ו-> ולשבור את הפירוק)
        html = Regex.Replace(html, @"(<(script|style)\b[^>]*>)[\s\S]*?(</\2\s*>)", "$1$3",
            RegexOptions.IgnoreCase);

        // פיצול לפי תגיות - הסוגריים ב-Regex שומרים את התגיות עצמן ברשימה
        string[] parts = Regex.Split(html, @"(<[^>]*>)");

        return parts
            .Select(p => Regex.Replace(p, @"\s+", " ").Trim()) // ירידות שורה ורווחים מיותרים
            .Where(p => p.Length > 0)                          // מחרוזות ריקות
            .ToList();
    }

    // ---------- 3. בניית העץ (בלולאה, בלי ריקורסיה) ----------
    public HtmlElement Parse(string html)
    {
        var helper = HtmlHelper.Instance;
        List<string> parts = SplitToParts(html);

        // שורש וירטואלי שמחזיק את כל המסמך
        var root = new HtmlElement { Name = "#document" };
        HtmlElement current = root;

        foreach (string part in parts)
        {
            // טקסט פנימי - כל מה שאינו תגית
            if (!part.StartsWith("<"))
            {
                AppendInnerHtml(current, part);
                continue;
            }

            // תוכן התגית בלי < ו->
            string tag = part.Substring(1, part.Length - 2).Trim();
            if (tag.Length == 0 || tag.StartsWith("!") || tag.StartsWith("?"))
                continue; // <!DOCTYPE ...> וכדומה

            string firstWord = tag.Split(' ', 2)[0];

            // סוף ה-HTML
            if (firstWord.Equals("/html", StringComparison.OrdinalIgnoreCase))
                break;

            // תגית סוגרת - עולים לרמה הקודמת
            if (firstWord.StartsWith("/"))
            {
                string closingName = firstWord.Substring(1).ToLower();
                // מחפשים את האלמנט התואם במעלה העץ (מגן מפני HTML לא תקין)
                HtmlElement? match = current;
                while (match != null && match != root && match.Name != closingName)
                    match = match.Parent;

                if (match != null && match != root)
                    current = match.Parent ?? root;
                continue;
            }

            string tagName = firstWord.TrimEnd('/').ToLower();

            // תגית HTML מוכרת - יוצרים אלמנט חדש
            if (helper.IsHtmlTag(tagName))
            {
                var element = new HtmlElement { Name = tagName, Parent = current };
                current.Children.Add(element);

                string rest = tag.Length > firstWord.Length ? tag.Substring(firstWord.Length) : "";
                ParseAttributes(element, rest);

                bool selfClosing = tag.EndsWith("/") || helper.IsVoidTag(tagName);
                if (!selfClosing)
                    current = element; // ייתכן תוכן פנימי - יורדים לתוך האלמנט
            }
            // תגית לא מוכרת (למשל רכיב מותאם אישית) - מדלגים עליה
        }

        return root;
    }

    // טעינה + פירסור בפעולה אחת
    public async Task<HtmlElement> LoadAndParse(string url) => Parse(await Load(url));

    private static void ParseAttributes(HtmlElement element, string attributesText)
    {
        attributesText = attributesText.Trim().TrimEnd('/');

        foreach (Match m in AttributeRegex.Matches(attributesText))
        {
            string name = m.Groups[1].Value.ToLower();
            string value = m.Groups[2].Success ? m.Groups[2].Value
                         : m.Groups[3].Success ? m.Groups[3].Value
                         : m.Groups[4].Success ? m.Groups[4].Value
                         : "";

            element.Attributes[name] = value;

            if (name == "class")
                element.Classes = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            else if (name == "id")
                element.Id = value;
        }
    }

    private static void AppendInnerHtml(HtmlElement element, string text)
    {
        element.InnerHtml = element.InnerHtml.Length == 0 ? text : element.InnerHtml + " " + text;
    }
}
