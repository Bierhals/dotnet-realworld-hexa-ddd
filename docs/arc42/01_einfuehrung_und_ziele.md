# Einführung und Ziele

Das Projekt "dotnet-realworld-hexa-ddd" implementiert das Backend für einen medium.com Clone mit dem Namen Conduit auf Basis des [RealWorld-Projektes](https://github.com/gothinkster/realworld).

Ich will damit vor allem
- neue Technologien ausprobieren
- bewährte Architekturmuster einsetzen

## Aufgabenstellung

Conduit stellt einen medium.com Clone dar, welcher die folgenden Funktionen bietet:
- Aritkel anzeigen und filtern
- Eigene Artikel erstellen und ändern
- Verwalten von bevorzugten Artikeln
- Benutzern folgen
- Authentifzierung

Diese Implementierung stellt nur das Backend bereit und ist Kompatibel mit der [Backend-Spezifikation](https://realworld-docs.netlify.app/docs/specs/backend-specs/introduction) der RealWorld-Applikation.

## Qualitätsziele

Die folgende Tabelle beschreibt die zentralen Qualitätsziele, wobei die Reihenfolge eine grobe Orientierung bezüglich der Wichtigkeit vorgibt.

| Nr. | Qualitätsziel | Motivation und Erläuterung |
|----|----|----|
| 1 | Wartbarkeit | Leicht zu verstehen und gut zu erweitern |
| 2 | Kompatibilität | Setzt die Backendspezifikation um und kann mit anderen Frontend-Implementierungen genutzt werden |
| 3 | Sicherheit | Es wird sichergestellt, dass zu jeder Zeit die Integrität der Daten gewahrt bleibt. Alle Änderungen müssen müssen auf den Benutzer zurückzuführen sein, welcher diese veranlasst hat |

## Stakeholder

| Rolle | Erwartungshaltung |
|----|----|
| RealWorl-Projekt | Erwartet eine mit anderen Immplentierungen kompatible Umsetzung. siehe [Dokumentation](https://realworld-docs.netlify.app/docs/implementation-creation/expectations) |
| Architekten | Ein einfach zu wartendes System, welches auch nach Jahren noch weiterentwickelt werden kann. |
| Entwickler | Überbllick über mögliche Umsetzungsmuster |

