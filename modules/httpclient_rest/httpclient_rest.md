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

Bisher lagen alle Daten auf der eigenen Festplatte. Die meisten Programme, die du täglich benutzt, holen ihre Daten aber aus dem Netz: die Wetter-App fragt einen Server nach der Temperatur, der Messenger nach neuen Nachrichten, das Spiel nach der Bestenliste. Das Erstaunliche ist, wie wenig sich dabei für uns ändert. Ein Server schickt Text – meistens JSON –, und den können wir mit demselben `JsonSerializer` aus dem [vorigen Modul](/modules/json_serialisierung/json_serialisierung.md) in Objekte verwandeln. Nur der Weg, den Text zu bekommen, ist neu: statt `File.ReadAllText` heißt das Werkzeug `HttpClient`.

## HTTP in Kürze

Das Web funktioniert nach dem Frage-Antwort-Prinzip. Der **Client** schickt eine **Anfrage** (*request*) an eine **URL**, der **Server** antwortet mit einer **Antwort** (*response*). Die wichtigste Anfrageart ist `GET` – „gib mir, was unter dieser Adresse liegt“. Jede Antwort trägt einen **Statuscode**: `200` heißt „alles gut“, `404` „nicht gefunden“, `500` „Fehler auf dem Server“. Dahinter folgt der Inhalt (*body*), bei Webseiten HTML, bei Datenschnittstellen fast immer JSON.

Eine Schnittstelle, die nach diesem Muster Daten unter sprechenden URLs anbietet, nennt man **REST-API**. Die URL benennt eine Ressource, die Anfrageart sagt, was damit geschehen soll, und die Antwort ist JSON. Ein Beispiel ist der Wetterdienst Open-Meteo, der ohne Anmeldung antwortet:

```
GET https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m

200 OK
{"latitude":52.52,"longitude":13.42,"timezone":"GMT", ...,
 "current":{"time":"2026-09-10T12:15","interval":900,"temperature_2m":18.6}}
```

Die Parameter nach dem `?` filtern die Anfrage – hier Koordinaten und die gewünschte Größe. Genau diese URL kannst du auch im Browser öffnen; der Browser ist nur ein besonders komfortabler HTTP-Client.

## Eine Instanz für alles: `HttpClient`

Die Klasse `HttpClient` aus `System.Net.Http` übernimmt in .NET die Rolle des Clients. Sie implementiert zwar `IDisposable`, ist aber die berühmte Ausnahme von der Faustregel aus dem Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md): Jede Instanz verwaltet einen Pool von Netzwerkverbindungen, und wer für jede Anfrage einen neuen Client erzeugt und wieder wegwirft, lässt halboffene Verbindungen zurück, bis das System keine mehr hergibt. Deshalb hält man **eine** Instanz für das ganze Programm:

```csharp
using System.Net.Http;

HttpClient client = new HttpClient();   // in einer Klasse: private static readonly HttpClient client = new();

string url = "https://www.erodner.de/prog2-lecture/assets/data/figuren.json";
string text = await client.GetStringAsync(url);

Console.WriteLine(text.Length);       // 438
Console.WriteLine(text[..30]);        // [
                                      //   {
                                      //     "typ": "rechteck",
```

`GetStringAsync` schickt ein `GET`, wartet auf die Antwort und liefert den Inhalt als `string` – das Gegenstück zu `File.ReadAllText`, nur für URLs. Das `await` davor ist neu und verdient eine kurze Erklärung.

Netzwerkzugriffe dauern lange – Millisekunden bis Sekunden, in denen das Programm sonst nur warten würde. Methoden, die auf `Async` enden, blockieren deshalb nicht, sondern geben sofort ein `Task<T>` zurück: ein Versprechen auf ein späteres Ergebnis. Das Schlüsselwort `await` löst dieses Versprechen ein: Es wartet, ohne den Thread zu blockieren, und liefert danach das eigentliche Ergebnis, hier den `string`. In Top-Level-Statements darf man `await` direkt verwenden; in einer eigenen Methode muss diese `async Task` (oder `async Task<T>`) als Rückgabetyp haben. Bei einer GUI ist das entscheidend: Ohne `await` würde das Fenster während des Downloads einfrieren. Mehr über `async`/`await` erfährst du in weiterführenden Veranstaltungen – für dieses Modul genügt: `await` vor jeden `Async`-Aufruf.
{: .notice--primary}

## JSON direkt in Objekte: `GetFromJsonAsync<T>`

Den Text könnte man nun mit `JsonSerializer.Deserialize<List<Figur>>(text)` in Objekte verwandeln. Weil das so häufig gebraucht wird, gibt es im Namensraum `System.Net.Http.Json` eine Abkürzung, die Herunterladen und Deserialisieren in einem Schritt erledigt. Da die Datei mit dem `typ`-Diskriminator aus `Figur.cs` geschrieben wurde, landen die Figuren direkt als richtige Laufzeittypen in der Liste:

```csharp
using System.Net.Http.Json;
using Geometrieeditor.Fachkonzept;

List<Figur>? figuren = await client.GetFromJsonAsync<List<Figur>>(url);

foreach (Figur f in figuren ?? [])
{
    Console.WriteLine(f.Beschreibung());
}
// r1 bei (0, 0) mit Fläche 12,00 (4 x 3)
// k1 bei (10, 5) mit Fläche 19,63 (r = 2,5)
// d1 bei (-3, 7) mit Fläche 6,00
// quadrat bei (2, 2) mit Fläche 2,25 (1,5 x 1,5)
```

