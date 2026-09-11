namespace Geometrieeditor.Fachkonzept;

public class Rechteck : Figur
{
    public double Breite { get; set; }
    public double Hoehe { get; set; }

    public Rechteck(string name, double x, double y, double breite, double hoehe)
        : base(name, x, y)
    {
        Breite = breite;
        Hoehe = hoehe;
    }

    public override double Flaeche => Breite * Hoehe;
    public override double Umfang => 2 * (Breite + Hoehe);

    public override string Beschreibung()
    {
        return base.Beschreibung() + $" ({Breite} x {Hoehe})";
    }
}
