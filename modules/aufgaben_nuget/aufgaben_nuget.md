---
title: "🧩 Aufgaben und Beispiele: NuGet"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei NuGet geht es dabei weniger um Syntax als um Entscheidungen: Welchem Paket vertraue ich, was steht wirklich in meiner Projektdatei, und wo im Programm gehört Logging hin? Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Die Verfolger im Adventure sollen künftig nicht mehr stur auf den Spieler zulaufen, sondern Wände umgehen. Ihr braucht dafür eine Bibliothek für Wegfindung auf einem Gitter (A*). Das fertige Spiel soll später als geschlossenes Produkt an einen Kunden verkauft werden. Auf nuget.org findet ihr zwei Kandidaten (fiktiv):

| | `GridPathKit` | `Grid.PathKit` |
| :--- | :--- | :--- |
| Aktuelle Version | 2.3.0 | 5.0.0-preview.4 |
| Letztes Release | vor 4 Jahren | vor 9 Tagen |
| Downloads gesamt | 1,8 Mio. | 41.000 |
| Downloads letzte 6 Wochen | 12.000 | 9.500 |
| Lizenz | MIT | GPL-3.0 |
| Abhängigkeiten | keine | `Newtonsoft.Json 13.0.3`, `System.Drawing.Common 8.0.0` |
| Quellcode | GitHub, Repository archiviert (schreibgeschützt), 3 offene Issues | GitHub, 140 offene Issues, letzter Commit gestern |
| Autor | `gridpathkit` (Reserved prefix) | `dev_4711` |

Welche Signale sprechen für, welche gegen jedes Paket? Trefft eine Entscheidung und begründet sie – oder begründet, warum ihr keines der beiden nehmt.

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ausschlusskriterien zuerst:**

Bevor man Downloads vergleicht, prüft man, was ein Paket sofort disqualifiziert. `Grid.PathKit` steht unter **GPL-3.0**: Wer es in seine App einbindet, muss die App selbst unter der GPL veröffentlichen – für ein geschlossenes Kundenprodukt ist das ausgeschlossen, egal wie gut der Code ist. Dazu kommt, dass `5.0.0-preview.4` eine Vorabversion ist, deren Schnittstelle sich bis zur stabilen 5.0.0 noch ändern darf.

**Schritt 2 — Die Signale von `GridPathKit` einordnen:**

Das Paket ist seit vier Jahren unverändert und das Repository archiviert – niemand wird dort eine Sicherheitslücke schließen. Aber: Es hat **keine Abhängigkeiten**, also keinen Baum, in dem eine Lücke stecken könnte, die MIT-Lizenz erlaubt jede Nutzung, und 12.000 Downloads in sechs Wochen zeigen, dass es trotz Alter noch produktiv eingesetzt wird. Drei offene Issues bei 1,8 Millionen Downloads bedeuten, dass es im Wesentlichen fertig ist – der A*-Algorithmus ändert sich nicht. Ein reines Rechenpaket ohne Netzwerk- oder Dateizugriff ist ein deutlich geringeres Risiko als eine Bibliothek, die Eingaben von außen verarbeitet.

**Schritt 3 — Das Namensmuster erkennen:**

`Grid.PathKit` sieht dem etablierten `GridPathKit` zum Verwechseln ähnlich, kommt von einem anonymen Autor ohne Reserved prefix und zieht `Newtonsoft.Json` sowie `System.Drawing.Common` mit – zwei Abhängigkeiten, die ein Wegfinder auf einem Gitter nicht braucht. Das muss kein Angriff sein, ist aber genau das Muster, das man bei Typosquatting sieht.

**Schritt 4 — Entscheidung:**

`GridPathKit` in Version 2.3.0, mit einem Vermerk im Projekt, dass das Paket nicht mehr gepflegt wird. Da der Quellcode unter MIT offen liegt, kann das Team ihn im Notfall forken und selbst weiterpflegen – das ist der eigentliche Wert einer freizügigen Lizenz. Alternativ prüft man, ob A* auf einem Gitter in ein paar hundert Zeilen selbst zu schreiben ist – für ein Spielfeld mit ein paar hundert Feldern durchaus realistisch; dann wäre auch das eine legitime Wahl.

**Zentrale Designentscheidungen:**