`GetFromJsonAsync<T>` verwendet Standardoptionen für das Web: Property-Namen werden ohne Rücksicht auf Groß-/Kleinschreibung zugeordnet, deshalb passt sowohl `"Name"` als auch `"name"`. Das Ergebnis ist `null`, wenn der Server das JSON-Literal `null` schickt – daher der `?`-Typ und das `?? []`.

## Eine Modellklasse für eine fremde API

Bei `figuren.json` hatten wir die Klassen bereits. Bei einer fremden API ist es umgekehrt: Das JSON ist vorgegeben, und wir schreiben Klassen, die zu seiner Struktur passen. Man nimmt nur die Felder auf, die man braucht – alles andere überspringt der Serialisierer. Für die Open-Meteo-Antwort von oben reicht ein verschachteltes Objekt, und für den Schlüssel `temperature_2m`, der kein gültiger C#-Name ist, hilft `[JsonPropertyName]`:

```csharp
using System.Text.Json.Serialization;

class WetterAntwort
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public AktuelleWerte? Current { get; set; }
}

class AktuelleWerte
{
    public string Time { get; set; } = "";

    [JsonPropertyName("temperature_2m")]
    public double Temperatur { get; set; }
}
```

`Current` entspricht dem JSON-Objekt `"current"`, das wiederum `"time"` und `"temperature_2m"` enthält – die Verschachtelung der Klassen spiegelt die Verschachtelung des JSON. Mit dieser Modellklasse ist die Wetterabfrage ein Dreizeiler:

```csharp
string wetterUrl = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m";
WetterAntwort? wetter = await client.GetFromJsonAsync<WetterAntwort>(wetterUrl);

Console.WriteLine($"Berlin, {wetter?.Current?.Time}: {wetter?.Current?.Temperatur} °C");
// Berlin, 2026-09-10T12:15: 18,6 °C
```

Wer eine neue API erkundet, öffnet die URL zuerst im Browser oder lädt sie mit `GetStringAsync`, schaut sich die Struktur an und schreibt dann die Modellklasse. Bei großen Antworten mit vielen Feldern lohnt sich der Blick in die Dokumentation der API, die meist auch angibt, welche Felder optional sind.

## Wenn der Server nicht mitspielt

Im Netz geht mehr schief als auf der Festplatte: keine Verbindung, falsche URL, überlasteter Server. `GetStringAsync` und `GetFromJsonAsync` werfen in all diesen Fällen eine `HttpRequestException` – auch dann, wenn die Verbindung zwar klappt, der Server aber einen Fehlercode wie `404` zurückgibt. Wer den Statuscode selbst auswerten möchte, arbeitet eine Ebene tiefer mit `GetAsync`, das die ganze Antwort liefert:

```csharp
try
{
    HttpResponseMessage antwort = await client.GetAsync(url);
    Console.WriteLine((int)antwort.StatusCode); // 200
    antwort.EnsureSuccessStatusCode();          // wirft bei 4xx/5xx eine HttpRequestException
    string inhalt = await antwort.Content.ReadAsStringAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Anfrage fehlgeschlagen: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Zeitüberschreitung – der Server hat nicht rechtzeitig geantwortet.");
}
```

`EnsureSuccessStatusCode` ist die explizite Form dessen, was die Komfortmethoden intern tun. Die `TaskCanceledException` tritt auf, wenn die Antwort länger dauert als `client.Timeout` – standardmäßig 100 Sekunden, was man für Benutzeroberflächen meist herabsetzt. Und weil `GetFromJsonAsync` das JSON deserialisiert, kann zusätzlich eine `JsonException` fliegen, wenn die Antwort nicht zur Modellklasse passt.

Ein Programm, das Netzwerkzugriffe macht, muss ohne Netz weiterhin sinnvoll reagieren – mit einer Meldung, zwischengespeicherten Daten oder einem erneuten Versuch. Eine unbehandelte `HttpRequestException` beim Start ist der klassische Fehler, mit dem eine App im Zug unbenutzbar wird.
{: .notice--warning}

## Ausblick: eigene REST-APIs

Was der Server auf der anderen Seite tut, ist kein Geheimnis: Er nimmt eine URL entgegen, ruft eine Methode auf und serialisiert deren Rückgabewert als JSON – also genau die Techniken dieser Vorlesung, nur in Gegenrichtung. Mit ASP.NET Core lässt sich ein solcher Dienst in wenigen Zeilen schreiben; ein `app.MapGet("/figuren", () => verwaltung.AlleFiguren)` genügt bereits, um die Figurenliste des Geometrieeditors im Netz anzubieten. Damit könnten mehrere Instanzen des Editors auf dieselben Figuren zugreifen – der `IFigurSpeicher` bekäme eine dritte Umsetzung, die statt einer Datei einen Server befragt.

Übung: Erweitere die Wetterabfrage um `&daily=temperature_2m_max&forecast_days=3` und modelliere die Antwort (`"daily"` enthält Arrays `"time"` und `"temperature_2m_max"`). Gib die Höchsttemperaturen der nächsten drei Tage aus und behandle den Fall, dass kein Netz verfügbar ist.
{: .notice--info}

## Weitere Quellen

- [Erstellen von HTTP-Anforderungen mit der HttpClient-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/fundamentals/networking/http/httpclient)
- [`HttpClientJsonExtensions` (System.Net.Http.Json) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.net.http.json.httpclientjsonextensions)
- [Asynchrone Programmierung mit async und await – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/asynchronous-programming/)
- [Open-Meteo API-Dokumentation](https://open-meteo.com/en/docs)
