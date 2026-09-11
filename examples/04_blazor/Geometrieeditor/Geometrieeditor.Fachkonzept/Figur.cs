using System.Text.Json.Serialization;

namespace Geometrieeditor.Fachkonzept;

/// <summary>
/// Gemeinsame Basisklasse aller geometrischen Figuren.
/// Position und Name sind für alle Figuren gleich, Fläche und Umfang
/// muss jede konkrete Figur selbst berechnen.
/// </summary>
// Damit System.Text.Json abstrakte Figuren speichern und wieder laden kann,
// bekommt jede abgeleitete Klasse einen Namen im JSON ("typ": "kreis").
[JsonPolymorphic(TypeDiscriminatorPropertyName = "typ")]
[JsonDerivedType(typeof(Rechteck), "rechteck")]
[JsonDerivedType(typeof(Kreis), "kreis")]
[JsonDerivedType(typeof(Dreieck), "dreieck")]
public abstract class Figur
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    protected Figur(string name, double x, double y)
    {
        Name = name;
        X = x;
        Y = y;
    }

    public abstract double Flaeche { get; }
    public abstract double Umfang { get; }

    public virtual void Verschieben(double dx, double dy)
    {
        X += dx;
        Y += dy;
    }

    public virtual string Beschreibung()
    {
        return $"{Name} bei ({X}, {Y}) mit Fläche {Flaeche:F2}";
    }

    public override string ToString() => Beschreibung();
}
