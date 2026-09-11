namespace Geometrieeditor.Fachkonzept;

/// <summary>
/// Geschäftslogik des Geometrieeditors: verwaltet die Figuren
/// und nutzt einen IFigurSpeicher zum Laden und Speichern.
/// </summary>
public class FigurenVerwaltung
{
    private readonly List<Figur> figuren = new();
    private readonly IFigurSpeicher speicher;

    public FigurenVerwaltung(IFigurSpeicher speicher)
    {
        this.speicher = speicher;
    }

    public IReadOnlyList<Figur> AlleFiguren => figuren;

    public void Hinzufuegen(Figur figur)
    {
        if (figuren.Any(f => f.Name == figur.Name))
        {
            throw new ArgumentException($"Es gibt bereits eine Figur mit dem Namen '{figur.Name}'.");
        }
        figuren.Add(figur);
    }

    public bool Entfernen(Figur figur)
    {
        return figuren.Remove(figur);
    }

    public Figur? Suchen(string name)
    {
        return figuren.FirstOrDefault(f => f.Name == name);
    }

    public double GesamtFlaeche()
    {
        double summe = 0;
        foreach (Figur f in figuren)
        {
            summe += f.Flaeche;
        }
        return summe;
    }

    public void Speichern()
    {
        speicher.Speichern(figuren);
    }

    public void Laden()
    {
        figuren.Clear();
        figuren.AddRange(speicher.Laden());
    }
}
