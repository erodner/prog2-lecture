namespace Geometrieeditor.Fachkonzept;

public class Kreis : Figur
{
    public double Radius { get; set; }

    public Kreis(string name, double x, double y, double radius)
        : base(name, x, y)
    {
        Radius = radius;
    }

    public override double Flaeche => Math.PI * Radius * Radius;
    public override double Umfang => 2 * Math.PI * Radius;

    public override string Beschreibung()
    {
        return base.Beschreibung() + $" (r = {Radius})";
    }
}
