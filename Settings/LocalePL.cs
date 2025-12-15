// Settings/LocalePL.cs
// Polish pl-PL for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocalePL : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocalePL(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "Informacje" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "Informacje" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Skróty klawiszowe" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nazwa moda" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Wyświetlana nazwa tego moda." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Wersja" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Aktualna wersja Zone Tools." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Pokaż/ukryj panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Skrót klawiszowy do pokazania lub ukrycia panelu Zone Tools (tak samo jak kliknięcie ikony menu w lewym górnym rogu)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Pokaż/ukryj panel" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "Aktualizuj drogę" },

                {
                    "ZoneTools.UI.Tooltip.UpdateRoad",
                    "Włącz/wyłącz narzędzie aktualizacji (dla istniejących dróg). Drogi z budynkami na strefach są pomijane."
                },
                { "ZoneTools.UI.Tooltip.ModeDefault", "Domyślnie (obie strony)" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "Lewa" },
                { "ZoneTools.UI.Tooltip.ModeRight", "Prawa" },
                { "ZoneTools.UI.Tooltip.ModeNone", "Brak" }

            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
