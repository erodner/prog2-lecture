using System.Text.Json;
using Geometrieeditor.Fachkonzept;

namespace Geometrieeditor.Datenhaltung;

/// <summary>
/// Datenhaltung in einer JSON-Datei. Welche konkreten Figurtypen es gibt,
/// weiß der Serialisierer dank der [JsonDerivedType]-Attribute an Figur.
/// </summary>
public class JsonFigurSpeicher : IFigurSpeicher
{
    private readonly string pfad;

    private static readonly JsonSerializerOptions optionen = new()
    {
        WriteIndented = true
    };

    public JsonFigurSpeicher(string pfad)
    {
        this.pfad = pfad;
    }

    public void Speichern(IEnumerable<Figur> figuren)
    {
        string json = JsonSerializer.Serialize(figuren, optionen);
        File.WriteAllText(pfad, json);
    }

    public List<Figur> Laden()
    {
        if (!File.Exists(pfad))
        {
            return new List<Figur>();
        }
        string json = File.ReadAllText(pfad);
        return JsonSerializer.Deserialize<List<Figur>>(json, optionen) ?? new List<Figur>();
    }
}
