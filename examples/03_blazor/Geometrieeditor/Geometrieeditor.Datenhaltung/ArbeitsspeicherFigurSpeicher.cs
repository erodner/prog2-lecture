using Geometrieeditor.Fachkonzept;

namespace Geometrieeditor.Datenhaltung;

/// <summary>
/// Einfachste Datenhaltung: merkt sich die Figuren nur im Arbeitsspeicher.
/// Praktisch zum Testen und für den Anfang – nach dem Programmende ist alles weg.
/// </summary>
public class ArbeitsspeicherFigurSpeicher : IFigurSpeicher
{
    private List<Figur> gespeichert = new();

    public void Speichern(IEnumerable<Figur> figuren)
    {
        gespeichert = new List<Figur>(figuren);
    }

    public List<Figur> Laden()
    {
        return new List<Figur>(gespeichert);
    }
}
