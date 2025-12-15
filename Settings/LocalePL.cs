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
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Akcje" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Informacje" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Akcje" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Skróty klawiszowe" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Linki" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nazwa moda" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Wyświetlana nazwa tego moda." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Wersja" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Aktualna wersja Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Otwórz stronę autora na Paradox Mods." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Automatycznie otwieraj Zone Tools przy narzędziach dróg." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Gdy włączone, panel Zone Tools otwiera się automatycznie po wybraniu narzędzia drogi z zonowaniem.\n" +
                    "Wyłącz, aby otwierać panel ręcznie."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Chroń zajęte komórki (budynki)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Gdy włączone, Zone Tools nie zmienia głębokości/obszaru strefy na komórkach, które mają już budynek."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Chroń już wyznaczone (puste) komórki" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Gdy włączone, Zone Tools nie zmienia głębokości/obszaru strefy na komórkach już oznaczonych strefą (nawet jeśli puste)."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Przełącz panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Skrót klawiszowy do pokazania/ukrycia panelu Zone Tools (jak ikona w lewym górnym rogu)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Przełącz panel" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Aktualizuj drogę" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Przełącz narzędzie aktualizacji (istniejące drogi)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Obie strony (domyślnie)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Lewa" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Prawa" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Brak" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
