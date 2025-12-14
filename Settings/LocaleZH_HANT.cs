// Settings/LocaleZH_HANT.cs
// Traditional Chinese zh-HANT for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleZH_HANT(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "關於" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "關於" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "按鍵綁定" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "模組名稱" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "此模組的顯示名稱。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "目前的 Zone Tools 版本。" },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "切換面板" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "用於顯示或隱藏 Zone Tools 面板的快捷鍵（與點擊左上角選單圖示相同）。"
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 切換面板" },
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
