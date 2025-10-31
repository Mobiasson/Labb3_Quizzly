# QUIZZLY

## Översikt
Bygg en **WPF-applikation** i **MVVM-arkitektur** för att:
- Skapa och konfigurera **frågepaket** (Question Packs)
- Köra **quiz-rundor** i spelläge
- (VG) Importera frågor via **Open Trivia DB API**

---

## Projektstruktur (XAML)

| Mapp       | Innehåll |
|------------|---------|
| `Dialogs/` | `PackOptionsDialog.xaml`<br>`CreateNewPackDialog.xaml` |
| `Views/`   | `MenuView.xaml` (huvudmeny)<br>`ConfigurationView.xaml` (editeringsläge)<br>`PlayerView.xaml` (spelläge) |

> `MainWindow.xaml`:  
> - `MenuView` högst upp  
> - `ConfigurationView` **eller** `PlayerView` i huvudområde (växla med kommentar för test)

---

## Funktioner – Godkänt (G)

### Configuration Mode
- Skapa, ta bort, redigera **Question Packs**
- Lägg till/ta bort/redigera **frågor** (4 svarsalternativ, 1 rätt)
- **Pack Options**:
  - Namn
  - Svårighetsgrad
  - Tidsgräns per fråga
- Åtkomst via:
  - Meny
  - Knappar i UI
  - Tangentbordsgenvägar (t.ex. `Ctrl+N`, `Del`)

### Play Mode
- Visar: `Fråga X av Y`
- **Slumpad ordning** på:
  - Frågor
  - Svarsalternativ
- **Nedräknare** per fråga (baserat på pack-inställning)
- Feedback efter svar: **rätt/fel + korrekt svar**
- Efter sista frågan: **resultatvy** (antal rätt)

---

## Meny & UX
- Ikoner (t.ex. **Font Awesome**)
- Tangentbordsstöd:
  - `Ctrl+...`
  - `Alt+E, O` (Pack Options)
- **Helskärmsläge** (via meny)

---

## Lagring (JSON) – Välj ett
1. **En fil i AppData**  
   ```csharp
   Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
