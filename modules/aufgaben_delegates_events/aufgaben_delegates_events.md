---
title: "🧩 Aufgaben und Beispiele: Delegaten, Lambdas und Ereignisse"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Delegaten und Ereignisse sind dafür besonders geeignet, weil sie erlauben, das *Was* vom *Wer* zu trennen – und weil man bei Closures und Multicast-Delegaten sehr genau hinschauen muss, was zur Laufzeit wirklich passiert. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Algorithmenentwurf

Sage die vollständige Ausgabe des folgenden Programms voraus, ohne es auszuführen. Das Programm kombiniert einen Multicast-Delegaten mit `+=` und `-=` und zwei Schleifen, die Lambdas in einer Liste sammeln.

```csharp
delegate void MeldungHandler(string text);

static class Ausgabe
{
    public static void Kurz(string text) => Console.WriteLine($"[kurz] {text}");
    public static void Lang(string text) => Console.WriteLine($"[lang] {text.ToUpper()}!");
}

MeldungHandler? melder = Ausgabe.Kurz;
melder += Ausgabe.Lang;
melder += Ausgabe.Kurz;
melder("a");

melder -= Ausgabe.Kurz;
melder("b");

List<Action> aktionen = [];
for (int i = 1; i <= 3; i++)
    aktionen.Add(() => Console.WriteLine($"i = {i}"));

int summe = 0;
foreach (int wert in new[] { 10, 20 })
    aktionen.Add(() => { summe += wert; Console.WriteLine($"summe = {summe}"); });

foreach (Action aktion in aktionen)
    aktion();

melder -= Ausgabe.Lang;
melder -= Ausgabe.Kurz;
melder?.Invoke("c");
Console.WriteLine(melder is null);
```

Leitfragen:
- In welcher Reihenfolge werden die Methoden eines Multicast-Delegaten aufgerufen?
- Was entfernt `-=`, wenn dieselbe Methode zweimal in der Liste steht?
- Welchen Wert hat `i`, wenn die Lambdas *ausgeführt* werden? Und `wert`?
- Was ist der Unterschied zwischen `for` und `foreach` beim Einfangen der Laufvariablen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Der Multicast-Delegat nach den drei Zuweisungen:**

Nach `melder = Kurz; melder += Lang; melder += Kurz;` enthält die Aufrufliste `[Kurz, Lang, Kurz]`. Der Aufruf `melder("a")` arbeitet sie in dieser Reihenfolge ab:

```
[kurz] a
[lang] A!
[kurz] a
```

**Schritt 2 — `-=` entfernt das letzte Vorkommen:**

`melder -= Kurz` sucht das *letzte* Vorkommen von `Kurz` und entfernt genau dieses eine. Die Liste ist jetzt `[Kurz, Lang]`, und `melder("b")` gibt aus:

```
[kurz] b
[lang] B!
```

**Schritt 3 — Die `for`-Schleife teilt eine Variable:**

Die drei Lambdas `() => Console.WriteLine($"i = {i}")` fangen nicht den Wert von `i` ein, sondern die Variable. Die `for`-Schleife hat nur *eine* Variable `i` für alle Durchläufe; nach dem letzten Durchlauf wurde `i++` ausgeführt und die Bedingung `i <= 3` verletzt – `i` ist 4. Erst dann werden die Lambdas ausgeführt:

```
i = 4
i = 4
i = 4
```

**Schritt 4 — Die `foreach`-Schleife legt pro Durchlauf eine neue Variable an:**

Seit C# 5 ist die Laufvariable von `foreach` für jeden Durchlauf eine eigene Variable. Das erste Lambda hat `wert = 10` eingefangen, das zweite `wert = 20`. `summe` dagegen ist eine einzige äußere Variable, die beide Lambdas teilen und verändern:

```
summe = 10
summe = 30
```

**Schritt 5 — Der leere Delegat:**

`melder -= Lang` macht aus `[Kurz, Lang]` die Liste `[Kurz]`, `melder -= Kurz` entfernt auch das letzte Element – das Ergebnis ist nicht ein leerer Delegat, sondern `null`. `melder?.Invoke("c")` tut daher nichts, und die letzte Zeile gibt `True` aus.

