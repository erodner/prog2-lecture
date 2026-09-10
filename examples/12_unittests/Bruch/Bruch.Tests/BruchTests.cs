namespace Bruch.Tests;

[TestFixture]
public class BruchTests
{
    [Test]
    public void Konstruktor_KuerztDenBruch()
    {
        // Arrange + Act
        Bruch b = new Bruch(6, 8);

        // Assert
        Assert.That(b.Zaehler, Is.EqualTo(3));
        Assert.That(b.Nenner, Is.EqualTo(4));
    }

    [Test]
    public void Konstruktor_NennerNull_WirftArgumentException()
    {
        Assert.That(() => new Bruch(1, 0), Throws.ArgumentException);
    }

    [Test]
    public void Plus_EinHalbPlusEinDrittel_ErgibtFuenfSechstel()
    {
        // Arrange
        Bruch a = new Bruch(1, 2);
        Bruch b = new Bruch(1, 3);

        // Act
        Bruch summe = a.Plus(b);

        // Assert
        Assert.That(summe, Is.EqualTo(new Bruch(5, 6)));
    }

    [TestCase(1, 2, 0.5)]
    [TestCase(3, 4, 0.75)]
    [TestCase(-1, 4, -0.25)]
    public void AlsDezimalzahl_LiefertErwartetenWert(int zaehler, int nenner, double erwartet)
    {
        Bruch b = new Bruch(zaehler, nenner);

        Assert.That(b.AlsDezimalzahl(), Is.EqualTo(erwartet).Within(1e-9));
    }

    [Test]
    public void ToString_GanzeZahl_OhneNenner()
    {
        Assert.That(new Bruch(4, 2).ToString(), Is.EqualTo("2"));
    }
}
