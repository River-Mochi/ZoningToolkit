// Settings/LocaleJA.cs
// Japanese ja-JP for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleJA : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleJA(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "情報" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "情報" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "キー割り当て" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "MOD名" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "このMODの表示名です。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "バージョン" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "現在の Zone Tools バージョンです。" },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "パネルの表示/非表示" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Zone Tools パネルを表示/非表示にするショートカット（左上のメニューアイコンをクリックするのと同じです）。"
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – パネルの表示/非表示" },
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
