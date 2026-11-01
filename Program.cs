using HtmlTools;

var serializer = new HtmlSerializer();

// ---------- בדיקה על HTML מקומי ----------
string sampleHtml = """
<!DOCTYPE html>
<html>
  <head><title>Test</title></head>
  <body>
    <div id="main" class="container wide">
      <div>
        <p class="class-name">First paragraph</p>
        <img src="a.png" alt="pic">
        <br/>
      </div>
      <p class="class-name other">Second paragraph</p>
    </div>
    <div id="mydiv"><span class="class-name">inside mydiv</span></div>
  </body>
</html>
""";

HtmlElement dom = serializer.Parse(sampleHtml);

Console.WriteLine("=== Tree ===");
PrintTree(dom, 0);

Console.WriteLine();
RunQuery(dom, "div p.class-name");      // 2 תוצאות, בלי כפילויות
RunQuery(dom, "div#main.container");     // 1
RunQuery(dom, "#mydiv .class-name");     // 1 (span)
RunQuery(dom, "body div div p");         // 1

// Ancestors
var span = dom.Query("span").First();
Console.WriteLine($"\nAncestors of {span}: {string.Join(" -> ", span.Ancestors())}");

// ---------- בדיקה על אתר אמיתי ----------
Console.Write("\nEnter a URL to test (or Enter to skip): ");
string? url = Console.ReadLine();
if (!string.IsNullOrWhiteSpace(url))
{
    try
    {
        HtmlElement site = await serializer.LoadAndParse(url);
        Console.WriteLine($"Total elements: {site.Descendants().Count() - 1}");

        while (true)
        {
            Console.Write("Selector (Enter to exit): ");
            string? q = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(q)) break;
            RunQuery(site, q);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

// ---------- פונקציות עזר ----------
static void RunQuery(HtmlElement root, string query)
{
    var results = root.Query(query);
    Console.WriteLine($"Query \"{query}\" => {results.Count} result(s)");
    foreach (var el in results)
        Console.WriteLine($"   {el}  {el.InnerHtml}");
}

static void PrintTree(HtmlElement element, int depth)
{
    // הדפסה בעזרת מחסנית (בלי ריקורסיה)
    var stack = new Stack<(HtmlElement el, int depth)>();
    stack.Push((element, depth));
    while (stack.Count > 0)
    {
        var (el, d) = stack.Pop();
        string text = el.InnerHtml.Length > 0 ? $"  \"{el.InnerHtml}\"" : "";
        Console.WriteLine($"{new string(' ', d * 2)}{el}{text}");
        for (int i = el.Children.Count - 1; i >= 0; i--)
            stack.Push((el.Children[i], d + 1));
    }
}
