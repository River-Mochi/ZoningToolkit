// Systems/ZoneToolSystem.Keybind.cs
// Handles Zone Tools keybinding (Shift+Z by default) via CO's InputManager.

namespace ZoningToolkit.Systems
{
    using Game;
    using Game.Input;       // ProxyAction
    using Unity.Entities;   // GameSystemBase

    /// <summary>
    /// Runs in ToolUpdate and listens to the CO ProxyAction registered in Setting.RegisterKeyBindings().
    /// When the action is pressed, it toggles Zone Tools UI panel (same as clicking the GameTopLeft button).
    /// </summary>
    public sealed partial class ZoneToolSystemKeybind : GameSystemBase
    {
        private ZoneToolBridgeUI? m_UISystem;
        private bool m_LoggedMissingAction;
        private bool m_LoggedMissingUISystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_UISystem = World.GetOrCreateSystemManaged<ZoneToolBridgeUI>();
            Mod.s_Log.Info($"{Mod.ModTag} ZoneToolSystemKeybind created.");
        }

        protected override void OnUpdate()
        {
            if (m_UISystem == null)
            {
                if (!m_LoggedMissingUISystem)
                {
                    m_LoggedMissingUISystem = true;
                    Mod.s_Log.Warn($"{Mod.ModTag} ZoneToolSystemKeybind: UI system is null in OnUpdate (unexpected).");
                }

                return;
            }

            ProxyAction? togglePanelAction = Mod.TogglePanelAction;
            if (togglePanelAction == null)
            {
                if (!m_LoggedMissingAction)
                {
                    m_LoggedMissingAction = true;
                    Mod.s_Log.Warn($"{Mod.ModTag} ZoneToolSystemKeybind: TogglePanelAction is null in OnUpdate.");
                }

                return;
            }

            if (togglePanelAction.WasPressedThisFrame())
            {
                Mod.Debug($"{Mod.ModTag} ZoneToolSystemKeybind: toggle pressed -> toggling panel.");
                m_UISystem.TogglePanelFromHotkey();
            }
        }
    }
}
