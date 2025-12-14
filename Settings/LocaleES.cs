// Settings/LocaleES.cs
// Spanish es-ES for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleES(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "Acerca de" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "Acerca de" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Asignaciones de teclas" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nombre del mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nombre que se muestra para este mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Versión actual de Zone Tools." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Mostrar/ocultar panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Atajo de teclado para mostrar u ocultar el panel de Zone Tools (igual que hacer clic en el icono del menú arriba a la izquierda)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Mostrar/ocultar panel" },
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