- **Lizenz vor Qualität:** Ein Paket, das man rechtlich nicht einsetzen darf, muss gar nicht weiter bewertet werden.
- **Aktivität ist nicht alles:** Ein archiviertes Paket ohne Abhängigkeiten kann für ein abgeschlossenes Problem die sicherere Wahl sein als ein hyperaktives mit großem Abhängigkeitsbaum.
- **Ähnliche Namen sind ein Warnsignal**, kein Zufall – Autor und Reserved prefix prüfen.

</details>

## Aufgabe 2 — Zerlegung

Eine Kommilitonin hat `Adventure.Web` erweitert: Spielstände sollen künftig in einer SQLite-Datenbank statt in einer JSON-Datei landen, und geloggt werden soll mit NLog. Sie schickt euch ihre `.csproj`. Lest sie Zeile für Zeile:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0" />
    <PackageReference Include="NLog" Version="6.2.0" />
    <PackageReference Include="CsvHelper" Version="33.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Adventure.Kern\Adventure.Kern.csproj" />
    <ProjectReference Include="..\Adventure.Daten\Adventure.Daten.csproj" />
  </ItemGroup>

</Project>
```

- Welche Pakete werden direkt referenziert, und welche Pakete landen zusätzlich im Projekt, ohne hier zu stehen?
- Was bedeutet `Version="33.*"`, und warum ist das im Team ein Problem?
- Die Kommilitonin sitzt im Zug ohne Internet, löscht `bin/` und `obj/` und ruft `dotnet build` auf. Was passiert – und was wäre passiert, hätte sie `Version="33.0.1"` geschrieben?
- Das Programm läuft, aber die Logdatei bleibt leer. Was fehlt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Direkte und transitive Abhängigkeiten trennen:**

Direkt stehen fünf Pakete in der Datei: drei `Microsoft.EntityFrameworkCore.*`-Pakete, `NLog` und `CsvHelper`. Dazu kommen die beiden Projektreferenzen auf `Adventure.Kern` und `Adventure.Daten` – das sind keine Pakete, sondern Code aus derselben Solution. Blazor selbst taucht nirgends auf: Es steckt im Shared Framework `Microsoft.AspNetCore.App`, das `Sdk="Microsoft.NET.Sdk.Web"` automatisch einbindet. Transitiv kommen über EF Core unter anderem `Microsoft.Data.Sqlite.Core`, `SQLitePCLRaw.bundle_e_sqlite3` mit seinen nativen SQLite-Bibliotheken pro Betriebssystem, `Microsoft.Extensions.Caching.Memory` und `Microsoft.Extensions.Logging.Abstractions` dazu – `dotnet list package --include-transitive` zeigt sie. Auffällig: `Microsoft.EntityFrameworkCore.Sqlite` steht auf `10.0.2`, die anderen auf `10.0.0`. Das baut zwar, weil `Microsoft.EntityFrameworkCore.Sqlite 10.0.2` intern mindestens `10.0.2` der beiden anderen verlangt und NuGet dann die höhere Version nimmt, aber die Zahl in der Datei stimmt dann nicht mehr mit dem überein, was tatsächlich verwendet wird. Besser alle drei auf dieselbe Version.

**Schritt 2 — Die Wildcard-Version:**

`33.*` ist eine **schwebende Version**: Bei jedem Restore fragt NuGet den Feed, welche 33er-Version die neueste ist, und nimmt sie. Zwei Rechner, die an verschiedenen Tagen restoren, bauen also mit unterschiedlichem Code, obwohl die `.csproj` identisch ist – ein Fehler, den eine Person hat und die andere nicht, ist so kaum zu finden. Feste Versionsnummern gehören in die Datei; Updates macht man bewusst mit `dotnet add package CsvHelper --version …`.

**Schritt 3 — Restore ohne Internet:**

Das Löschen von `bin/` und `obj/` ist harmlos – die Pakete liegen im globalen Cache `~/.nuget/packages`, nicht im Projekt. Für die vier fest versionierten Pakete findet NuGet die Versionen dort und fragt keinen Feed. Für `33.*` muss es aber den Feed fragen, um die neueste 33er-Version zu bestimmen. Ohne Verbindung scheitert der Restore mit einem Fehler wie `NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json`, und `dotnet build` bricht ab, ohne eine Zeile zu kompilieren. Mit `Version="33.0.1"` hätte der Build im Zug funktioniert – vorausgesetzt, diese Version lag schon im Cache. Eine noch nie geladene Version scheitert offline in beiden Fällen.

**Schritt 4 — Die leere Logdatei:**

In der `.csproj` fehlt der Eintrag, der die `nlog.config` in den Ausgabeordner kopiert:

```xml
<ItemGroup>
  <None Update="nlog.config" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Ohne ihn liegt die Konfiguration nur im Projektordner, nicht neben der gebauten `.dll`. NLog findet keine Ziele und verwirft alle Meldungen, ohne sich zu beschweren.

