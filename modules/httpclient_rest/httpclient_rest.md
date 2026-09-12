---
title: "`HttpClient` und REST"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Bisher lagen alle Daten auf der eigenen Festplatte. Die meisten Programme, die du täglich benutzt, holen ihre Daten aber aus dem Netz: die Wetter-App fragt einen Server nach der Temperatur, der Messenger nach neuen Nachrichten, das Spiel nach neuen Karten und der Bestenliste. Genau das bauen wir jetzt in unser Adventure ein – eine dritte Umsetzung von `ILevelQuelle`, die die Level nicht aus einem Ordner, sondern von einem Webserver holt. Das Erstaunliche ist, wie wenig sich dabei ändert: Ein Server schickt Text, und den können wir mit demselben `JsonSerializer` aus dem [vorigen Modul](/modules/json_serialisierung/json_serialisierung.md) in Objekte verwandeln. Nur der Weg, den Text zu bekommen, ist neu: statt `File.ReadAllText` heißt das Werkzeug `HttpClient`.

## HTTP in Kürze

Das Web funktioniert nach dem Frage-Antwort-Prinzip. Der **Client** schickt eine **Anfrage** (*request*) an eine **URL**, der **Server** antwortet mit einer **Antwort** (*response*). Die wichtigste Anfrageart ist `GET` – „gib mir, was unter dieser Adresse liegt“. Jede Antwort trägt einen **Statuscode**: `200` heißt „alles gut“, `404` „nicht gefunden“, `500` „Fehler auf dem Server“. Dahinter folgt der Inhalt (*body*), bei Webseiten HTML, bei Datenschnittstellen fast immer JSON.

Eine Schnittstelle, die nach diesem Muster Daten unter sprechenden URLs anbietet, nennt man **REST-API**. Die URL benennt eine Ressource, die Anfrageart sagt, was damit geschehen soll, und die Antwort ist maschinenlesbar. Unsere Level-Schnittstelle ist bewusst so einfach wie möglich: Unter einer Basis-URL liegt ein Verzeichnis mit einer `index.json` und je einer Textdatei pro Level.

```
GET https://www.erodner.de/prog2-lecture/assets/data/levels/index.json

200 OK
["kerker", "katakomben", "schatzkammer"]
```

```
GET https://www.erodner.de/prog2-lecture/assets/data/levels/kerker.txt

200 OK
####################
#@.....#...........#
#......#.....W.....#
...
```

Beide URLs kannst du direkt im Browser öffnen; der Browser ist nur ein besonders komfortabler HTTP-Client. Der Aufbau ist derselbe wie beim Ordner auf der Festplatte – die `index.json` übernimmt die Rolle, die dort `Directory.GetFiles` hatte. Denn über HTTP kann man ein Verzeichnis nicht einfach auflisten: Der Server verrät nur, was man gezielt anfragt.

## Eine Instanz für alles: `HttpClient`

Die Klasse `HttpClient` aus `System.Net.Http` übernimmt in .NET die Rolle des Clients. Sie implementiert zwar `IDisposable`, ist aber die berühmte Ausnahme von der Faustregel aus dem Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md): Jede Instanz verwaltet einen Pool von Netzwerkverbindungen, und wer für jede Anfrage einen neuen Client erzeugt und wieder wegwirft, lässt halboffene Verbindungen zurück, bis das System keine mehr hergibt. Deshalb hält man **eine** Instanz für das ganze Programm – in `HttpLevelQuelle` ist das ein `private static readonly`-Feld:

```csharp
public class HttpLevelQuelle : ILevelQuelle
{
    private static readonly HttpClient http = new();
    private readonly string basisUrl;
    private readonly List<string> namen;
```

Netzwerkzugriffe dauern lange – Millisekunden bis Sekunden, in denen das Programm sonst nur warten würde. Methoden, die auf `Async` enden, blockieren deshalb nicht, sondern geben sofort ein `Task<T>` zurück: ein Versprechen auf ein späteres Ergebnis. Das Schlüsselwort `await` löst dieses Versprechen ein: Es wartet, ohne den Thread zu blockieren, und liefert danach das eigentliche Ergebnis. In Top-Level-Statements darf man `await` direkt verwenden; in einer eigenen Methode muss diese `async Task` (oder `async Task<T>`) als Rückgabetyp haben. Bei einer GUI ist das entscheidend: Ohne `await` würde die Oberfläche während des Downloads einfrieren. Mehr über `async`/`await` erfährst du in weiterführenden Veranstaltungen – für dieses Modul genügt: `await` vor jeden `Async`-Aufruf.
{: .notice--primary}

