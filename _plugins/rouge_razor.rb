# Rouge kennt keine Razor-Syntax. Ohne Lexer rendert kramdown ```razor-Blöcke
# als ungestyltes <pre>. Dieser Lexer behandelt Razor wie HTML, sodass die
# Blöcke wie alle anderen Code-Blöcke aussehen (Tags und Attribute hervorgehoben,
# @-Ausdrücke als normaler Text).

require "rouge"

module Rouge
  module Lexers
    class Razor < HTML
      title "Razor"
      desc "Razor-Markup (Blazor-Komponenten), hervorgehoben wie HTML"
      tag "razor"
      aliases "cshtml"
      filenames "*.razor", "*.cshtml"
    end
  end
end
