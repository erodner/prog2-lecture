namespace Bruch;

/// <summary>Ein gekürzter Bruch aus Zähler und Nenner.</summary>
public class Bruch : IEquatable<Bruch>
{
    public int Zaehler { get; }
    public int Nenner { get; }

    public Bruch(int zaehler, int nenner)
    {
        if (nenner == 0)
        {
            throw new ArgumentException("Der Nenner darf nicht 0 sein.", nameof(nenner));
        }
        if (nenner < 0)
        {
            zaehler = -zaehler;
            nenner = -nenner;
        }
        int ggt = Ggt(Math.Abs(zaehler), nenner);
        Zaehler = zaehler / ggt;
        Nenner = nenner / ggt;
    }

    public Bruch Plus(Bruch anderer)
    {
        return new Bruch(Zaehler * anderer.Nenner + anderer.Zaehler * Nenner, Nenner * anderer.Nenner);
    }

    public Bruch Mal(Bruch anderer)
    {
        return new Bruch(Zaehler * anderer.Zaehler, Nenner * anderer.Nenner);
    }

    public double AlsDezimalzahl() => (double)Zaehler / Nenner;

    public bool Equals(Bruch? anderer)
    {
        return anderer is not null && Zaehler == anderer.Zaehler && Nenner == anderer.Nenner;
    }

    public override bool Equals(object? obj) => Equals(obj as Bruch);
    public override int GetHashCode() => HashCode.Combine(Zaehler, Nenner);
    public override string ToString() => Nenner == 1 ? $"{Zaehler}" : $"{Zaehler}/{Nenner}";

    private static int Ggt(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a == 0 ? 1 : a;
    }
}