**Zentrale Designentscheidungen:**

- **Closures fangen Variablen ein, keine Werte:** Wer einen Wert festhalten will, muss ihn innerhalb des Schleifenrumpfs in eine neue lokale Variable kopieren (`int kopie = i;`).
- **`-=` ist kein „alle entfernen“:** Hat man eine Methode mehrfach registriert – ein häufiger Fehler bei Ereignissen, wenn `+=` in einer Methode steht, die mehrfach aufgerufen wird – muss man sie ebenso oft abmelden.
- **`?.Invoke` statt direktem Aufruf:** Ein Delegat ohne Methoden ist `null`, nicht leer. Der Aufruf `melder("c")` hätte eine `NullReferenceException` ausgelöst.

</details>

## Aufgabe 2 — Abstraktion

Ein Temperatursensor in einem Serverraum misst regelmäßig die Temperatur. Überschreitet sie einen Schwellwert, sollen drei unabhängige Komponenten reagieren: eine **Anzeige** (gibt die aktuelle Temperatur aus), ein **Logger** (merkt sich alle Überschreitungen mit Zeitpunkt) und ein **Alarm** (löst aber erst aus, wenn die Temperatur den Schwellwert um mehr als 5 Grad überschreitet). Der Sensor soll keine der drei Komponenten kennen.

Entwirf das System mit einem Ereignis `SchwellwertUeberschritten`:
- Welche Daten muss das Ereignis transportieren? Wie sieht die `EventArgs`-Klasse aus?
- Wer registriert die Empfänger beim Sensor – der Sensor, die Empfänger selbst oder ein Dritter?
- Soll das Ereignis bei *jeder* Messung über dem Schwellwert ausgelöst werden oder nur beim *Übergang* von unter nach über dem Schwellwert? Was bedeutet die Entscheidung für den Logger?
- Wo gehört die „mehr als 5 Grad“-Regel des Alarms hin: in den Sensor oder in den Alarm?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Ereignisdaten:**

Die Empfänger brauchen die gemessene Temperatur und den Schwellwert (damit der Alarm die Differenz berechnen kann). Der Zeitpunkt wird beim Erzeugen festgehalten, damit alle Empfänger denselben sehen. Die Properties sind nur lesbar:

```csharp
class SchwellwertEventArgs : EventArgs
{
    public double Temperatur { get; }
    public double Schwellwert { get; }
    public DateTime Zeitpunkt { get; }

    public SchwellwertEventArgs(double temperatur, double schwellwert)
    {
        Temperatur = temperatur;
        Schwellwert = schwellwert;
        Zeitpunkt = DateTime.Now;
    }
}
```

**Schritt 2 — Der Sensor als Sender:**

Wir entscheiden uns, das Ereignis nur beim *Übergang* auszulösen – sonst würde der Logger bei einer Messung pro Sekunde jede Sekunde einen Eintrag schreiben, solange es warm ist. Dafür merkt sich der Sensor, ob er sich bereits über dem Schwellwert befindet:

```csharp
class Temperatursensor
{
    public double Schwellwert { get; }
    private bool ueberSchwellwert;

    public event EventHandler<SchwellwertEventArgs>? SchwellwertUeberschritten;

    public Temperatursensor(double schwellwert)
    {
        Schwellwert = schwellwert;
    }

    public void MessungEintragen(double temperatur)
    {
        bool jetztDrueber = temperatur > Schwellwert;
        if (jetztDrueber && !ueberSchwellwert)
            OnSchwellwertUeberschritten(new SchwellwertEventArgs(temperatur, Schwellwert));
        ueberSchwellwert = jetztDrueber;
    }

    protected virtual void OnSchwellwertUeberschritten(SchwellwertEventArgs e)
    {
        SchwellwertUeberschritten?.Invoke(this, e);
    }
}
```

**Schritt 3 — Die drei Empfänger:**

Jeder Empfänger bringt eine Methode `Anmelden(Temperatursensor sensor)` mit und registriert seine eigene Behandlungsmethode. Die Alarm-Regel steckt im Alarm, nicht im Sensor – der Sensor soll nicht wissen, dass es einen Alarm gibt, geschweige denn, wann er auslöst:

