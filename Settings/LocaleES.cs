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
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Acciones" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Atajos de teclado" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Enlaces" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nombre del mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nombre visible de este mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual de Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Abrir la página del autor en Paradox Mods." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Abrir Zone Tools con las herramientas de carreteras." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Si está activado, el panel de Zone Tools se abre automáticamente al usar una carretera con zonificación.\n" +
                    "Desactívalo para abrir el panel manualmente."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Proteger celdas ocupadas (con edificios)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Si está activado, Zone Tools no cambiará la profundidad/área de zonificación en celdas que ya tienen un edificio."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Proteger celdas zonificadas pero vacías" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Si está activado, Zone Tools no cambiará la profundidad/área de zonificación en celdas ya zonificadas (aunque estén vacías)."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Mostrar/ocultar panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Atajo de teclado para mostrar u ocultar el panel de Zone Tools (igual que el icono arriba a la izquierda)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Mostrar/ocultar panel" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Actualizar carretera" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Activar herramienta de actualización (carreteras existentes)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Ambos (predeterminado)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Izquierda" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Derecha" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Ninguno" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