**Zentrale Designentscheidungen:**

- **Eine `.csproj` ist die vollständige Beschreibung des Projekts:** Alles, was zum Bauen nötig ist, muss darin stehen – Pakete, Versionen und Dateien, die mit ausgeliefert werden.
- **Feste Versionen sind reproduzierbar**, schwebende nicht. Reproduzierbarkeit ist im Team und in der Build-Pipeline wichtiger als Bequemlichkeit.
- **`bin/` und `obj/` sind Wegwerfordner**, der Paket-Cache ist es nicht – deshalb funktioniert Restore offline, solange nichts Neues gebraucht wird.

</details>

## Aufgabe 3 — Algorithmenentwurf

`dotnet list package --outdated` muss entscheiden, ob `6.10.0` neuer ist als `6.9.0`. Ein Zeichenkettenvergleich liefert die falsche Antwort, denn `"6.10.0" < "6.9.0"`, weil `'1' < '9'`. Entwirf eine Klasse `SemVersion`, die eine Versionsnummer nach den Regeln der semantischen Versionierung zerlegt und `IComparable<SemVersion>` implementiert (siehe [IComparable<T>](/modules/icomparable_sortieren/icomparable_sortieren.md)).

- Aus welchen Teilen besteht eine Version wie `7.0.0-beta.2`, und in welcher Reihenfolge werden sie verglichen?
- Wo ordnet sich eine Vorabversion gegenüber der stabilen Version ein?
- Sage voraus, in welcher Reihenfolge `["6.2.0", "6.10.0", "6.2.0-beta.1", "5.3.4", "6.9.0"]` nach `Sort()` stehen – einmal als `string`, einmal als `SemVersion`.
- Welche Eingaben sollte der Konstruktor ablehnen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Struktur erkennen:**

Eine Version besteht aus drei Zahlen `MAJOR.MINOR.PATCH` und optional einer Vorabversion hinter einem Bindestrich. Verglichen wird von links nach rechts: Erst wenn `Major` gleich ist, zählt `Minor`, dann `Patch`. Sind alle drei gleich, entscheidet die Vorabversion – und zwar so, dass `7.0.0-beta.2` **vor** `7.0.0` liegt: Eine Version mit Anhang ist immer älter als dieselbe Version ohne Anhang.

**Schritt 2 — Zerlegen im Konstruktor:**

```csharp
public class SemVersion : IComparable<SemVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Vorabversion { get; }   // null bei einer stabilen Version

    public SemVersion(string text)
    {
        string kern = text;
        int minus = text.IndexOf('-');
        if (minus >= 0)
        {
            kern = text[..minus];
            Vorabversion = text[(minus + 1)..];
        }

        string[] teile = kern.Split('.');
        if (teile.Length != 3)
        {
            throw new FormatException($"'{text}' ist keine gültige Version.");
        }
        Major = int.Parse(teile[0]);
        Minor = int.Parse(teile[1]);
        Patch = int.Parse(teile[2]);
    }

    public override string ToString() =>
        Vorabversion is null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}-{Vorabversion}";
}
```

`int.Parse` wirft bei `"6.a.0"` selbst eine `FormatException`; die Prüfung auf genau drei Teile fängt `"6.2"` und `"6.2.0.1"` ab. Negative Zahlen wie `"-1.0.0"` sind ein Randfall, den eine ernsthafte Implementierung zusätzlich prüfen müsste.

**Schritt 3 — Der Vergleich:**

