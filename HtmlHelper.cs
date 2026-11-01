using System.Text.Json;

namespace HtmlTools;

/// <summary>
/// מחלקת עזר שמספקת את רשימות תגיות ה-HTML.
/// ממומשת כ-Singleton כדי שקבצי ה-JSON ייטענו פעם אחת בלבד.
/// </summary>
public class HtmlHelper
{
    private static readonly HtmlHelper _instance = new HtmlHelper();
    public static HtmlHelper Instance => _instance;

    /// <summary>כל תגיות ה-HTML</summary>
    public string[] HtmlTags { get; }

    /// <summary>תגיות שלא דורשות תגית סגירה</summary>
    public string[] HtmlVoidTags { get; }

    // Constructor פרטי - אי אפשר ליצור מופע מבחוץ
    private HtmlHelper()
    {
        string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

        string tagsJson = File.ReadAllText(Path.Combine(dataDir, "HtmlTags.json"));
        string voidTagsJson = File.ReadAllText(Path.Combine(dataDir, "HtmlVoidTags.json"));

        HtmlTags = JsonSerializer.Deserialize<string[]>(tagsJson) ?? Array.Empty<string>();
        HtmlVoidTags = JsonSerializer.Deserialize<string[]>(voidTagsJson) ?? Array.Empty<string>();

        _tagsSet = new HashSet<string>(HtmlTags, StringComparer.OrdinalIgnoreCase);
        _voidTagsSet = new HashSet<string>(HtmlVoidTags, StringComparer.OrdinalIgnoreCase);
    }

    // HashSet לחיפוש מהיר (O(1)) ולא תלוי רישיות
    private readonly HashSet<string> _tagsSet;
    private readonly HashSet<string> _voidTagsSet;

    public bool IsHtmlTag(string name) => _tagsSet.Contains(name);

    public bool IsVoidTag(string name) => _voidTagsSet.Contains(name);
}
