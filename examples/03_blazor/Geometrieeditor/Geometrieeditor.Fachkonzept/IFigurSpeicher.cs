namespace Geometrieeditor.Fachkonzept;

/// <summary>
/// Schnittstelle zur Datenhaltungsschicht. Das Fachkonzept legt fest,
/// was ein Speicher können muss – wie er das tut, ist ihm egal.
/// </summary>
public interface IFigurSpeicher
{
    void Speichern(IEnumerable<Figur> figuren);
    List<Figur> Laden();
}
