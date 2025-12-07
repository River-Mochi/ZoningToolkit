// Settings/LocaleEN.cs
// en-US localization for ZoneTools Options UI (About tab).

using System.Collections.Generic;
using Colossal; // IDictionarySource, IDictionaryEntryError
using Game.Settings;

namespace ZoningToolkit
{
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting _setting;

        public LocaleEN(Setting setting)
        {
            _setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title – like BuildingFixer: "ModName [Tag]"
                { _setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { _setting.GetOptionTabLocaleID(Setting.kAboutTab), "About" },

                // Groups
                { _setting.GetOptionGroupLocaleID(Setting.kAboutInfoGroup), "Info" },

                // About: ModName
                { _setting.GetOptionLabelLocaleID(nameof(Setting.ModName)), "Mod name" },
                { _setting.GetOptionDescLocaleID(nameof(Setting.ModName)),  "Display name of this mod." },

                // About: ModVersion
                { _setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Version" },
                { _setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Current ZoneTools version." },

                // About: Description
                { _setting.GetOptionLabelLocaleID(nameof(Setting.Description)), "Description" },
                {
                    _setting.GetOptionDescLocaleID(nameof(Setting.Description)),
                    "ZoneTools lets you enable or disable zoning on each side of roads (left, right, both, or none)."
                }
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; game manages localization sources.
        }
    }
}
