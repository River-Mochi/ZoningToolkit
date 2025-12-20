// Settings/LocaleFR.cs
// French fr-FR for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleFR(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Actions" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Raccourcis clavier" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Liens" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nom du mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nom affiché de ce mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle de Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Ouvrir la page Paradox Mods de l’auteur." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Ouvrir Zone Tools avec les outils de route." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Quand activé, le panneau Zone Tools s’ouvre automatiquement avec un outil de route zonable.\n" +
                    "Désactivez pour l’ouvrir manuellement."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Protéger les cellules occupées (bâtiments)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Quand activé, Zone Tools ne modifie pas la profondeur/la zone sur les cellules qui ont déjà un bâtiment."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Protéger les cellules zonées mais vides" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Quand activé, Zone Tools ne modifie pas la profondeur/la zone sur les cellules déjà zonées (même si vides)."
                },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Afficher/masquer le panneau" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Raccourci clavier pour afficher ou masquer le panneau Zone Tools (comme le bouton en haut à gauche)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Afficher/masquer le panneau" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Mettre à jour la route" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Activer l’outil de mise à jour (routes existantes)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Les deux (par défaut)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Gauche" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Droite" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Aucun" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