```csharp
    public int CompareTo(SemVersion? other)
    {
        if (other is null) return 1;

        int ergebnis = Major.CompareTo(other.Major);
        if (ergebnis != 0) return ergebnis;
        ergebnis = Minor.CompareTo(other.Minor);
        if (ergebnis != 0) return ergebnis;
        ergebnis = Patch.CompareTo(other.Patch);
        if (ergebnis != 0) return ergebnis;

        // Gleiche Zahlen: die Vorabversion liegt vor der stabilen Version
        if (Vorabversion is null && other.Vorabversion is null) return 0;
        if (Vorabversion is null) return 1;
        if (other.Vorabversion is null) return -1;
        return string.CompareOrdinal(Vorabversion, other.Vorabversion);
    }
```

Die drei Zahlen delegieren an `int.CompareTo` – das Muster „erst nach A, bei Gleichstand nach B“ ist dasselbe wie beim Sortieren nach Nachname und dann Vorname.

**Schritt 4 — Vorhersage prüfen:**

```csharp
List<string> texte = ["6.2.0", "6.10.0", "6.2.0-beta.1", "5.3.4", "6.9.0"];

List<string> alsString = [.. texte];
alsString.Sort();
Console.WriteLine(string.Join(" < ", alsString));
// 5.3.4 < 6.10.0 < 6.2.0 < 6.2.0-beta.1 < 6.9.0

List<SemVersion> alsVersion = texte.Select(t => new SemVersion(t)).ToList();
alsVersion.Sort();
Console.WriteLine(string.Join(" < ", alsVersion));
// 5.3.4 < 6.2.0-beta.1 < 6.2.0 < 6.9.0 < 6.10.0
```

Der `string`-Vergleich macht zwei Fehler: `6.10.0` rutscht vor `6.2.0`, und die Beta landet hinter der stabilen Version, weil `"6.2.0"` ein Präfix von `"6.2.0-beta.1"` ist.

**Zentrale Designentscheidungen:**

- **Zerlegen beim Erzeugen, nicht beim Vergleichen:** Der Konstruktor parst einmal und lehnt ungültige Eingaben sofort ab; `CompareTo` arbeitet dann nur noch mit Zahlen.
- **`CompareTo` gibt nur negativ, null oder positiv zurück** – deshalb reicht es, das Ergebnis von `int.CompareTo` durchzureichen.
- **Bewusste Vereinfachung:** Die Vorabversion wird als Zeichenkette verglichen, wodurch `beta.10` vor `beta.9` landet. Die vollständige SemVer-Spezifikation vergleicht die durch Punkte getrennten Teile einzeln und numerisch, wo möglich – eine gute Erweiterung.

</details>

## Aufgabe 4 — Abstraktion

Das Adventure soll Logging bekommen. Es besteht aus den drei Schichten `Adventure.Kern` (Spielregeln), `Adventure.Daten` (Level laden, Spielstände speichern) und `Adventure.Web` (Blazor-Oberfläche, das ausführbare Projekt) – siehe [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md).

- Welche Schicht loggt welche Ereignisse, und mit welchem Level? Nenne je Schicht zwei konkrete Beispiele aus dem vorhandenen Code (`Spielfeld`, `JsonSpielstandSpeicher`, `Home.razor`).
- In welche Projekte kommt die `PackageReference` auf NLog, in welches die `nlog.config`?
- `Adventure.Kern` soll nicht wissen, welche Logging-Bibliothek die Anwendung verwendet. Wie erreicht man das, ohne auf Logging zu verzichten?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ereignisse den Schichten zuordnen:**

Jede Schicht loggt, was sie selbst weiß, und nichts, was eine andere besser weiß:

- **`Adventure.Kern`** kennt die Spielregeln: `Info`, wenn eine Runde etwas Bemerkenswertes bringt („Runde 14: Schlüssel eingesammelt“, „Tür aufgeschlossen“), `Warn`, wenn `SpielerZieht` einen Zug bekommt, obwohl `Status` nicht mehr `Laeuft` ist – ein Aufruf, der auf einen Fehler in der Oberfläche hindeutet. Der Kern kennt weder Dateien noch Browser und loggt deshalb auch keine Pfade und keine Tastendrücke.
- **`Adventure.Daten`** kennt Pfade und Formate: `Debug` mit Dateiname und Feldgröße, wenn `TextdateiLevelQuelle` ein Level eingelesen hat, `Error` mitsamt Exception, wenn `JsonSpielstandSpeicher.Speichern` an `File.WriteAllText` scheitert oder das JSON beim Laden ungültig ist. Bei `HttpLevelQuelle` kommt `Warn` bei einem fehlgeschlagenen Abruf dazu.
- **`Adventure.Web`** kennt Benutzeraktionen: `Info` beim Start und beim Wechsel des Levels über die Auswahlliste in `Home.razor`, `Debug` für jeden Tastendruck in `TasteGedrueckt`, `Error` mit Exception, wenn ein Fehler dem Spieler als Dialog angezeigt wird – das ist die Stelle, die die Exception wirklich behandelt, also loggt sie hier genau einmal.

