using Geometrieeditor.Datenhaltung;
using Geometrieeditor.Fachkonzept;

namespace Geometrieeditor.Tests;

[TestFixture]
public class JsonFigurSpeicherTests
{
    private string pfad = "";

    [SetUp]
    public void Vorbereiten()
    {
        pfad = Path.Combine(Path.GetTempPath(), $"figuren_{Guid.NewGuid()}.json");
    }

    [TearDown]
    public void Aufraeumen()
    {
        if (File.Exists(pfad)) File.Delete(pfad);
    }

    [Test]
    public void Laden_OhneDatei_LiefertLeereListe()
    {
        JsonFigurSpeicher speicher = new JsonFigurSpeicher(pfad);

        Assert.That(speicher.Laden(), Is.Empty);
    }

    [Test]
    public void SpeichernUndLaden_BehaeltKonkreteTypen()
    {
        JsonFigurSpeicher speicher = new JsonFigurSpeicher(pfad);
        List<Figur> figuren = new()
        {
            new Rechteck("r", 0, 0, 2, 3),
            new Kreis("k", 5, 5, 1),
            new Dreieck("d", 1, 1, 3, 4, 5)
        };

        speicher.Speichern(figuren);
        List<Figur> geladen = speicher.Laden();

        Assert.That(geladen, Has.Count.EqualTo(3));
        Assert.That(geladen[0], Is.TypeOf<Rechteck>());
        Assert.That(geladen[1], Is.TypeOf<Kreis>());
        Assert.That(geladen[2], Is.TypeOf<Dreieck>());
        Assert.That(geladen[1].Flaeche, Is.EqualTo(Math.PI).Within(1e-9));
    }
}