## JSON direkt in Objekte: `GetFromJsonAsync<T>`

Den Text einer Antwort könnte man mit `JsonSerializer.Deserialize<T>(text)` in Objekte verwandeln. Weil das so häufig gebraucht wird, gibt es im Namensraum `System.Net.Http.Json` eine Abkürzung, die Herunterladen und Deserialisieren in einem Schritt erledigt. Für unsere `index.json` ist der Zieltyp schlicht `List<string>`:

```csharp
/// <summary>Holt zuerst die Liste der Level – deshalb asynchron und über eine Fabrikmethode.</summary>
public static async Task<HttpLevelQuelle> ErzeugenAsync(string basisUrl)
{
    basisUrl = basisUrl.TrimEnd('/') + "/";
    List<string> namen = await http.GetFromJsonAsync<List<string>>(basisUrl + "index.json")
                         ?? new List<string>();
    return new HttpLevelQuelle(basisUrl, namen);
}
```

Hier steckt ein Entwurfsproblem und seine Lösung. Ein Konstruktor kann nicht `async` sein – er muss sein Objekt sofort zurückgeben, nicht erst einen `Task`. Die Klasse braucht aber die Levelnamen, bevor sie benutzbar ist. Deshalb ist der Konstruktor `private`, und eine statische **Fabrikmethode** übernimmt: Sie darf `async` sein, holt die Liste und reicht sie dem Konstruktor. Aufgerufen wird sie mit `await`:

```csharp
ILevelQuelle quelle = await HttpLevelQuelle.ErzeugenAsync(
    "https://www.erodner.de/prog2-lecture/assets/data/levels/");

Console.WriteLine(string.Join(", ", quelle.LevelNamen));
// kerker, katakomben, schatzkammer
```

Ab dieser Zeile merkt das Spiel keinen Unterschied mehr: `quelle` ist eine `ILevelQuelle` wie jede andere, und `LevelParser.Parsen(quelle.Laden("kerker"))` liefert dasselbe `Spielfeld` wie aus einer Datei. Das ist der Lohn der Schichtenarchitektur – der Kern kennt nur das Interface, nicht die Herkunft der Daten.

## Reinen Text holen: `GetStringAsync`

Ein Level ist keine JSON-Datei, sondern eine schlichte Textkarte. Dafür gibt es `GetStringAsync` – das Gegenstück zu `File.ReadAllText`, nur für URLs:

```csharp
public Level Laden(string name)
{
    // Synchron warten ist hier vertretbar: ein Level ist klein und wird einmal geladen.
    string text = http.GetStringAsync(basisUrl + name + ".txt").GetAwaiter().GetResult();
    List<string> zeilen = text.Split('\n')
        .Select(z => z.TrimEnd('\r'))
        .Where(z => z.Trim().Length > 0)
        .ToList();
    return new Level(name, zeilen);
}
```

Die Zeilen werden hier von Hand getrennt, statt sie wie in `TextdateiLevelQuelle` mit `ReadLine` zu lesen – der ganze Text liegt ja bereits als `string` vor. Das `TrimEnd('\r')` fängt Server ab, die die Datei mit Windows-Zeilenenden ausliefern. Auffällig ist `GetAwaiter().GetResult()`: Damit wartet der Aufruf *blockierend* auf das Ergebnis, weil `ILevelQuelle.Laden` eine synchrone Signatur hat, die wir wegen einer einzigen Implementierung nicht ändern wollen. Der Kommentar im Quelltext sagt ehrlich, warum das hier vertretbar ist. In einer GUI-Anwendung mit größeren Datenmengen wäre es das nicht – dort würde man das Interface auf `Task<Level> LadenAsync(string name)` umstellen.
{: .notice--warning}

## Wenn der Server nicht mitspielt

Im Netz geht mehr schief als auf der Festplatte: keine Verbindung, falsche URL, überlasteter Server. `GetStringAsync` und `GetFromJsonAsync` werfen in all diesen Fällen eine `HttpRequestException` – auch dann, wenn die Verbindung zwar klappt, der Server aber einen Fehlercode wie `404` zurückgibt. Wer den Statuscode selbst auswerten möchte, arbeitet eine Ebene tiefer mit `GetAsync`, das die ganze Antwort liefert:

```csharp
try
{
    HttpResponseMessage antwort = await http.GetAsync(basisUrl + "index.json");
    Console.WriteLine((int)antwort.StatusCode);  // 200
    antwort.EnsureSuccessStatusCode();           // wirft bei 4xx/5xx eine HttpRequestException
    string inhalt = await antwort.Content.ReadAsStringAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Level konnten nicht geladen werden: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Zeitüberschreitung – der Server hat nicht rechtzeitig geantwortet.");
}
```

