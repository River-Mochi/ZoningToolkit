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
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "情報" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "操作" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "キー設定" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "リンク" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Mod名" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "このModの表示名です。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "バージョン" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Zone Tools の現在のバージョンです。" },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "作者の Paradox Mods ページを開きます。" },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "道路ツールで Zone Tools を自動表示" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "有効にすると、ゾーン可能な道路ツールを開いたときに Zone Tools パネルが自動で開きます。\n" +
                    "無効にすると手動で開きます。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "占有セルを保護（建物あり）" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "有効にすると、建物があるセルのゾーン深さ/範囲を変更しません。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "ゾーン済み（空き）セルを保護" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "有効にすると、すでにゾーンが塗られているセル（空きでも）を変更しません。"
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "パネル切り替え" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Zone Tools パネルを表示/非表示にするショートカット（左上のアイコンと同じ）。"
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – パネル切り替え" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "道路を更新" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "更新ツールを切り替え（既存の道路）。" },

                { "ZoneTools.UI.Tooltip.ModeDefault", "両方（デフォルト）" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "左" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "右" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "なし" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
