namespace Geometrieeditor.Fachkonzept;

/// <summary>Dreieck, beschrieben durch seine drei Seitenlängen.</summary>
public class Dreieck : Figur
{
    public double SeiteA { get; set; }
    public double SeiteB { get; set; }
    public double SeiteC { get; set; }

    // Die Parameternamen entsprechen den Property-Namen – das braucht System.Text.Json beim Laden.
    public Dreieck(string name, double x, double y, double seiteA, double seiteB, double seiteC)
        : base(name, x, y)
    {
        if (seiteA + seiteB <= seiteC || seiteA + seiteC <= seiteB || seiteB + seiteC <= seiteA)
        {
            throw new ArgumentException("Die Seitenlängen ergeben kein Dreieck.");
        }
        SeiteA = seiteA;
        SeiteB = seiteB;
        SeiteC = seiteC;
    }

    public override double Umfang => SeiteA + SeiteB + SeiteC;

    // Satz des Heron
    public override double Flaeche
    {
        get
        {
            double s = Umfang / 2;
            return Math.Sqrt(s * (s - SeiteA) * (s - SeiteB) * (s - SeiteC));
        }
    }
}