```csharp
class Anzeige
{
    public void Anmelden(Temperatursensor sensor) => sensor.SchwellwertUeberschritten += Aktualisieren;

    private void Aktualisieren(object? sender, SchwellwertEventArgs e)
        => Console.WriteLine($"Anzeige: {e.Temperatur:F1} °C (Grenze {e.Schwellwert:F1} °C)");
}

class Logger
{
    private readonly List<string> eintraege = [];
    public IReadOnlyList<string> Eintraege => eintraege;

    public void Anmelden(Temperatursensor sensor) => sensor.SchwellwertUeberschritten += Protokollieren;

    private void Protokollieren(object? sender, SchwellwertEventArgs e)
        => eintraege.Add($"{e.Zeitpunkt:HH:mm:ss} – {e.Temperatur:F1} °C");
}

class Alarm
{
    private const double Toleranz = 5.0;

    public void Anmelden(Temperatursensor sensor) => sensor.SchwellwertUeberschritten += Pruefen;

    private void Pruefen(object? sender, SchwellwertEventArgs e)
    {
        if (e.Temperatur - e.Schwellwert > Toleranz)
            Console.WriteLine("ALARM: Kritische Temperatur!");
    }
}
```

**Schritt 4 — Verdrahtung durch einen Dritten:**

Weder der Sensor noch die Empfänger entscheiden, wer mit wem verbunden ist. Das macht der aufrufende Code – im Geometrieeditor war das die Gui-Schicht, die den `IFigurSpeicher` in die `FigurenVerwaltung` steckt, hier ist es das Hauptprogramm:

```csharp
Temperatursensor sensor = new(schwellwert: 30.0);
Anzeige anzeige = new();
Logger logger = new();
Alarm alarm = new();

anzeige.Anmelden(sensor);
logger.Anmelden(sensor);
alarm.Anmelden(sensor);

sensor.MessungEintragen(28.0);   // nichts
sensor.MessungEintragen(31.5);   // Anzeige: 31,5 °C (Grenze 30,0 °C)
sensor.MessungEintragen(33.0);   // nichts – immer noch drüber, kein Übergang
sensor.MessungEintragen(27.0);   // nichts – wieder drunter
sensor.MessungEintragen(36.0);   // Anzeige: 36,0 °C (Grenze 30,0 °C)
                                 // ALARM: Kritische Temperatur!
Console.WriteLine(logger.Eintraege.Count);   // 2
```

**Zentrale Designentscheidungen:**

- **Der Sensor kennt keinen Empfänger:** Er hat nur ein Ereignis und eine `On…`-Methode. Ein vierter Empfänger (SMS-Versand) braucht keine Änderung am Sensor.
- **Flankenerkennung im Sender:** Ob bei jeder Messung oder nur beim Übergang ausgelöst wird, ist eine fachliche Entscheidung des Senders – sonst müsste jeder Empfänger den vorigen Zustand selbst nachhalten. Der Preis: Bleibt die Temperatur dauerhaft hoch, gibt es nur ein Ereignis. Für einen Alarm ist das gewollt, für eine Live-Anzeige bräuchte man ein zweites Ereignis `MessungEingetragen`.
- **Empfängerspezifische Regeln beim Empfänger:** Die 5-Grad-Toleranz gehört in den `Alarm`. Läge sie im Sensor, wäre er wieder mit einem konkreten Empfänger verheiratet.
- **Unveränderliche `EventArgs`:** Alle drei Empfänger sehen dieselben Daten, weil keiner sie verändern kann.

</details>

## Aufgabe 3 — Mustererkennung

Gegeben ist eine Liste von Figuren aus dem Geometrieeditor (`Figur` mit `Name`, `X`, `Y`, `Flaeche`, `Umfang`; abgeleitet `Kreis` mit `Radius` und `Rechteck` mit `Breite`, `Hoehe`). Übersetze die Abfragen (a) bis (c) von Query- in Methodensyntax, (d) und (e) von Methoden- in Query-Syntax. Schreibe dann (f) und begründe, warum diese Abfrage nur in Methodensyntax vollständig ausdrückbar ist.

