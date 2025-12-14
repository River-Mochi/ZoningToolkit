// Settings/LocaleZH_CN.cs
// Simplified Chinese zh-HANS for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleZH_CN(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "关于" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "关于" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "键绑定" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "模组名称" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "此模组的显示名称。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "当前 Zone Tools 版本。" },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "切换面板" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "用于显示或隐藏 Zone Tools 面板的快捷键（与点击左上角菜单图标相同）。"
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 切换面板" },
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
