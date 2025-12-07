// Settings/Setting.cs
// Options UI layout for ZoneTools – buttons, toggles, sliders, dropdowns.

using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace ZoningToolkit
{
    // Saved settings file: ModsSettings/ZoneTools/ZoneTools.coc
    [FileLocation("ModsSettings/ZoneTools/ZoneTools")]
    [SettingsUIGroupOrder(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
    [SettingsUIShowGroupName(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";

        public const string kButtonGroup = "Button";
        public const string kToggleGroup = "Toggle";
        public const string kSliderGroup = "Slider";
        public const string kDropdownGroup = "Dropdown";

        public Setting(IMod mod) : base(mod)
        {
        }

        // ===== Buttons =====

        [SettingsUISection(kSection, kButtonGroup)]
        public bool Button
        {
            set
            {
                Mod.s_Log.Info("ZoneTools Setting: Button clicked");
            }
        }

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(kSection, kButtonGroup)]
        public bool ButtonWithConfirmation
        {
            set
            {
                Mod.s_Log.Info("ZoneTools Setting: ButtonWithConfirmation clicked");
            }
        }

        // ===== Toggle =====

        [SettingsUISection(kSection, kToggleGroup)]
        public bool Toggle
        {
            get; set;
        }

        // ===== Slider =====

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kDataMegabytes)]
        [SettingsUISection(kSection, kSliderGroup)]
        public int IntSlider
        {
            get; set;
        }

        // ===== Dropdowns =====

        [SettingsUIDropdown(typeof(Setting), nameof(GetIntDropdownItems))]
        [SettingsUISection(kSection, kDropdownGroup)]
        public int IntDropdown
        {
            get; set;
        }

        [SettingsUISection(kSection, kDropdownGroup)]
        public SomeEnum EnumDropdown { get; set; } = SomeEnum.Value1;

        public DropdownItem<int>[] GetIntDropdownItems()
        {
            var items = new List<DropdownItem<int>>();

            for (var i = 0; i < 3; i++)
            {
                items.Add(new DropdownItem<int>
                {
                    value = i,
                    displayName = i.ToString(),
                });
            }

            return items.ToArray();
        }

        public override void SetDefaults()
        {
            // Simple, safe defaults – no exception.
            Toggle = false;
            IntSlider = 50;
            IntDropdown = 0;
            EnumDropdown = SomeEnum.Value1;
        }

        public enum SomeEnum
        {
            Value1,
            Value2,
            Value3,
        }
    }
}