```csharp
// (a)
var a = from f in figuren where f.Flaeche > 10 select f.Name;

// (b)
var b = from f in figuren orderby f.Umfang descending, f.Name select f;

// (c)
var c = from f in figuren where f is Kreis orderby f.Flaeche select $"{f.Name}: {f.Flaeche:F1}";

// (d)
var d = figuren.Where(f => f.X > 0 && f.Y > 0).Select(f => f.Name.ToUpper());

// (e)
var e = figuren.Where(f => f is Rechteck).OrderBy(f => f.Name).Select(f => f.Umfang);

// (f) Die zwei Namen mit der größten Fläche – ohne Duplikate, falls Namen doppelt vorkommen.
```

Leitfragen:
- Welches Schlüsselwort entspricht welcher Methode – und in welcher Reihenfolge?
- Wie drückt man `descending` und einen zweiten Sortierschlüssel in Methodensyntax aus?
- Was passiert bei `select f` – braucht man dafür ein `Select`?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Query nach Methode (a bis c):**

Jedes `where` wird zu `Where`, `orderby` zu `OrderBy` (bzw. `OrderByDescending`), ein zweiter Sortierschlüssel zu `ThenBy`, und `select` zu `Select`. Ein `select f`, das das Element unverändert durchreicht, erzeugt in Methodensyntax keinen Aufruf:

```csharp
var a = figuren.Where(f => f.Flaeche > 10).Select(f => f.Name);

var b = figuren.OrderByDescending(f => f.Umfang).ThenBy(f => f.Name);

var c = figuren.Where(f => f is Kreis)
               .OrderBy(f => f.Flaeche)
               .Select(f => $"{f.Name}: {f.Flaeche:F1}");
```

**Schritt 2 — Methode nach Query (d und e):**

Umgekehrt wird jede Methode wieder zu einem Schlüsselwort. Die Reihenfolge bleibt erhalten, das `from` kommt hinzu:

```csharp
var d = from f in figuren
        where f.X > 0 && f.Y > 0
        select f.Name.ToUpper();

var e = from f in figuren
        where f is Rechteck
        orderby f.Name
        select f.Umfang;
```

**Schritt 3 — Nur in Methodensyntax (f):**

Die Query-Syntax kennt weder `Take` noch `Distinct`. Man kann eine Query-Abfrage in Klammern setzen und die fehlenden Methoden anhängen – aber vollständig in Query-Syntax geht es nicht:

```csharp
var f = figuren.OrderByDescending(x => x.Flaeche)
               .Select(x => x.Name)
               .Distinct()
               .Take(2);

// Mischform: Query-Teil in Klammern, Rest als Methoden
var fGemischt = (from x in figuren orderby x.Flaeche descending select x.Name)
                .Distinct()
                .Take(2);
```

Die Reihenfolge von `Distinct` und `Take` ist entscheidend: `Take(2).Distinct()` könnte nur *einen* Namen liefern, wenn die beiden größten Figuren gleich heißen.

**Zentrale Designentscheidungen:**

- **Die Übersetzung ist mechanisch:** Der Compiler macht genau das, was wir hier von Hand getan haben – deshalb sind beide Formen gleichwertig und gleich schnell.
- **`select f` ist ein Leerlauf:** Eine Identitätsprojektion erzeugt keinen `Select`-Aufruf. Das ist eine Optimierung, keine Ausnahme.
- **Methodensyntax ist die Obermenge:** Sobald `Take`, `Skip`, `Distinct`, `Any`, `Count` oder `ToList` gebraucht werden, kommt man um Methoden nicht herum. Die Mischform ist erlaubt, aber selten lesbarer als die reine Methodenkette.

</details>

## Aufgabe 4 — Mustererkennung

Der folgende Taschenrechner wählt die Operation über eine `switch`-Anweisung aus. Jede neue Operation erfordert einen weiteren `case` – und der Rechner kann nur, was zur Kompilierzeit im `switch` steht.

```csharp
static double Berechne(string operation, double a, double b)
{
    switch (operation)
    {
        case "+": return a + b;
        case "-": return a - b;
        case "*": return a * b;
        case "/": return a / b;
        case "max": return Math.Max(a, b);
        default: throw new ArgumentException($"Unbekannte Operation: {operation}");
    }
}
```