`EnsureSuccessStatusCode` ist die explizite Form dessen, was die Komfortmethoden intern tun. Die `TaskCanceledException` tritt auf, wenn die Antwort länger dauert als `http.Timeout` – standardmäßig 100 Sekunden, was man für Benutzeroberflächen meist herabsetzt. Und weil `GetFromJsonAsync` das JSON deserialisiert, kann zusätzlich eine `JsonException` fliegen, wenn die Antwort nicht zur erwarteten Struktur passt.

Ein Programm, das Netzwerkzugriffe macht, muss ohne Netz weiterhin sinnvoll reagieren. Bei uns ist der Ausweg besonders bequem: Scheitert `ErzeugenAsync`, nimmt man einfach eine andere `ILevelQuelle` – die `TextdateiLevelQuelle` oder notfalls die `EingebauteLevelQuelle`. Eine unbehandelte `HttpRequestException` beim Start ist dagegen der klassische Fehler, mit dem eine App im Zug unbenutzbar wird.
{: .notice--warning}

## Eine Modellklasse für eine fremde API

Bei unseren Levels kannten wir das Format, weil wir es selbst festgelegt haben. Bei einer fremden API ist es umgekehrt: Das JSON ist vorgegeben, und wir schreiben Klassen, die zu seiner Struktur passen. Man nimmt nur die Felder auf, die man braucht – alles andere überspringt der Serialisierer. Der Wetterdienst Open-Meteo antwortet ohne Anmeldung:

```
GET https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m

{"latitude":52.52,"longitude":13.42, ...,
 "current":{"time":"2026-09-10T12:15","interval":900,"temperature_2m":18.6}}
```

Für den Schlüssel `temperature_2m`, der kein gültiger C#-Name ist, hilft `[JsonPropertyName]` aus dem vorigen Modul; die Verschachtelung der Klassen spiegelt die Verschachtelung des JSON:

```csharp
class WetterAntwort
{
    public AktuelleWerte? Current { get; set; }
}

class AktuelleWerte
{
    public string Time { get; set; } = "";

    [JsonPropertyName("temperature_2m")]
    public double Temperatur { get; set; }
}

WetterAntwort? wetter = await http.GetFromJsonAsync<WetterAntwort>(wetterUrl);
Console.WriteLine($"Berlin, {wetter?.Current?.Time}: {wetter?.Current?.Temperatur} °C");
// Berlin, 2026-09-10T12:15: 18,6 °C
```

`GetFromJsonAsync<T>` verwendet Standardoptionen für das Web: Property-Namen werden ohne Rücksicht auf Groß-/Kleinschreibung zugeordnet, deshalb passt `Current` zu `"current"`. Wer eine neue API erkundet, öffnet die URL zuerst im Browser, schaut sich die Struktur an und schreibt dann die Modellklasse.

## Ausblick: eigene REST-APIs

Was der Server auf der anderen Seite tut, ist kein Geheimnis: Er nimmt eine URL entgegen, ruft eine Methode auf und serialisiert deren Rückgabewert als JSON – also genau die Techniken dieser Vorlesung, nur in Gegenrichtung. Mit ASP.NET Core lässt sich ein solcher Dienst in wenigen Zeilen schreiben; ein `app.MapGet("/levels", () => quelle.LevelNamen)` genügt bereits, um unsere Levelliste anzubieten. Ein `app.MapPost("/spielstaende", ...)` daneben, und mehrere Geräte könnten sich denselben Spielstand teilen – der `ISpielstandSpeicher` bekäme eine dritte Umsetzung, die statt einer Datei einen Server befragt.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v09-daten`).

Übung: Erweitere `Program.cs` so, dass das Spiel mit dem Argument `--online` die Level per `HttpLevelQuelle.ErzeugenAsync` holt und sonst wie bisher aus dem Ordner. Fange den Fall ohne Netz ab und falle auf die `EingebauteLevelQuelle` zurück. Miss anschließend mit einer `Stopwatch`, wie lange das erste `Laden` dauert – und überlege, wo ein Zwischenspeicher (*Cache*) sinnvoll wäre.
{: .notice--info}

## Weitere Quellen

- [Erstellen von HTTP-Anforderungen mit der HttpClient-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/fundamentals/networking/http/httpclient)
- [`HttpClientJsonExtensions` (System.Net.Http.Json) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.net.http.json.httpclientjsonextensions)
- [Asynchrone Programmierung mit async und await – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/asynchronous-programming/)
- [Open-Meteo API-Dokumentation](https://open-meteo.com/en/docs)