**Schritt 2 — Paket und Konfiguration verteilen:**

Die `nlog.config` gehört ausschließlich in `Adventure.Web`, denn nur das ausführbare Projekt hat einen Ausgabeordner, aus dem NLog sie beim Start liest – und nur dort wird entschieden, wohin die Meldungen gehen. Die `PackageReference` auf NLog braucht jedes Projekt, das `LogManager` aufruft – bei einem naiven Ansatz also alle drei. Damit die Datenschicht ausführlicher loggt als der Rest, nutzt man die Logger-Namen, die ja den Namensräumen entsprechen:

```xml
<rules>
  <logger name="Adventure.Daten.*" minlevel="Debug" writeTo="datei" />
  <logger name="*" minlevel="Info" writeTo="datei" />
</rules>
```

**Schritt 3 — Die Abhängigkeit im Kern vermeiden:**

`Adventure.Kern` referenziert bisher kein einziges Paket und kein einziges anderes Projekt – genau das macht es leicht testbar und wiederverwendbar. Eine `PackageReference` auf NLog würde die Spielregeln an eine konkrete Bibliothek binden. Die Lösung ist dieselbe wie bei `ILevelQuelle` und `ISpielstandSpeicher`: eine Schnittstelle statt einer Implementierung. Das Paket `Microsoft.Extensions.Logging.Abstractions` enthält nur das Interface `ILogger<T>` und keine Zeile Logging-Code, und der Kern bekommt den Logger über den Konstruktor gereicht:

```csharp
using Microsoft.Extensions.Logging;

namespace Adventure.Kern;

public class Spielfeld
{
    private readonly ILogger<Spielfeld> logger;

    public Spielfeld(int breite, int hoehe, Spieler spieler, ILogger<Spielfeld> logger)
    {
        Breite = breite;
        Hoehe = hoehe;
        Spieler = spieler;
        this.logger = logger;
    }

    public void SpielerZieht(Richtung richtung)
    {
        if (Status != Spielstatus.Laeuft)
        {
            logger.LogWarning("Zug nach {Richtung} ignoriert, das Spiel ist {Status}.", richtung, Status);
            return;
        }

        Runde++;
        // ... erst zieht der Spieler, dann alle Gegner ...
        logger.LogInformation("Runde {Runde}: {Meldung}", Runde, LetzteMeldung);
    }
}
```

`Adventure.Web` entscheidet, was hinter `ILogger<T>` steckt: Mit dem Brückenpaket `NLog.Extensions.Logging` registriert es NLog als Anbieter, und die Abhängigkeitsinjektion, die dort ohnehin schon `ILevelQuelle` liefert, reicht den passenden Logger in den Konstruktor. In `Adventure.Tests` genügt `NullLogger<Spielfeld>.Instance`, der alles verwirft – kein Log, keine Datei, kein Zeitstempel im Testlauf.

**Zentrale Designentscheidungen:**

- **Jede Schicht loggt ihr eigenes Wissen:** Pfade in `Adventure.Daten`, Spielregeln in `Adventure.Kern`, Benutzeraktionen in `Adventure.Web` – so steht jede Information genau einmal im Log.
- **Konfiguration liegt beim ausführbaren Projekt**, weil nur dort entschieden wird, wohin Meldungen gehen. Eine `nlog.config` in `Adventure.Kern` würde nie gelesen.
- **Der Kern hängt nur von Abstraktionen ab** – dasselbe Prinzip wie bei `ILevelQuelle`, jetzt für das Logging. `Microsoft.Extensions.Logging.Abstractions` ist zwar auch ein Paket, aber eines, das nur Schnittstellen enthält. Welche Bibliothek am Ende schreibt, ist eine Entscheidung der Anwendung, nicht der Spielregeln.

</details>