Erkenne das Muster, das sich in allen `case`-Zweigen wiederholt, und ersetze den `switch` durch eine Tabelle `Dictionary<string, Func<double, double, double>>`.
- Was haben alle Zweige gemeinsam – welche Signatur steckt dahinter?
- Wie sieht der Eintrag für `Math.Max` aus? Braucht er ein Lambda?
- Wie kann ein Nutzer des Rechners zur Laufzeit eine neue Operation hinzufügen, ohne den Rechner zu ändern?
- Wie behandelst du eine unbekannte Operation – und wie listest du alle verfügbaren auf?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Muster erkennen:**

Jeder `case` bildet zwei `double` auf ein `double` ab. Alle Zweige haben also die Signatur `double (double, double)` – das ist ein `Func<double, double, double>`. Der `switch` ist nichts anderes als eine Zuordnung von Name zu Funktion, und dafür gibt es eine Datenstruktur: das `Dictionary`.

**Schritt 2 — Die Tabelle:**

```csharp
class Rechner
{
    private readonly Dictionary<string, Func<double, double, double>> operationen = new()
    {
        ["+"] = (a, b) => a + b,
        ["-"] = (a, b) => a - b,
        ["*"] = (a, b) => a * b,
        ["/"] = (a, b) => a / b,
        ["max"] = Math.Max
    };

    public IEnumerable<string> VerfuegbareOperationen => operationen.Keys;

    public void Registrieren(string name, Func<double, double, double> operation)
    {
        operationen[name] = operation;
    }

    public double Berechne(string operation, double a, double b)
    {
        if (!operationen.TryGetValue(operation, out Func<double, double, double>? rechnung))
            throw new ArgumentException($"Unbekannte Operation: {operation}");
        return rechnung(a, b);
    }
}
```

`Math.Max` braucht kein Lambda: Die Methodengruppe wird direkt zugewiesen, und der Compiler wählt aus den Überladungen von `Math.Max` diejenige mit zwei `double`-Parametern aus. Für `+` und `-` gibt es keine Methode, die man zuweisen könnte – Operatoren sind keine Methodengruppen – deshalb hier Lambdas.

**Schritt 3 — Erweiterung zur Laufzeit:**

```csharp
Rechner rechner = new();
rechner.Registrieren("pow", Math.Pow);
rechner.Registrieren("hyp", (a, b) => Math.Sqrt(a * a + b * b));

Console.WriteLine(rechner.Berechne("hyp", 3, 4));   // 5
Console.WriteLine(rechner.Berechne("pow", 2, 10));  // 1024
Console.WriteLine(string.Join(", ", rechner.VerfuegbareOperationen));
// +, -, *, /, max, pow, hyp
```

Der Rechner wurde für `pow` und `hyp` nicht angefasst. Ein `switch` hätte das nicht erlaubt.

**Zentrale Designentscheidungen:**

- **Daten statt Kontrollfluss:** Eine Zuordnung „Name → Verhalten“ ist eine Tabelle, kein Verzweigungsbaum. Sobald ein `switch` in jedem Zweig dasselbe Muster hat, ist das ein Signal für eine Delegat-Tabelle.
- **Offen für Erweiterung, geschlossen für Änderung:** Neue Operationen kommen über `Registrieren` hinzu; der Code in `Berechne` bleibt unverändert. Das ist dasselbe Prinzip wie bei Ereignissen – der Sender kennt seine Empfänger nicht.
- **`TryGetValue` statt Indexer:** Der Indexer würde bei einem unbekannten Schlüssel eine `KeyNotFoundException` werfen, deren Meldung dem Nutzer nichts sagt. Mit `TryGetValue` formulieren wir die Fehlermeldung selbst.
- **Das `Dictionary` ist `private`:** Von außen gibt es nur `Registrieren`, `Berechne` und die Liste der Namen. Niemand kann versehentlich `operationen.Clear()` aufrufen – dieselbe Überlegung wie beim `event`-Schlüsselwort gegenüber einem öffentlichen Delegatfeld.

</details>
