# PlantUML-Diagramme beim Build als Inline-SVG einbetten.
#
#   {% plantuml Klassenhierarchie des Adventure %}
#   abstract class Spielobjekt
#   class Wand
#   Spielobjekt <|-- Wand
#   {% endplantuml %}
#
# Der Text hinter "plantuml" wird zum Alternativtext (<title>) der Grafik.
# Gerendert wird mit vendor/plantuml.jar (oder $PLANTUML_JAR) und dem
# eingebauten Layout "smetana", sodass kein Graphviz nötig ist.
# Ergebnisse werden in .jekyll-cache/plantuml/ zwischengespeichert.

require "digest"
require "fileutils"
require "open3"

module PlantUml
  STIL = <<~PUML
    !pragma layout smetana
    skinparam monochrome true
    skinparam shadowing false
    skinparam backgroundColor transparent
    skinparam defaultFontName sans-serif
    skinparam classFontName monospace
    skinparam roundCorner 6
    hide circle
    hide empty members
  PUML

  class Block < Liquid::Block
    def initialize(tag_name, markup, tokens)
      super
      @titel = markup.strip
    end

    def render(context)
      site = context.registers[:site]
      quelltext = "@startuml\n#{STIL}#{super.strip}\n@enduml\n"
      svg = PlantUml.svg(site, quelltext)
      svg = svg.sub(/\A.*?(<svg)/m, '\1')
      # Feste Größe von PlantUML durch eine mitskalierende ersetzen.
      svg = svg.sub(/<svg\b[^>]*>/) do |tag|
        breite = tag[/\bwidth="(\d+)px"/, 1]
        tag = tag.gsub(/\s(style|width|height|preserveAspectRatio)="[^"]*"/, "")
        tag.sub("<svg", %(<svg role="img" style="width:#{breite}px;max-width:100%;height:auto"))
      end
      svg = svg.sub(/(<svg[^>]*>)/, "\\1<title>#{escape(@titel)}</title>") unless @titel.empty?
      # Eine Zeile ohne Leerzeilen, damit kramdown den Block als HTML durchreicht.
      "\n\n<div class=\"plantuml\">#{svg.gsub(/\s*\n\s*/, ' ')}</div>\n\n"
    end

    private

    def escape(text)
      text.gsub("&", "&amp;").gsub("<", "&lt;").gsub(">", "&gt;")
    end
  end

  def self.svg(site, quelltext)
    cache = File.join(site.source, ".jekyll-cache", "plantuml")
    FileUtils.mkdir_p(cache)
    datei = File.join(cache, "#{Digest::SHA256.hexdigest(quelltext)}.svg")
    return File.read(datei) if File.exist?(datei)

    jar = ENV.fetch("PLANTUML_JAR", File.join(site.source, "vendor", "plantuml.jar"))
    raise "PlantUML nicht gefunden: #{jar} (siehe CLAUDE.md)" unless File.exist?(jar)

    ausgabe, fehler, status = Open3.capture3(
      "java", "-Djava.awt.headless=true", "-jar", jar, "-tsvg", "-pipe", "-charset", "UTF-8",
      stdin_data: quelltext
    )
    raise "PlantUML-Fehler:\n#{fehler}\n#{quelltext}" unless status.success?

    File.write(datei, ausgabe)
    ausgabe
  end
end

Liquid::Template.register_tag("plantuml", PlantUml::Block)
