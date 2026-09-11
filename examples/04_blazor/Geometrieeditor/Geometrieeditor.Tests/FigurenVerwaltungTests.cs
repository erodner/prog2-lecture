using Geometrieeditor.Datenhaltung;
using Geometrieeditor.Fachkonzept;

namespace Geometrieeditor.Tests;

[TestFixture]
public class FigurenVerwaltungTests
{
    private FigurenVerwaltung verwaltung = null!;

    [SetUp]
    public void Vorbereiten()
    {
        // Für Tests reicht der Speicher im Arbeitsspeicher – keine Datei, keine GUI.
        verwaltung = new FigurenVerwaltung(new ArbeitsspeicherFigurSpeicher());
    }

    [Test]
    public void Hinzufuegen_NeueFigur_ErscheintInAlleFiguren()
    {
        Kreis kreis = new Kreis("k1", 0, 0, 1);

        verwaltung.Hinzufuegen(kreis);

        Assert.That(verwaltung.AlleFiguren, Has.Count.EqualTo(1));
        Assert.That(verwaltung.AlleFiguren[0], Is.SameAs(kreis));
    }

    [Test]
    public void Hinzufuegen_DoppelterName_WirftArgumentException()
    {
        verwaltung.Hinzufuegen(new Kreis("k1", 0, 0, 1));

        Assert.That(() => verwaltung.Hinzufuegen(new Rechteck("k1", 0, 0, 2, 3)),
                    Throws.ArgumentException);
    }

    [Test]
    public void GesamtFlaeche_RechteckUndKreis_SummiertFlaechen()
    {
        verwaltung.Hinzufuegen(new Rechteck("r", 0, 0, 2, 3));   // 6
        verwaltung.Hinzufuegen(new Kreis("k", 0, 0, 1));         // pi

        Assert.That(verwaltung.GesamtFlaeche(), Is.EqualTo(6 + Math.PI).Within(1e-9));
    }

    [Test]
    public void SpeichernUndLaden_StelltFigurenWiederHer()
    {
        verwaltung.Hinzufuegen(new Dreieck("d", 1, 1, 3, 4, 5));
        verwaltung.Speichern();
        verwaltung.Entfernen(verwaltung.Suchen("d")!);

        verwaltung.Laden();

        Assert.That(verwaltung.Suchen("d"), Is.Not.Null);
        Assert.That(verwaltung.Suchen("d")!.Flaeche, Is.EqualTo(6).Within(1e-9));
    }
}
