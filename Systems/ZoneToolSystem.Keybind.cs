// Systems/ZoneToolSystem.Keybind.cs
// Handles the Zone Tools keybinding (Shift+Z by default) via CO's InputManager.

namespace ZoningToolkit.Systems
{
    using Game;
    using Game.Input;
    using Unity.Entities;

    /// <summary>
    /// Runs in ToolUpdate and listens to the CO ProxyAction registered in Setting.RegisterKeyBindings().
    /// When the action is pressed, it toggles the Zone Tools UI panel (same as clicking the GameTopLeft button).
    /// </summary>
    public sealed partial class ZoneToolSystemKeybind : GameSystemBase
    {
        private ZoneToolBridgeUI m_UISystem = null!;

        protected override void OnCreate()
        {
            base.OnCreate();

            // UI bridge system that owns TogglePanelFromHotkey().
            m_UISystem = World.GetOrCreateSystemManaged<ZoneToolBridgeUI>();
        }

        protected override void OnUpdate()
        {
            // Use the CO ProxyAction that was registered from Setting.RegisterKeyBindings().
            ProxyAction? togglePanelAction = Mod.TogglePanelAction;
            if (togglePanelAction == null)
            {
                return;
            }

            // Rebindable via Options UI. Default is Shift+Z.
            if (togglePanelAction.WasPressedThisFrame())
            {
                m_UISystem.TogglePanelFromHotkey();
            }
        }
    }
}
